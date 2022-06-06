// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Common.Extensions;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Common;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Services
{
    public class WorkflowExecuterService : IWorkflowExecuterService
    {
        private readonly ILogger<WorkflowExecuterService> _logger;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        private readonly IMessageBrokerPublisherService _messageBrokerPublisherService;
        private readonly IArtifactMapper _artifactMapper;
        private readonly IStorageService _storageService;
        private readonly StorageServiceConfiguration _storageConfiguration;

        private string TaskDispatchRoutingKey { get; }

        public WorkflowExecuterService(
            ILogger<WorkflowExecuterService> logger,
            IOptions<WorkflowManagerOptions> configuration,
            IOptions<StorageServiceConfiguration> storageConfiguration,
            IWorkflowRepository workflowRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IMessageBrokerPublisherService messageBrokerPublisherService,
            IArtifactMapper artifactMapper,
            IStorageService storageService)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (storageConfiguration is null)
            {
                throw new ArgumentNullException(nameof(storageConfiguration));
            }

            _storageConfiguration = storageConfiguration.Value;

            TaskDispatchRoutingKey = configuration.Value.Messaging.Topics.TaskDispatchRequest;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowRepository = workflowRepository ?? throw new ArgumentNullException(nameof(workflowRepository));
            _workflowInstanceRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
            _messageBrokerPublisherService = messageBrokerPublisherService ?? throw new ArgumentNullException(nameof(messageBrokerPublisherService));
            _artifactMapper = artifactMapper ?? throw new ArgumentNullException(nameof(artifactMapper));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        public async Task<bool> ProcessPayload(WorkflowRequestEvent message)
        {
            Guard.Against.Null(message, nameof(message));

            var processed = true;
            var workflows = new List<WorkflowRevision>();

            workflows = message.Workflows?.Any() != true ?
                await _workflowRepository.GetWorkflowsByAeTitleAsync(message.CalledAeTitle) as List<WorkflowRevision> :
                await _workflowRepository.GetByWorkflowsIdsAsync(message.Workflows) as List<WorkflowRevision>;

            if (workflows is null)
            {
                return false;
            }

            var workflowInstances = new List<WorkflowInstance>();

            workflows.ForEach(async (workflow) => workflowInstances.Add(await CreateWorkflowInstanceAsync(message, workflow)));

            var existingInstances = await _workflowInstanceRepository.GetByWorkflowsIdsAsync(workflowInstances.Select(w => w.WorkflowId).ToList());
            workflowInstances.RemoveAll(i => existingInstances.ToList().Exists(e => e.WorkflowId == i.WorkflowId && e.PayloadId == i.PayloadId));

            if (workflowInstances.Any())
            {
                processed &= await _workflowInstanceRepository.CreateAsync(workflowInstances);
            }

            workflowInstances.AddRange(existingInstances.Where(e => e.PayloadId == message.PayloadId.ToString()));

            if (!processed)
            {
                return false;
            }

            foreach (var workflowInstance in workflowInstances)
            {
                if (workflowInstance.Status == Status.Failed)
                {
                    continue;
                }

                var task = workflowInstance.Tasks.FirstOrDefault();

                if (task.Status != TaskExecutionStatus.Created)
                {
                    _logger.TaskPreviouslyDispatched(workflowInstance.PayloadId, task.TaskId);

                    continue;
                }

                processed &= await DispatchTask(workflowInstance, task, message.CorrelationId);
            }

            return processed;
        }

        public async Task<bool> ProcessTaskUpdate(TaskUpdateEvent message)
        {
            Guard.Against.Null(message, nameof(message));
            Guard.Against.Null(message.WorkflowInstanceId, nameof(message.WorkflowInstanceId));

            var workflowInstance = await _workflowInstanceRepository.GetByWorkflowInstanceIdAsync(message.WorkflowInstanceId);

            if (workflowInstance is null)
            {
                _logger.TypeNotFound(nameof(workflowInstance));

                return false;
            }

            var currentTask = workflowInstance.Tasks.FirstOrDefault(t => t.TaskId == message.TaskId);

            if (currentTask is null)
            {
                _logger.TaskNotFoundInWorkfowInstance(message.TaskId, message.WorkflowInstanceId);

                return false;
            }

            if (!message.Status.IsTaskExecutionStatusUpdateValid(currentTask.Status))
            {
                _logger.TaskStatusUpdateNotValid(workflowInstance.PayloadId, message.TaskId, currentTask.Status.ToString(), message.Status.ToString());

                return false;
            }

            var previouslyFailed = workflowInstance.Tasks.Any(t => t.Status == TaskExecutionStatus.Failed) || workflowInstance.Status == Status.Failed;

            if (message.Status != TaskExecutionStatus.Succeeded || previouslyFailed)
            {
                await UpdateWorkflowInstanceStatus(workflowInstance, message.TaskId, message.Status);

                return await _workflowInstanceRepository.UpdateTaskStatusAsync(message.WorkflowInstanceId, message.TaskId, message.Status);
            }

            var workflow = await _workflowRepository.GetByWorkflowIdAsync(workflowInstance.WorkflowId);

            if (workflow is null)
            {
                _logger.TypeNotFound(nameof(workflow));

                return false;
            }
            var artifactDict = message.Outputs.ToArtifactDictionary();

            var validOutputArtifacts = _storageService.VerifyObjectsExist(workflowInstance.BucketId, artifactDict);

            workflowInstance.Tasks?.ForEach(t =>
            {
                if (t.TaskId == message.TaskId)
                {
                    t.OutputArtifacts = validOutputArtifacts;
                }
            });

            var currentTaskDestinations = workflow.Workflow?.Tasks?.SingleOrDefault(t => t.Id == message.TaskId)?.TaskDestinations;
            var newTaskExecutions = await HandleTaskDestinations(workflowInstance, workflow, currentTaskDestinations);

            if (!newTaskExecutions.Any())
            {
                await UpdateWorkflowInstanceStatus(workflowInstance, message.TaskId, message.Status);

                await _workflowInstanceRepository.UpdateTaskOutputArtifactsAsync(workflowInstance.Id, message.TaskId, validOutputArtifacts);

                return await _workflowInstanceRepository.UpdateTaskStatusAsync(message.WorkflowInstanceId, message.TaskId, message.Status);
            }

            workflowInstance.Tasks.AddRange(newTaskExecutions);

            if (!await _workflowInstanceRepository.UpdateTasksAsync(message.WorkflowInstanceId, workflowInstance.Tasks))
            {
                return false;
            }

            var processed = true;

            foreach (var taskExec in newTaskExecutions)
            {
                processed &= await DispatchTask(workflowInstance, taskExec, message.CorrelationId);

                if (!processed)
                {
                    continue;
                }

                processed &= await _workflowInstanceRepository.UpdateTaskStatusAsync(message.WorkflowInstanceId, taskExec.TaskId, TaskExecutionStatus.Dispatched);
            }

            processed &= await _workflowInstanceRepository.UpdateTaskStatusAsync(message.WorkflowInstanceId, message.TaskId, message.Status);

            return processed;
        }

        private async Task<bool> UpdateWorkflowInstanceStatus(WorkflowInstance workflowInstance, string taskId, TaskExecutionStatus currentTaskStatus)
        {
            if (!workflowInstance.Tasks.Any(t => t.TaskId == taskId))
            {
                return false;
            }

            if (workflowInstance.Status == Status.Failed)
            {
                return true;
            }

            var previousTasks = workflowInstance.Tasks.Where(t => t.TaskId != taskId);

            if (previousTasks.Any(t => t.Status == TaskExecutionStatus.Failed) || currentTaskStatus == TaskExecutionStatus.Failed)
            {
                return await _workflowInstanceRepository.UpdateWorkflowInstanceStatusAsync(workflowInstance.Id, Status.Failed);
            }

            if (!previousTasks.Any(t => t.Status != TaskExecutionStatus.Succeeded && t.Status != TaskExecutionStatus.Canceled)
                && (currentTaskStatus == TaskExecutionStatus.Succeeded || currentTaskStatus == TaskExecutionStatus.Canceled))
            {
                return await _workflowInstanceRepository.UpdateWorkflowInstanceStatusAsync(workflowInstance.Id, Status.Succeeded);
            }

            return true;
        }

        private async Task<List<TaskExecution>> HandleTaskDestinations(WorkflowInstance workflowInstance, WorkflowRevision workflow, TaskDestination[]? currentTaskDestinations)
        {
            var newTaskExecutions = new List<TaskExecution>();

            if (currentTaskDestinations is null)
            {
                return newTaskExecutions;
            }

            foreach (var taskDest in currentTaskDestinations)
            {
                var existingTask = workflowInstance.Tasks.FirstOrDefault(t => t.TaskId == taskDest.Name);

                if (existingTask is not null
                    && existingTask.Status != TaskExecutionStatus.Created)
                {
                    _logger.TaskPreviouslyDispatched(workflowInstance.PayloadId, taskDest.Name);

                    continue;
                }

                var newTask = workflow.Workflow.Tasks.FirstOrDefault(t => t.Id == taskDest.Name);

                if (newTask is null)
                {
                    _logger.TaskNotFoundInWorkfow(workflowInstance.PayloadId, taskDest.Name, workflow.WorkflowId);

                    continue;
                }

                newTaskExecutions.Add(await CreateTaskExecutionAsync(newTask, workflowInstance.Id, workflowInstance.BucketId, workflowInstance.PayloadId));
            }

            return newTaskExecutions;
        }

        private async Task<bool> DispatchTask(WorkflowInstance workflowInstance, TaskExecution taskExec, string correlationId)
        {
            var taskDispatchEvent = EventMapper.ToTaskDispatchEvent(taskExec, workflowInstance.Id, correlationId, _storageConfiguration);
            var jsonMesssage = new JsonMessage<TaskDispatchEvent>(taskDispatchEvent, MessageBrokerConfiguration.WorkflowManagerApplicationId, taskDispatchEvent.CorrelationId, Guid.NewGuid().ToString());

            await _messageBrokerPublisherService.Publish(TaskDispatchRoutingKey, jsonMesssage.ToMessage());

            return await _workflowInstanceRepository.UpdateTaskStatusAsync(workflowInstance.Id, taskExec.TaskId, TaskExecutionStatus.Dispatched);
        }

        private async Task<WorkflowInstance> CreateWorkflowInstanceAsync(WorkflowRequestEvent message, WorkflowRevision workflow)
        {
            Guard.Against.Null(message, nameof(message));
            Guard.Against.Null(workflow, nameof(workflow));
            Guard.Against.Null(workflow.Workflow, nameof(workflow.Workflow));

            var workflowInstanceId = Guid.NewGuid().ToString();

            var workflowInstance = new WorkflowInstance()
            {
                Id = workflowInstanceId,
                WorkflowId = workflow.WorkflowId,
                PayloadId = message.PayloadId.ToString(),
                StartTime = DateTime.UtcNow,
                Status = Status.Created,
                AeTitle = workflow.Workflow?.InformaticsGateway?.AeTitle,
                BucketId = message.Bucket,
                InputMetaData = { } //Functionality to be added later
            };

            var tasks = new List<TaskExecution>();
            // part of this ticket just take the first task
            if (workflow.Workflow.Tasks.Length > 0)
            {
                var firstTask = workflow.Workflow.Tasks.First();

                // check if template exists merge args.

                //if (!string.IsNullOrEmpty(firstTask.Ref))
                //{
                //    var template = workflow.TaskTemplates.Where(x => x.Id == firstTask.Ref).FirstOrDefault();
                //    firstTask = template ?? firstTask;
                //}

                try
                {
                    tasks.Add(await CreateTaskExecutionAsync(firstTask, workflowInstance.Id, message.Bucket, message.PayloadId.ToString()));
                }
                catch (FileNotFoundException e)
                {
                    workflowInstance.Status = Status.Failed;
                }
            }

            workflowInstance.Tasks = tasks;

            return workflowInstance;
        }

        private async Task<TaskExecution> CreateTaskExecutionAsync(TaskObject task, string workflowInstanceId, string bucketName, string payloadId)
        {
            Guard.Against.Null(task, nameof(task));
            Guard.Against.NullOrWhiteSpace(task.Type, nameof(task.Type));
            Guard.Against.NullOrWhiteSpace(task.Id, nameof(task.Id));
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrWhiteSpace(bucketName, nameof(bucketName));
            Guard.Against.NullOrWhiteSpace(payloadId, nameof(payloadId));

            var executionId = Guid.NewGuid().ToString();

            return new TaskExecution()
            {
                ExecutionId = executionId,
                TaskType = task.Type,
                TaskPluginArguments = task.Args ?? new Dictionary<string, string> { },
                TaskId = task.Id,
                Status = TaskExecutionStatus.Created,
                InputArtifacts = await _artifactMapper.ConvertArtifactVariablesToPath(task?.Artifacts?.Input ?? new Artifact[] { }, payloadId, workflowInstanceId, bucketName),
                OutputDirectory = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}/",
                Metadata = { }
            };
        }
    }
}
