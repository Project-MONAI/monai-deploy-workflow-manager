/*
 * Copyright 2021-2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
using Monai.Deploy.WorkflowManager.ConditionsResolver.Extensions;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Parser;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Constants;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Logging.Logging;
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
        private readonly IPayloadService _payloadService;
        private readonly StorageServiceConfiguration _storageConfiguration;
        private readonly double _defaultTaskTimeoutMinutes;

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
            IPayloadService payloadService)
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
            _defaultTaskTimeoutMinutes = configuration.Value.TaskTimeoutMinutes;
            TaskDispatchRoutingKey = configuration.Value.Messaging.Topics.TaskDispatchRequest;
            ExportRequestRoutingKey = $"{configuration.Value.Messaging.Topics.ExportRequestPrefix}.{configuration.Value.Messaging.DicomAgents.ScuAgentName}";

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowRepository = workflowRepository ?? throw new ArgumentNullException(nameof(workflowRepository));
            _workflowInstanceRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
            _messageBrokerPublisherService = messageBrokerPublisherService ?? throw new ArgumentNullException(nameof(messageBrokerPublisherService));
            _conditionalParameterParser = conditionalParser ?? throw new ArgumentNullException(nameof(artifactMapper));
            _artifactMapper = artifactMapper ?? throw new ArgumentNullException(nameof(artifactMapper));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _payloadService = payloadService ?? throw new ArgumentNullException(nameof(payloadService));
        }

        public async Task<bool> ProcessPayload(WorkflowRequestEvent message, Payload payload)
        {
            Guard.Against.Null(message, nameof(message));

            using var loggerScope = _logger.BeginScope($"Correlation ID={message.CorrelationId}, Payload ID={payload.PayloadId}");
            var processed = true;
            var workflows = new List<WorkflowRevision>();

            if (message.Workflows?.Any() == true)
            {
                workflows = await _workflowRepository.GetByWorkflowsIdsAsync(message.Workflows) as List<WorkflowRevision>;
            }
            else
            {
                var aeTitles = new List<string>
                {
                    message.CalledAeTitle,
                    message.CallingAeTitle
                };

                workflows = await _workflowRepository.GetWorkflowsByAeTitleAsync(aeTitles) as List<WorkflowRevision>;
            }

            if (workflows is null || workflows.Any() is false)
            {
                _logger.NoMatchingWorkflowFoundForPayload();
                return false;
            }

            var workflowInstances = new List<WorkflowInstance>();

            var tasks = workflows.Select(workflow => CreateWorkflowInstanceAsync(message, workflow));
            var newInstances = await Task.WhenAll(tasks).ConfigureAwait(false);
            workflowInstances.AddRange(newInstances);

            var existingInstances = await _workflowInstanceRepository.GetByWorkflowsIdsAsync(workflowInstances.Select(w => w.WorkflowId).ToList());

            workflowInstances.RemoveAll(i => existingInstances.Any(e => e.WorkflowId == i.WorkflowId
                                                                           && e.PayloadId == i.PayloadId));
            if (workflowInstances.Any())
            {
                processed &= await _workflowInstanceRepository.CreateAsync(workflowInstances);

                var workflowInstanceIds = workflowInstances.Select(workflowInstance => workflowInstance.Id);
                await _payloadService.UpdateWorkflowInstanceIdsAsync(payload.Id, workflowInstanceIds).ConfigureAwait(false);
            }

            workflowInstances.AddRange(existingInstances.Where(e => e.PayloadId == message.PayloadId.ToString()));

            if (!processed)
            {
                _logger.LogWarning("!processed");
                return false;
            }

            foreach (var workflowInstance in workflowInstances)
            {
                await ProcessFirstWorkflowTask(workflowInstance, message.CorrelationId);
            }

            return true;
        }

        public async Task ProcessFirstWorkflowTask(WorkflowInstance workflowInstance, string correlationId)
        {
            if (workflowInstance.Status == Status.Failed)
            {
                _logger.WorkflowBadStatus(workflowInstance.WorkflowId, workflowInstance.Status.ToString());
                return;
            }

            var task = workflowInstance.Tasks.FirstOrDefault();

            var workflow = await _workflowRepository.GetByWorkflowIdAsync(workflowInstance.WorkflowId);

            if (task is null || workflow is null)
            {
                return;
            }

            if (string.Equals(task.TaskType, TaskTypeConstants.RouterTask, StringComparison.InvariantCultureIgnoreCase))
            {
                await HandleTaskDestinations(workflowInstance, workflow, task, correlationId);

                return;
            }

            if (string.Equals(task.TaskType, TaskTypeConstants.ExportTask, StringComparison.InvariantCultureIgnoreCase))
            {
                await HandleDicomExportAsync(workflow, workflowInstance, task, correlationId);

                return;
            }

            if (task.Status != TaskExecutionStatus.Created)
            {
                _logger.TaskPreviouslyDispatched(workflowInstance.PayloadId, task.TaskId);

                return;
            }

            await DispatchTask(workflowInstance, workflow, task, correlationId);
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

            if (message.Reason == FailureReason.TimedOut && currentTask.Status == TaskExecutionStatus.Failed)
            {
                _logger.TaskTimedOut(message.TaskId, message.WorkflowInstanceId, currentTask.Timeout);

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

            if (message.Reason != FailureReason.None)
            {
                if (message.ExecutionStats?.IsNullOrEmpty() is false)
                {
                    currentTask.ExecutionStats.Append(message.ExecutionStats);
                }
                currentTask.Reason = message.Reason;
                await _workflowInstanceRepository.UpdateTaskAsync(workflowInstance.Id, currentTask.TaskId, currentTask);
            }

            var previouslyFailed = workflowInstance.Tasks.Any(t => t.Status == TaskExecutionStatus.Failed) || workflowInstance.Status == Status.Failed;

            if (message.Status == TaskExecutionStatus.Failed || message.Status == TaskExecutionStatus.Canceled || previouslyFailed)
            {
                await UpdateWorkflowInstanceStatus(workflowInstance, message.TaskId, message.Status);

                return await CompleteTask(currentTask, workflowInstance, message.CorrelationId, message.Status);
            }

            if (message.Status != TaskExecutionStatus.Succeeded)
            {
                return await _workflowInstanceRepository.UpdateTaskStatusAsync(workflowInstance.Id, message.TaskId, message.Status);
            }

            if (message.Metadata.Any())
            {
                currentTask.ResultMetadata = message.Metadata;
                await _workflowInstanceRepository.UpdateTaskAsync(workflowInstance.Id, currentTask.TaskId, currentTask);
            }

            var isValid = await HandleOutputArtifacts(workflowInstance, message.Outputs, currentTask, workflow);

            if (isValid is false)
            {
                await UpdateWorkflowInstanceStatus(workflowInstance, message.TaskId, TaskExecutionStatus.Failed);

                return await CompleteTask(currentTask, workflowInstance, message.CorrelationId, TaskExecutionStatus.Failed);
            }

            return await HandleTaskDestinations(workflowInstance, workflow, currentTask, message.CorrelationId);
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

                return await HandleTaskDestinations(workflowInstance, workflow, task, correlationId);
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

        private async Task HandleDicomExportAsync(WorkflowRevision workflow, WorkflowInstance workflowInstance, TaskExecution task, string correlationId)
        {
            var exportList = workflow.Workflow?.Tasks?.FirstOrDefault(t => t.Id == task.TaskId)?.ExportDestinations.Select(e => e.Name).ToArray();

            var artifactValues = GetDicomExports(workflow, workflowInstance, task, exportList);

            var files = new List<VirtualFileInfo>();
            foreach (var artifact in artifactValues)
            {
                if (artifact is not null)
                {
                    var objects = await _storageService.ListObjectsAsync(
                        workflowInstance.BucketId,
                        artifact,
                        true);

                    var dcmFiles = objects?.Where(o => o.IsValidDicomFile())?.ToList();

                    if (dcmFiles?.IsNullOrEmpty() is false)
                    {
                        files.AddRange(dcmFiles.ToList());
                    }
                }
            }

            artifactValues = files.Select(f => f.FilePath).ToArray();

            if (artifactValues.IsNullOrEmpty())
            {
                _logger.ExportFilesNotFound(task.TaskId, workflowInstance.Id);

                await UpdateWorkflowInstanceStatus(workflowInstance, task.TaskId, TaskExecutionStatus.Failed);

                await CompleteTask(task, workflowInstance, correlationId, TaskExecutionStatus.Failed);

                return;
            }

            await DispatchDicomExport(workflowInstance, task, exportList, artifactValues, correlationId);
        }

        private string[] GetDicomExports(WorkflowRevision workflow, WorkflowInstance workflowInstance, TaskExecution task, string[]? exportDestinations)
        {
            var validExportDestinations = workflow.Workflow?.InformaticsGateway?.ExportDestinations;

            if (exportDestinations is null
                || exportDestinations.IsNullOrEmpty()
                || validExportDestinations is null
                || validExportDestinations.IsNullOrEmpty())
            {
                return Array.Empty<string>();
            }

            foreach (var exportDestination in exportDestinations)
            {
                if (validExportDestinations.Contains(exportDestination) is false)
                {
                    return Array.Empty<string>();
                }
            }

            if (!task.InputArtifacts.Any())
            {
                return Array.Empty<string>();
            }

            return new List<string>(task.InputArtifacts.Values).ToArray();
        }

        private async Task<bool> DispatchDicomExport(WorkflowInstance workflowInstance, TaskExecution task, string[]? exportDestinations, string[] artifactValues, string correlationId)
        {
            if (exportDestinations is null || !exportDestinations.Any())
            {
                return false;
            }

            await ExportRequest(workflowInstance, task, exportDestinations, artifactValues, correlationId);
            return await _workflowInstanceRepository.UpdateTaskStatusAsync(workflowInstance.Id, task.TaskId, TaskExecutionStatus.Dispatched);
        }

        private async Task<bool> HandleOutputArtifacts(WorkflowInstance workflowInstance, List<Messaging.Common.Storage> outputs, TaskExecution task, WorkflowRevision workflowRevision)
        {
            var artifactDict = outputs.ToArtifactDictionary();

            var validOutputArtifacts = await _storageService.VerifyObjectsExistAsync(workflowInstance.BucketId, artifactDict);

            var revisionTask = workflowRevision.Workflow?.Tasks.FirstOrDefault(t => t.Id == task.TaskId);

            if (revisionTask is null)
            {
                _logger.TaskNotFoundInWorkfow(workflowInstance.PayloadId, task.TaskId, workflowRevision.WorkflowId);

                return false;
            }

            foreach (var artifact in artifactDict)
            {
                var outputArtifact = revisionTask.Artifacts.Output.FirstOrDefault(o => o.Name == artifact.Key);
                _logger.LogArtifactPassing(new Artifact { Name = artifact.Key, Value = artifact.Value, Mandatory = outputArtifact?.Mandatory ?? true }, artifact.Value, "Post-Task Output Artifact", validOutputArtifacts.ContainsKey(artifact.Key));
            }

            var missingOutputs = revisionTask.Artifacts.Output.Where(o => validOutputArtifacts.ContainsKey(o.Name) is false);

            if (missingOutputs.Any(o => o.Mandatory))
            {
                _logger.MandatoryOutputArtefactsMissingForTask(task.TaskId);

                return false;
            }

            var currentTask = workflowInstance.Tasks?.FirstOrDefault(t => t.TaskId == task.TaskId);

            if (currentTask is not null)
            {
                currentTask.OutputArtifacts = validOutputArtifacts;
            }

            if (validOutputArtifacts is not null && validOutputArtifacts.Any())
            {
                return await _workflowInstanceRepository.UpdateTaskOutputArtifactsAsync(workflowInstance.Id, task.TaskId, validOutputArtifacts);
            }

            return true;
        }

        private async Task<bool> DispatchTaskDestinations(WorkflowInstance workflowInstance, WorkflowRevision workflow, string correlationId, IList<TaskExecution> taskExecutions)
        {
            workflowInstance.Tasks.AddRange(taskExecutions);

            if (!await _workflowInstanceRepository.UpdateTasksAsync(workflowInstance.Id, workflowInstance.Tasks))
            {
                return false;
            }

            var processed = true;

            foreach (var taskExec in taskExecutions)
            {
                if (string.Equals(taskExec.TaskType, TaskTypeConstants.RouterTask, StringComparison.InvariantCultureIgnoreCase))
                {
                    await HandleTaskDestinations(workflowInstance, workflow, taskExec, correlationId);

                    continue;
                }

                if (string.Equals(taskExec.TaskType, TaskTypeConstants.ExportTask, StringComparison.InvariantCultureIgnoreCase))
                {
                    await HandleDicomExportAsync(workflow, workflowInstance, taskExec, correlationId);

                    continue;
                }

                processed &= await DispatchTask(workflowInstance, workflow, taskExec, correlationId);

                if (processed is false)
                {
                    continue;
                }

                processed &= await _workflowInstanceRepository.UpdateTaskStatusAsync(workflowInstance.Id, taskExec.TaskId, TaskExecutionStatus.Dispatched);
            }

            return processed;
        }

        private async Task<bool> HandleTaskDestinations(WorkflowInstance workflowInstance, WorkflowRevision workflow, TaskExecution task, string correlationId)
        {
            var newTaskExecutions = await CreateTaskDestinations(workflowInstance, workflow, task.TaskId);

            if (newTaskExecutions.Any() is false)
            {
                await UpdateWorkflowInstanceStatus(workflowInstance, task.TaskId, TaskExecutionStatus.Succeeded);

                return await CompleteTask(task, workflowInstance, correlationId, TaskExecutionStatus.Succeeded);
            }

            await DispatchTaskDestinations(workflowInstance, workflow, correlationId, newTaskExecutions);

            return await CompleteTask(task, workflowInstance, correlationId, TaskExecutionStatus.Succeeded);
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
                if (taskDest.Conditions.IsNullOrEmpty() is false
                    && taskDest.Conditions.Any(c => string.IsNullOrWhiteSpace(c) is false)
                    && _conditionalParameterParser.TryParse(taskDest.Conditions, workflowInstance) is false)
                {
                    _logger.TaskDestinationConditionFalse(taskDest.Conditions.CombineConditionString(), taskDest.Name);

                    continue;
                }

                var existingTask = workflowInstance.Tasks.FirstOrDefault(t => t.TaskId == taskDest.Name);

                if (existingTask is not null
                    && existingTask.Status != TaskExecutionStatus.Created)
                {
                    _logger.TaskPreviouslyDispatched(workflowInstance.PayloadId, taskDest.Name);

                    continue;
                }

                var newTask = workflow?.Workflow?.Tasks.FirstOrDefault(t => t.Id == taskDest.Name);

                if (newTask is null)
                {
                    _logger.TaskNotFoundInWorkfow(workflowInstance.PayloadId, taskDest.Name, workflow?.WorkflowId);

                    continue;
                }

                newTaskExecutions.Add(await CreateTaskExecutionAsync(newTask, workflowInstance, null, null, taskId));
            }

            return newTaskExecutions;
        }

        private async Task<bool> DispatchTask(WorkflowInstance workflowInstance, WorkflowRevision workflow, TaskExecution taskExec, string correlationId)
        {
            var task = workflow?.Workflow?.Tasks?.FirstOrDefault(t => t.Id == taskExec.TaskId);

            if (task is null)
            {
                return false;
            }

            using (_logger.BeginScope(new Dictionary<string, object> { ["correlationId"] = correlationId, ["taskId"] = task.Id }))
            {
                var outputArtifacts = task.Artifacts?.Output;

                if (outputArtifacts is not null && outputArtifacts.Any())
                {
                    foreach (var artifact in outputArtifacts)
                    {
                        if (!string.IsNullOrWhiteSpace(artifact.Value))
                        {
                            continue;
                        }

                        artifact.Value = $"{{ context.executions.{task.Id}.output_dir }}/{artifact.Name}";
                    }
                }

                var pathOutputArtifacts = await _artifactMapper.ConvertArtifactVariablesToPath(outputArtifacts ?? Array.Empty<Artifact>(), workflowInstance.PayloadId, workflowInstance.Id, workflowInstance.BucketId, false);

                var taskDispatchEvent = EventMapper.ToTaskDispatchEvent(taskExec, workflowInstance, pathOutputArtifacts, correlationId, _storageConfiguration);
                var jsonMesssage = new JsonMessage<TaskDispatchEvent>(taskDispatchEvent, MessageBrokerConfiguration.WorkflowManagerApplicationId, taskDispatchEvent.CorrelationId, Guid.NewGuid().ToString());

                await _messageBrokerPublisherService.Publish(TaskDispatchRoutingKey, jsonMesssage.ToMessage());

                return await _workflowInstanceRepository.UpdateTaskStatusAsync(workflowInstance.Id, taskExec.TaskId, TaskExecutionStatus.Dispatched);
            }
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
                WorkflowName = workflow.Workflow.Name,
                PayloadId = message.PayloadId.ToString(),
                StartTime = DateTime.UtcNow,
                Status = Status.Created,
                AeTitle = workflow.Workflow?.InformaticsGateway?.AeTitle,
                BucketId = message.Bucket,
                InputMetaData = { } //Functionality to be added later
            };

            var tasks = new List<TaskExecution>();
            // part of this ticket just take the first task
            if (workflow?.Workflow?.Tasks.Length > 0)
            {
                var firstTask = workflow.Workflow.Tasks.First();

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

        public async Task<TaskExecution> CreateTaskExecutionAsync(TaskObject task,
                                                  WorkflowInstance workflowInstance,
                                                  string? bucketName = null,
                                                  string? payloadId = null,
                                                  string? previousTaskId = null)
        {
            Guard.Against.Null(workflowInstance);

            var workflowInstanceId = workflowInstance.Id;

            bucketName ??= workflowInstance.BucketId;

            payloadId ??= workflowInstance.PayloadId;

            Guard.Against.Null(task);
            Guard.Against.NullOrWhiteSpace(task.Type);
            Guard.Against.NullOrWhiteSpace(task.Id);
            Guard.Against.NullOrWhiteSpace(workflowInstanceId);
            Guard.Against.NullOrWhiteSpace(bucketName);
            Guard.Against.NullOrWhiteSpace(payloadId);

            var executionId = Guid.NewGuid().ToString();
            var newInputParameters = GetInputParameters(task, workflowInstance);
            var newTaskArgs = GetTaskArgs(task, workflowInstance);

            if (task.TimeoutMinutes == -1)
            {
                task.TimeoutMinutes = _defaultTaskTimeoutMinutes;
            }

            return new TaskExecution()
            {
                ExecutionId = executionId,
                TaskType = task.Type,
                TaskPluginArguments = newTaskArgs,
                TaskStartTime = DateTime.UtcNow,
                TaskId = task.Id,
                WorkflowInstanceId = workflowInstanceId,
                Status = TaskExecutionStatus.Created,
                Reason = FailureReason.None,
                InputArtifacts = await _artifactMapper.ConvertArtifactVariablesToPath(task?.Artifacts?.Input ?? Array.Empty<Artifact>(), payloadId, workflowInstanceId, bucketName),
                OutputDirectory = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}",
                ResultMetadata = { },
                InputParameters = newInputParameters,
                PreviousTaskId = previousTaskId ?? string.Empty,
                TimeoutInterval = task?.TimeoutMinutes ?? _defaultTaskTimeoutMinutes,
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
