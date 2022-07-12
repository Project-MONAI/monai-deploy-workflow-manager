// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Storage.API;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Common.Extensions;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using Monai.Deploy.WorkflowManager.Storage.Services;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Common;

namespace Monai.Deploy.WorkflowManager.WorkfowExecuter.Services
{
    public class WorkflowExecuterService : IWorkflowExecuterService
    {
        private readonly ILogger<WorkflowExecuterService> _logger;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        private readonly IMessageBrokerPublisherService _messageBrokerPublisherService;
        private readonly IConditionalParameterParser _conditionalParameterParser;
        private readonly IArtifactMapper _artifactMapper;
        private readonly IStorageService _storageService;
        private readonly IDicomService _dicomService;
        private readonly IPayloadService _payloadService;
        private readonly StorageServiceConfiguration _storageConfiguration;

        private string TaskDispatchRoutingKey { get; }
        private string ExportRequestRoutingKey { get; }

        public WorkflowExecuterService(
            ILogger<WorkflowExecuterService> logger,
            IOptions<WorkflowManagerOptions> configuration,
            IOptions<StorageServiceConfiguration> storageConfiguration,
            IWorkflowRepository workflowRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IMessageBrokerPublisherService messageBrokerPublisherService,
            IConditionalParameterParser conditionalParser,
            IArtifactMapper artifactMapper,
            IStorageService storageService,
            IPayloadService payloadService,
            IDicomService dicomService)
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
            ExportRequestRoutingKey = $"{configuration.Value.Messaging.Topics.ExportRequestPrefix}.{configuration.Value.Messaging.DicomAgents.DicomWebAgentName}";

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowRepository = workflowRepository ?? throw new ArgumentNullException(nameof(workflowRepository));
            _workflowInstanceRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
            _messageBrokerPublisherService = messageBrokerPublisherService ?? throw new ArgumentNullException(nameof(messageBrokerPublisherService));
            _conditionalParameterParser = conditionalParser ?? throw new ArgumentNullException(nameof(artifactMapper));
            _artifactMapper = artifactMapper ?? throw new ArgumentNullException(nameof(artifactMapper));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _dicomService = dicomService ?? throw new ArgumentNullException(nameof(dicomService));
            _payloadService = payloadService ?? throw new ArgumentNullException(nameof(payloadService));
        }

        public async Task<bool> ProcessPayload(WorkflowRequestEvent message, Payload payload)
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

            workflows.ForEach(async (workflow) =>
            {
                var workflowInstance = await CreateWorkflowInstanceAsync(message, workflow);
                workflowInstances.Add(workflowInstance);
            });

            var existingInstances = await _workflowInstanceRepository.GetByWorkflowsIdsAsync(workflowInstances.Select(w => w.WorkflowId).ToList());

            workflowInstances.RemoveAll(i => existingInstances.Any(e => e.WorkflowId == i.WorkflowId
                                                                           && e.PayloadId == i.PayloadId));
            if (workflowInstances.Any())
            {
                processed &= await _workflowInstanceRepository.CreateAsync(workflowInstances);

                var workflowInstanceIds = workflowInstances.Select(workflowInstance => workflowInstance.WorkflowId);
                await _payloadService.UpdateWorkflowInstanceIdsAsync(payload.Id, workflowInstanceIds).ConfigureAwait(false);
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

                if (task is null)
                {
                    continue;
                }

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

            var workflow = await _workflowRepository.GetByWorkflowIdAsync(workflowInstance.WorkflowId);

            if (workflow is null)
            {
                _logger.TypeNotFound(nameof(workflow));

                return false;
            }

            if (!message.Status.IsTaskExecutionStatusUpdateValid(currentTask.Status))
            {
                _logger.TaskStatusUpdateNotValid(workflowInstance.PayloadId, message.TaskId, currentTask.Status.ToString(), message.Status.ToString());

                return false;
            }

            if (message.ExecutionStats is not null)
            {
                currentTask.ExecutionStats = message.ExecutionStats;
                await _workflowInstanceRepository.UpdateTaskAsync(workflowInstance.Id, currentTask.TaskId, currentTask);
            }

            var previouslyFailed = workflowInstance.Tasks.Any(t => t.Status == TaskExecutionStatus.Failed) || workflowInstance.Status == Status.Failed;

            if (message.Status != TaskExecutionStatus.Succeeded || previouslyFailed)
            {
                await UpdateWorkflowInstanceStatus(workflowInstance, message.TaskId, message.Status);

                return await CompleteTask(currentTask, workflowInstance, message.CorrelationId, message.Status);
            }

            if (message.Metadata.Any())
            {
                currentTask.Metadata = message.Metadata;
            }

            await HandleOutputArtifacts(workflowInstance, message.Outputs, currentTask);

            var exportDestinations = workflow.Workflow?.InformaticsGateway?.ExportDestinations;

            if (exportDestinations is not null && exportDestinations.Any())
            {
                var dicomImages = (await _dicomService.GetDicomPathsForTaskAsync(currentTask.OutputDirectory, workflowInstance.BucketId))?.ToList();

                if (dicomImages is not null && dicomImages.Any())
                {
                    return await HandleDicomExport(workflowInstance, currentTask, exportDestinations, dicomImages, message.CorrelationId);
                }

                _logger.ExportFilesNotFound(currentTask.TaskId, workflowInstance.Id);
            }

            var newTaskExecutions = await CreateTaskDestinations(workflowInstance, workflow, message.TaskId);

            if (!newTaskExecutions.Any())
            {
                await UpdateWorkflowInstanceStatus(workflowInstance, message.TaskId, message.Status);

                return await CompleteTask(currentTask, workflowInstance, message.CorrelationId, message.Status);
            }

            var processed = await HandleTaskDestinations(workflowInstance, message.CorrelationId, newTaskExecutions);

            processed &= await CompleteTask(currentTask, workflowInstance, message.CorrelationId, message.Status);

            return processed;
        }

        public async Task<bool> ProcessExportComplete(ExportCompleteEvent message, string correlationId)
        {
            var workflowInstance = await _workflowInstanceRepository.GetByWorkflowInstanceIdAsync(message.WorkflowInstanceId);
            var task = workflowInstance.Tasks.FirstOrDefault(t => t.TaskId == message.ExportTaskId);

            if (task is null)
            {
                return false;
            }

            if (message.Status.Equals(ExportStatus.Success)
                && TaskExecutionStatus.Succeeded.IsTaskExecutionStatusUpdateValid(task.Status))
            {
                var workflow = await _workflowRepository.GetByWorkflowIdAsync(workflowInstance.WorkflowId);

                if (workflow is null)
                {
                    _logger.TypeNotFound(nameof(workflow));

                    return false;
                }

                var newTaskExecutions = await CreateTaskDestinations(workflowInstance, workflow, task.TaskId);

                if (!newTaskExecutions.Any())
                {
                    await UpdateWorkflowInstanceStatus(workflowInstance, task.TaskId, TaskExecutionStatus.Succeeded);

                    return await CompleteTask(task, workflowInstance, correlationId, TaskExecutionStatus.Succeeded);
                }

                var processed = await HandleTaskDestinations(workflowInstance, correlationId, newTaskExecutions);

                processed &= await CompleteTask(task, workflowInstance, correlationId, TaskExecutionStatus.Succeeded);

                return processed;
            }

            if ((message.Status.Equals(ExportStatus.Failure) || message.Status.Equals(ExportStatus.PartialFailure)) &&
                TaskExecutionStatus.Failed.IsTaskExecutionStatusUpdateValid(task.Status))
            {
                return await CompleteTask(task, workflowInstance, correlationId, TaskExecutionStatus.Failed);
            }

            return true;
        }

        private async Task<bool> CompleteTask(TaskExecution task, WorkflowInstance workflowInstance, string correlationId, TaskExecutionStatus status)
        {
            var payload = await _payloadService.GetByIdAsync(workflowInstance.PayloadId);
            _logger.TaskComplete(task, workflowInstance, payload?.PatientDetails, correlationId, status.ToString());

            return await _workflowInstanceRepository.UpdateTaskStatusAsync(workflowInstance.Id, task.TaskId, status);
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

        private async Task<bool> HandleDicomExport(WorkflowInstance workflowInstance, TaskExecution task, string[] exportDestinations, IList<string> dicomImages, string correlationId)
        {
            var processed = true;

            if (dicomImages is null || !dicomImages.Any())
            {
                return processed;
            }

            processed &= await ExportRequest(workflowInstance, task, exportDestinations, dicomImages, correlationId);
            processed &= await _workflowInstanceRepository.UpdateTaskStatusAsync(workflowInstance.Id, task.TaskId, TaskExecutionStatus.Exported);

            return processed;
        }

        private async Task<bool> HandleOutputArtifacts(WorkflowInstance workflowInstance, List<Messaging.Common.Storage> outputs, TaskExecution task)
        {
            var artifactDict = outputs.ToArtifactDictionary();

            var validOutputArtifacts = await _storageService.VerifyObjectsExistAsync(workflowInstance.BucketId, artifactDict);

            workflowInstance.Tasks?.ForEach(t =>
            {
                if (t.TaskId == task.TaskId)
                {
                    t.OutputArtifacts = validOutputArtifacts;
                }
            });

            if (validOutputArtifacts is not null && validOutputArtifacts.Any())
            {
                return await _workflowInstanceRepository.UpdateTaskOutputArtifactsAsync(workflowInstance.Id, task.TaskId, validOutputArtifacts);
            }

            return true;
        }

        private async Task<bool> HandleTaskDestinations(WorkflowInstance workflowInstance, string correlationId, IList<TaskExecution> taskExecutions)
        {
            workflowInstance.Tasks?.AddRange(taskExecutions);

            if (!await _workflowInstanceRepository.UpdateTasksAsync(workflowInstance.Id, workflowInstance.Tasks))
            {
                return false;
            }

            var processed = true;

            foreach (var taskExec in taskExecutions)
            {
                processed &= await DispatchTask(workflowInstance, taskExec, correlationId);

                if (!processed)
                {
                    continue;
                }

                processed &= await _workflowInstanceRepository.UpdateTaskStatusAsync(workflowInstance.Id, taskExec.TaskId, TaskExecutionStatus.Dispatched);
            }

            return processed;
        }

        private async Task<List<TaskExecution>> CreateTaskDestinations(WorkflowInstance workflowInstance, WorkflowRevision workflow, string taskId)
        {
            var currentTaskDestinations = workflow.Workflow?.Tasks?.SingleOrDefault(t => t.Id == taskId)?.TaskDestinations;

            var newTaskExecutions = new List<TaskExecution>();

            if (currentTaskDestinations is null)
            {
                return newTaskExecutions;
            }

            foreach (var taskDest in currentTaskDestinations)
            {
                //Evaluate Conditional
                if (!string.IsNullOrEmpty(taskDest.Conditions)
                    && taskDest.Conditions != string.Empty
                    && !_conditionalParameterParser.TryParse(taskDest.Conditions, workflowInstance))
                {
                    continue;
                }

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

                newTaskExecutions.Add(await CreateTaskExecutionAsync(newTask, workflowInstance));
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

        private async Task<bool> ExportRequest(WorkflowInstance workflowInstance, TaskExecution taskExec, string[] exportDestinations, IList<string> dicomImages, string correlationId)
        {
            var exportRequestEvent = EventMapper.ToExportRequestEvent(dicomImages, exportDestinations, taskExec.TaskId, workflowInstance.Id, correlationId);
            var jsonMesssage = new JsonMessage<ExportRequestEvent>(exportRequestEvent, MessageBrokerConfiguration.WorkflowManagerApplicationId, exportRequestEvent.CorrelationId, Guid.NewGuid().ToString());

            await _messageBrokerPublisherService.Publish(ExportRequestRoutingKey, jsonMesssage.ToMessage());
            return true;
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
                    tasks.Add(await CreateTaskExecutionAsync(firstTask, workflowInstance, message.Bucket, message.PayloadId.ToString()));
                }
                catch (FileNotFoundException)
                {
                    workflowInstance.Status = Status.Failed;
                }
            }

            workflowInstance.Tasks = tasks;

            return workflowInstance;
        }

        private async Task<TaskExecution> CreateTaskExecutionAsync(TaskObject task,
                                                  WorkflowInstance workflowInstance,
                                                  string? bucketName = null,
                                                  string? payloadId = null)
        {
            Guard.Against.Null(workflowInstance, nameof(workflowInstance));

            var workflowInstanceId = workflowInstance.Id;
            if (bucketName is null)
            {
                bucketName = workflowInstance.BucketId;
            }
            if (payloadId is null)
            {
                payloadId = workflowInstance.PayloadId;
            }

            Guard.Against.Null(task, nameof(task));
            Guard.Against.NullOrWhiteSpace(task.Type, nameof(task.Type));
            Guard.Against.NullOrWhiteSpace(task.Id, nameof(task.Id));
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrWhiteSpace(bucketName, nameof(bucketName));
            Guard.Against.NullOrWhiteSpace(payloadId, nameof(payloadId));

            var executionId = Guid.NewGuid().ToString();
            var newInputParameters = GetInputParameters(task, workflowInstance);
            var newTaskArgs = GetTaskArgs(task, workflowInstance);

            return new TaskExecution()
            {
                ExecutionId = executionId,
                TaskType = task.Type,
                TaskPluginArguments = newTaskArgs,
                TaskStartTime = DateTime.UtcNow,
                TaskId = task.Id,
                Status = TaskExecutionStatus.Created,
                InputArtifacts = await _artifactMapper.ConvertArtifactVariablesToPath(task?.Artifacts?.Input ?? new Artifact[] { }, payloadId, workflowInstanceId, bucketName),
                OutputDirectory = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}/",
                Metadata = { },
                InputParameters = newInputParameters
            };
        }

        /// <summary>
        /// Gets and resolves input parameters
        /// </summary>
        /// <param name="task"></param>
        /// <param name="workflowInstance"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetInputParameters(TaskObject task,
                                                              WorkflowInstance workflowInstance)
        {
            var newInputParameters = new Dictionary<string, object>();
            if (task.InputParameters is not null)
            {
                foreach (var item in task.InputParameters)
                {
                    var newValue = item.Value;
                    if (item.Value is string itemValueString)
                    {
                        newValue = _conditionalParameterParser.ResolveParameters(itemValueString, workflowInstance);
                    }
                    newInputParameters.Add(item.Key, newValue);
                }
            }
            return newInputParameters;
        }

        /// <summary>
        /// Gets and resolves task arguments
        /// </summary>
        /// <param name="task"></param>
        /// <param name="workflowInstance"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetTaskArgs(TaskObject task,
                                                              WorkflowInstance workflowInstance)
        {
            var newArgs = new Dictionary<string, string>();
            if (task.Args is not null)
            {
                foreach (var item in task.Args)
                {
                    var newValue = item.Value;
                    if (item.Value is string itemValueString)
                    {
                        newValue = _conditionalParameterParser.ResolveParameters(itemValueString, workflowInstance);
                    }
                    newArgs.Add(item.Key, newValue);
                }
            }
            return newArgs;
        }
    }
}
