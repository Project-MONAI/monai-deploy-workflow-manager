/*
 * Copyright 2022 MONAI Consortium
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

using System.Globalization;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Monai.Deploy.Storage.API;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Extensions;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Extensions;
using Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Parser;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Contracts.Constants;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Logging;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Common;
using Monai.Deploy.WorkloadManager.WorkflowExecuter.Extensions;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Services
{
    public class WorkflowExecuterService : IWorkflowExecuterService
    {
        private readonly ILogger<WorkflowExecuterService> _logger;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        private readonly IWorkflowInstanceService _workflowInstanceService;
        private readonly IMessageBrokerPublisherService _messageBrokerPublisherService;
        private readonly IConditionalParameterParser _conditionalParameterParser;
        private readonly ITaskExecutionStatsRepository _taskExecutionStatsRepository;
        private readonly List<string> _migExternalAppPlugins;
        private readonly IArtifactMapper _artifactMapper;
        private readonly IStorageService _storageService;
        private readonly IPayloadService _payloadService;
        private readonly StorageServiceConfiguration _storageConfiguration;
        private readonly double _defaultTaskTimeoutMinutes;
        private readonly Dictionary<string, double> _defaultPerTaskTypeTimeoutMinutes = new Dictionary<string, double>();

        private string TaskDispatchRoutingKey { get; }
        private string ExportRequestRoutingKey { get; }
        private string ClinicalReviewTimeoutRoutingKey { get; }

        public WorkflowExecuterService(
            ILogger<WorkflowExecuterService> logger,
            IOptions<WorkflowManagerOptions> configuration,
            IOptions<StorageServiceConfiguration> storageConfiguration,
            IWorkflowRepository workflowRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IMessageBrokerPublisherService messageBrokerPublisherService,
            IWorkflowInstanceService workflowInstanceService,
            IConditionalParameterParser conditionalParser,
            ITaskExecutionStatsRepository taskExecutionStatsRepository,
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
            _defaultPerTaskTypeTimeoutMinutes = configuration.Value.PerTaskTypeTimeoutMinutes;
            TaskDispatchRoutingKey = configuration.Value.Messaging.Topics.TaskDispatchRequest;
            ClinicalReviewTimeoutRoutingKey = configuration.Value.Messaging.Topics.AideClinicalReviewCancelation;
            _migExternalAppPlugins = configuration.Value.MigExternalAppPlugins.ToList();
            ExportRequestRoutingKey = $"{configuration.Value.Messaging.Topics.ExportRequestPrefix}.{configuration.Value.Messaging.DicomAgents.ScuAgentName}";

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowRepository = workflowRepository ?? throw new ArgumentNullException(nameof(workflowRepository));
            _workflowInstanceRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
            _workflowInstanceService = workflowInstanceService ?? throw new ArgumentNullException(nameof(workflowInstanceService));
            _messageBrokerPublisherService = messageBrokerPublisherService ?? throw new ArgumentNullException(nameof(messageBrokerPublisherService));
            _conditionalParameterParser = conditionalParser ?? throw new ArgumentNullException(nameof(artifactMapper));
            _taskExecutionStatsRepository = taskExecutionStatsRepository ?? throw new ArgumentNullException(nameof(taskExecutionStatsRepository));
            _artifactMapper = artifactMapper ?? throw new ArgumentNullException(nameof(artifactMapper));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _payloadService = payloadService ?? throw new ArgumentNullException(nameof(payloadService));
        }

        public async Task<bool> ProcessPayload(WorkflowRequestEvent message, Payload payload)
        {
            Guard.Against.Null(message, nameof(message));

            using var loggerScope = _logger.BeginScope($"correlationId={message.CorrelationId}, payloadId={payload.PayloadId}");

            // for external App executions then workflowInstanceId will be supplied and we can continue the workflow from that task.
            if (string.IsNullOrWhiteSpace(message.WorkflowInstanceId) is false)
            {
                var instance = await _workflowInstanceRepository.GetByWorkflowInstanceIdAsync(message.WorkflowInstanceId);
                if (instance is not null)
                {
                    var task = instance.Tasks.First(t => t.TaskId == message.TaskId);
                    if (task is not null)
                    {
                        var workflow = await _workflowRepository.GetByWorkflowIdAsync(instance.WorkflowId);
                        await HandleTaskDestinations(instance, workflow, task, message.CorrelationId);
                        return true;
                    }
                }
            }

            var processed = true;
            List<WorkflowRevision>? workflows;

            if (message.Workflows?.Any() == true)
            {
                workflows = await _workflowRepository.GetByWorkflowsIdsAsync(message.Workflows) as List<WorkflowRevision>;
            }
            else
            {
                var result = await _workflowRepository.GetWorkflowsForWorkflowRequestAsync(message.DataTrigger.Destination, message.DataTrigger.Source);
                workflows = new List<WorkflowRevision>(result);
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
                _logger.FailedToCreateWorkflowInstances();
                return false;
            }

            foreach (var workflowInstance in workflowInstances)
            {
                await ProcessFirstWorkflowTask(workflowInstance, message.CorrelationId, payload);
            }

            return true;
        }

<<<<<<< Updated upstream
=======
        public async Task<bool> ProcessArtifactReceivedAsync(ArtifactsReceivedEvent message)
        {
            Guard.Against.Null(message, nameof(message));

            var workflowInstanceId = message.WorkflowInstanceId;
            var taskId = message.TaskId;

            if (string.IsNullOrWhiteSpace(workflowInstanceId) || string.IsNullOrWhiteSpace(taskId))
            {
                return false;
            }

            var workflowInstance = await _workflowInstanceRepository.GetByWorkflowInstanceIdAsync(workflowInstanceId).ConfigureAwait(false);
            if (workflowInstance is null)
            {
                _logger.WorkflowInstanceNotFound(workflowInstanceId);
                return false;
            }

            var workflowTemplate = await _workflowRepository.GetByWorkflowIdAsync(workflowInstance.WorkflowId)
                .ConfigureAwait(false);

            if (workflowTemplate is null)
            {
                _logger.WorkflowNotFound(workflowInstanceId);
                return false;
            }

            var taskTemplate = workflowTemplate.Workflow?.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (taskTemplate is null)
            {
                _logger.TaskNotFoundInWorkflow(message.PayloadId.ToString(), taskId, workflowTemplate.Id);
                return false;
            }

            var previouslyReceivedArtifactsFromRepo = await _artifactsRepository.GetAllAsync(workflowInstanceId, taskId).ConfigureAwait(false);
            if (previouslyReceivedArtifactsFromRepo is null || previouslyReceivedArtifactsFromRepo.Count == 0)
            {
                previouslyReceivedArtifactsFromRepo = new List<ArtifactReceivedItems>() { new ArtifactReceivedItems()
                {
                    TaskId = taskId,
                    WorkflowInstanceId = workflowInstanceId,
                    Artifacts = message.Artifacts.Select(ArtifactReceivedDetails.FromArtifact).ToList()
                } };
            }
            await _artifactsRepository
                .AddOrUpdateItemAsync(workflowInstanceId, taskId, message.Artifacts).ConfigureAwait(false);

            var previouslyReceivedArtifacts = previouslyReceivedArtifactsFromRepo.SelectMany(a => a.Artifacts).Select(a => a.Type).ToList();

            var requiredArtifacts = taskTemplate.Artifacts.Output.Where(a => a.Mandatory).Select(a => a.Type);
            var receivedArtifacts = message.Artifacts.Select(a => a.Type).Concat(previouslyReceivedArtifacts).ToList();
            var missingArtifacts = requiredArtifacts.Except(receivedArtifacts).ToList();
            var allArtifacts = taskTemplate.Artifacts.Output.Select(a => a.Type);
            var unexpectedArtifacts = receivedArtifacts.Except(allArtifacts).ToList();

            if (unexpectedArtifacts.Any())
            {
                _logger.UnexpectedArtifactsReceived(taskId, workflowInstanceId, string.Join(',', unexpectedArtifacts));
            }

            if (!missingArtifacts.Any())
            {
                return await AllRequiredArtifactsReceivedAsync(message, workflowInstance, taskId, workflowInstanceId, workflowTemplate).ConfigureAwait(false);
            }

            _logger.MandatoryOutputArtifactsMissingForTask(taskId, string.Join(',', missingArtifacts));
            return true;
        }

        private async Task<bool> AllRequiredArtifactsReceivedAsync(ArtifactsReceivedEvent message, WorkflowInstance workflowInstance,
            string taskId, string workflowInstanceId, WorkflowRevision workflowTemplate)
        {
            var taskExecution = workflowInstance.Tasks.FirstOrDefault(t => t.TaskId == taskId);

            if (taskExecution is null)
            {
                _logger.TaskNotFoundInWorkflowInstance(taskId, workflowInstanceId);
                return false;
            }

            await _workflowInstanceRepository.UpdateTaskStatusAsync(workflowInstanceId, taskId,
                TaskExecutionStatus.Succeeded).ConfigureAwait(false);

            // Dispatch Task
            var taskDispatchedResult =
                await HandleTaskDestinations(workflowInstance, workflowTemplate, taskExecution, message.CorrelationId).ConfigureAwait(false);

            if (taskDispatchedResult is false)
            {
                _logger.LogTaskDispatchFailure(message.PayloadId.ToString(), taskId, workflowInstanceId, workflowTemplate.WorkflowId, JsonConvert.SerializeObject(message.Artifacts));
                return false;
            }

            return true;
        }

>>>>>>> Stashed changes
        public async Task ProcessFirstWorkflowTask(WorkflowInstance workflowInstance, string correlationId, Payload payload)
        {
            if (workflowInstance.Status == Status.Failed)
            {
                _logger.WorkflowBadStatus(workflowInstance.WorkflowId, workflowInstance.Status);
                return;
            }

            var task = workflowInstance.Tasks.FirstOrDefault();

            var workflow = await _workflowRepository.GetByWorkflowIdAsync(workflowInstance.WorkflowId);

            if (task is null || workflow is null)
            {
                return;
            }

            using var loggingScope = _logger.BeginScope(new LoggingDataDictionary<string, object>
            {
                ["workflowInstanceId"] = workflowInstance.Id,
                ["durationSoFar"] = (DateTime.UtcNow - workflowInstance.StartTime).TotalMilliseconds,
                ["executionId"] = task.ExecutionId
            });

            await SwitchTasksAsync(task,
                    routerFunc: () => HandleTaskDestinations(workflowInstance, workflow, task, correlationId),
                    exportFunc: () => HandleDicomExportAsync(workflow, workflowInstance, task, correlationId),
                    externalFunc: () => HandleExternalAppAsync(workflow, workflowInstance, task, correlationId),
                    notCreatedStatusFunc: () =>
                    {
                        _logger.TaskPreviouslyDispatched(workflowInstance.PayloadId, task.TaskId);
                        return Task.CompletedTask;
                    },
                    defaultFunc: () => DispatchTask(workflowInstance, workflow, task, correlationId, payload)
            ).ConfigureAwait(false);
        }

        private static Task SwitchTasksAsync(TaskExecution task,
            Func<Task> routerFunc,
            Func<Task> exportFunc,
            Func<Task> externalFunc,
            Func<Task> notCreatedStatusFunc,
            Func<Task> defaultFunc) =>
            task switch
            {
                {  TaskType: TaskTypeConstants.RouterTask } => routerFunc(),
                {  TaskType: TaskTypeConstants.ExportTask } => exportFunc(),
                {  TaskType: TaskTypeConstants.ExternalAppTask } => externalFunc(),
                { Status: var s } when s != TaskExecutionStatus.Created  => notCreatedStatusFunc(),
                _ => defaultFunc()
            };

        public async Task<bool> ProcessTaskUpdate(TaskUpdateEvent message)
        {
            Guard.Against.Null(message, nameof(message));
            Guard.Against.Null(message.WorkflowInstanceId, nameof(message.WorkflowInstanceId));

            var workflowInstance = await _workflowInstanceRepository.GetByWorkflowInstanceIdAsync(message.WorkflowInstanceId);

            if (workflowInstance is null)
            {
                _logger.WorkflowInstanceNotFound(message.WorkflowInstanceId);

                return false;
            }

            var currentTask = workflowInstance.Tasks.FirstOrDefault(t => t.TaskId == message.TaskId);

            using var loggingScope = _logger.BeginScope(new LoggingDataDictionary<string, object>
            {
                ["workflowInstanceId"] = workflowInstance.Id,
                ["taskStatus"] = message.Status,
                ["durationSoFar"] = (DateTime.UtcNow - workflowInstance.StartTime).TotalMilliseconds,
                ["executionId"] = currentTask?.ExecutionId ?? ""
            });

            if (currentTask is null)
            {
                _logger.TaskNotFoundInWorkflowInstance(message.TaskId, message.WorkflowInstanceId);

                return false;
            }

            currentTask.WorkflowInstanceId = message.WorkflowInstanceId;

            if (message.Reason == FailureReason.TimedOut && currentTask.Status == TaskExecutionStatus.Failed)
            {
                _logger.TaskTimedOut(message.TaskId, message.WorkflowInstanceId, currentTask.Timeout);
                await ClinicalReviewTimeOutEvent(workflowInstance, currentTask, message.CorrelationId);
            }

            if (!message.Status.IsTaskExecutionStatusUpdateValid(currentTask.Status))
            {
                _logger.TaskStatusUpdateNotValid(workflowInstance.PayloadId, message.TaskId, currentTask.Status.ToString(), message.Status.ToString());

                return false;
            }

            await UpdateTaskDetails(currentTask, workflowInstance.Id, workflowInstance.WorkflowId, message.ExecutionStats, message.Metadata, message.Reason);

            var previouslyFailed = workflowInstance.Tasks.Any(t => t.Status == TaskExecutionStatus.Failed) || workflowInstance.Status == Status.Failed;

            if (message.Status == TaskExecutionStatus.Failed || message.Status == TaskExecutionStatus.Canceled || previouslyFailed)
            {
                await UpdateWorkflowInstanceStatus(workflowInstance, message.TaskId, message.Status);

                return await CompleteTask(currentTask, workflowInstance, message.CorrelationId, message.Status);
            }

            if (message.Status != TaskExecutionStatus.Succeeded)
            {
                await UpdateWorkflowInstanceStatus(workflowInstance, message.TaskId, message.Status);

                return await HandleUpdatingTaskStatus(currentTask, workflowInstance.WorkflowId, message.Status);
            }

            var workflow = await _workflowRepository.GetByWorkflowIdAsync(workflowInstance.WorkflowId);

            if (workflow is null)
            {
                _logger.WorkflowNotFound(workflowInstance.WorkflowId);

                return false;
            }

            var isValid = await HandleOutputArtifacts(workflowInstance, message.Outputs, currentTask, workflow);

            if (isValid is false)
            {
                return await CompleteTask(currentTask, workflowInstance, message.CorrelationId, TaskExecutionStatus.Failed);
            }

            return await HandleTaskDestinations(workflowInstance, workflow, currentTask, message.CorrelationId);
        }

        private async Task<bool> HandleUpdatingTaskStatus(TaskExecution taskExecution, string workflowId, TaskExecutionStatus status)
        {
            await _taskExecutionStatsRepository.UpdateExecutionStatsAsync(taskExecution, workflowId, status);
            return await _workflowInstanceRepository.UpdateTaskStatusAsync(taskExecution.WorkflowInstanceId, taskExecution.TaskId, status);
        }

        public async Task UpdateTaskDetails(TaskExecution currentTask, string workflowInstanceId, string workflowId, Dictionary<string, string> executionStats, Dictionary<string, object> metadata, FailureReason failureReason)
        {
            if (executionStats?.IsNullOrEmpty() is false)
            {
                currentTask.ExecutionStats.AppendSafe(executionStats);
            }

            if (metadata.Any())
            {
                currentTask.ResultMetadata.AppendSafe(metadata);
            }

            currentTask.Reason = failureReason;

            if (currentTask.Reason == FailureReason.TimedOut)
            {
                _logger.TaskTimedOut(currentTask.TaskId, workflowInstanceId, currentTask.Timeout);
            }

            await _workflowInstanceRepository.UpdateTaskAsync(workflowInstanceId, currentTask.TaskId, currentTask);
            await _taskExecutionStatsRepository.UpdateExecutionStatsAsync(currentTask, workflowId);
        }

        public async Task<bool> ProcessExportComplete(ExportCompleteEvent message, string correlationId)
        {
            var workflowInstance = await _workflowInstanceRepository.GetByWorkflowInstanceIdAsync(message.WorkflowInstanceId);
            var task = workflowInstance.Tasks.FirstOrDefault(t => t.TaskId == message.ExportTaskId);

            if (task is null)
            {
                return false;
            }

            using var loggingScope = _logger.BeginScope(new LoggingDataDictionary<string, object>
            {
                ["workflowInstanceId"] = workflowInstance.Id,
                ["durationSoFar"] = (DateTime.UtcNow - workflowInstance.StartTime).TotalMilliseconds,
                ["executionId"] = task.ExecutionId
            });

            await _workflowInstanceService.UpdateExportCompleteMetadataAsync(workflowInstance.Id, task.ExecutionId, message.FileStatuses);

            var succeededFileCount = message.FileStatuses.Count(f => f.Value == FileExportStatus.Success);
            var totalFileCount = message.FileStatuses.Count();

            if (message.Status.Equals(ExportStatus.Success)
                && TaskExecutionStatus.Succeeded.IsTaskExecutionStatusUpdateValid(task.Status))
            {
                _logger.DicomExportSucceeded($"{succeededFileCount}/{totalFileCount}");

                var workflow = await _workflowRepository.GetByWorkflowIdAsync(workflowInstance.WorkflowId);

                if (workflow is null)
                {
                    _logger.WorkflowNotFound(workflowInstance.WorkflowId);

                    return false;
                }

                if (string.Compare(task.TaskType, ValidationConstants.ExportTaskType, true) == 0)
                {
                    return await HandleTaskDestinations(workflowInstance, workflow, task, correlationId);
                }
            }

            if ((message.Status.Equals(ExportStatus.Failure) || message.Status.Equals(ExportStatus.PartialFailure)) &&
                TaskExecutionStatus.Failed.IsTaskExecutionStatusUpdateValid(task.Status))
            {
                _logger.DicomExportFailed($"{succeededFileCount}/{totalFileCount}");

                return await CompleteTask(task, workflowInstance, correlationId, TaskExecutionStatus.Failed);
            }

            return true;
        }

        private async Task<bool> CompleteTask(TaskExecution task, WorkflowInstance workflowInstance, string correlationId, TaskExecutionStatus status)
        {
            var payload = await _payloadService.GetByIdAsync(workflowInstance.PayloadId);

            if (status is TaskExecutionStatus.Failed)
            {
                _logger.TaskFailed(task, workflowInstance, payload?.PatientDetails, correlationId, status.ToString());

                await UpdateWorkflowInstanceStatus(workflowInstance, task.TaskId, TaskExecutionStatus.Failed);
            }
            else
            {
                _logger.TaskComplete(task, workflowInstance, payload?.PatientDetails, correlationId, status.ToString());
            }

            task.WorkflowInstanceId = workflowInstance.Id;
            return await HandleUpdatingTaskStatus(task, workflowInstance.WorkflowId, status);
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

            if (!previousTasks.Any(t => t.Status != TaskExecutionStatus.Succeeded && t.Status != TaskExecutionStatus.Canceled && t.Status != TaskExecutionStatus.PartialFail)
                && (currentTaskStatus == TaskExecutionStatus.Succeeded || currentTaskStatus == TaskExecutionStatus.Canceled || currentTaskStatus == TaskExecutionStatus.PartialFail))
            {
                return await _workflowInstanceRepository.UpdateWorkflowInstanceStatusAsync(workflowInstance.Id, Status.Succeeded);
            }

            return true;
        }

        private async Task HandleExternalAppAsync(WorkflowRevision workflow, WorkflowInstance workflowInstance, TaskExecution task, string correlationId)
        {
            var plugins = _migExternalAppPlugins;
            await HandleDicomExportAsync(workflow, workflowInstance, task, correlationId, plugins).ConfigureAwait(false);
        }

        private async Task HandleDicomExportAsync(WorkflowRevision workflow, WorkflowInstance workflowInstance, TaskExecution task, string correlationId, List<string>? plugins = null)
        {
            plugins ??= new List<string>();
            var exportList = workflow.Workflow?.Tasks?.FirstOrDefault(t => t.Id == task.TaskId)?.ExportDestinations.Select(e => e.Name).ToArray();

            var artifactValues = GetDicomExports(workflow, task, exportList);

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

                await CompleteTask(task, workflowInstance, correlationId, TaskExecutionStatus.Failed);

                return;
            }
            await DispatchDicomExport(workflowInstance, task, exportList, artifactValues, correlationId, plugins);
        }

        private string[] GetDicomExports(WorkflowRevision workflow, TaskExecution task, string[]? exportDestinations)
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

        private async Task<bool> DispatchDicomExport(WorkflowInstance workflowInstance, TaskExecution task, string[]? exportDestinations, string[] artifactValues, string correlationId, List<string> plugins)
        {
            if (exportDestinations is null || !exportDestinations.Any())
            {
                return false;
            }

            _logger.LogMigExport(task.TaskId, string.Join(",", exportDestinations), artifactValues.Length, string.Join(",", plugins));
            await ExportRequest(workflowInstance, task, exportDestinations, artifactValues, correlationId, plugins);
            return await _workflowInstanceRepository.UpdateTaskStatusAsync(workflowInstance.Id, task.TaskId, TaskExecutionStatus.Dispatched);
        }

        private async Task<bool> HandleOutputArtifacts(WorkflowInstance workflowInstance, List<Messaging.Common.Storage> outputs, TaskExecution task, WorkflowRevision workflowRevision)
        {
            var artifactDict = outputs.ToArtifactDictionary();
            var list = artifactDict.Select(a => $"{a.Value}").ToList();
            var validOutputArtifacts = (await _storageService.VerifyObjectsExistAsync(workflowInstance.BucketId, artifactDict.Select(a => a.Value).ToList(), default)) ?? new Dictionary<string, bool>();

            var validArtifacts = artifactDict.Where(a => validOutputArtifacts.Any(v => v.Value && v.Key == a.Value)).ToDictionary(d => d.Key, d => d.Value);

            var revisionTask = workflowRevision.Workflow?.Tasks.FirstOrDefault(t => t.Id == task.TaskId);

            if (revisionTask is null)
            {
                _logger.TaskNotFoundInWorkflow(workflowInstance.PayloadId, task.TaskId, workflowRevision.WorkflowId);

                return false;
            }

            foreach (var artifact in artifactDict)
            {
                var outputArtifact = revisionTask.Artifacts.Output.FirstOrDefault(o => o.Name == artifact.Key);
                _logger.LogArtifactPassing(new Artifact { Name = artifact.Key, Value = artifact.Value, Mandatory = outputArtifact?.Mandatory ?? true }, artifact.Value, "Post-Task Output Artifact", validOutputArtifacts?.ContainsKey(artifact.Key) ?? false);
            }

            var missingOutputs = revisionTask.Artifacts.Output.Where(o => validArtifacts.Any(m => m.Key == o.Name) is false);

            if (missingOutputs.Any(o => o.Mandatory))
            {
                _logger.MandatoryOutputArtefactsMissingForTask(task.TaskId);

                return false;
            }

            var currentTask = workflowInstance.Tasks?.FirstOrDefault(t => t.TaskId == task.TaskId);

            if (currentTask is not null)
            {
                currentTask.OutputArtifacts = validArtifacts;
            }

            if (validOutputArtifacts is not null && validOutputArtifacts.Any())
            {
                return await _workflowInstanceRepository.UpdateTaskOutputArtifactsAsync(workflowInstance.Id, task.TaskId, validArtifacts);
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
                using var loggingScope = _logger.BeginScope(new LoggingDataDictionary<string, object> { ["executionId"] = taskExec?.ExecutionId ?? "" });

                if (taskExec is null)
                {
                    continue;
                }

                if (string.Equals(taskExec!.TaskType, TaskTypeConstants.RouterTask, StringComparison.InvariantCultureIgnoreCase))
                {
                    await HandleTaskDestinations(workflowInstance, workflow, taskExec!, correlationId);

                    continue;
                }

                if (string.Equals(taskExec!.TaskType, TaskTypeConstants.ExportTask, StringComparison.InvariantCultureIgnoreCase))
                {
                    await HandleDicomExportAsync(workflow, workflowInstance, taskExec!, correlationId);

                    continue;
                }

                if (string.Equals(taskExec!.TaskType, TaskTypeConstants.ExternalAppTask, StringComparison.InvariantCultureIgnoreCase))
                {
                    await HandleExternalAppAsync(workflow, workflowInstance, taskExec!, correlationId);

                    continue;
                }

                processed &= await DispatchTask(workflowInstance, workflow, taskExec!, correlationId);

                if (processed is false)
                {
                    continue;
                }
                processed &= await HandleUpdatingTaskStatus(taskExec, workflowInstance.WorkflowId, TaskExecutionStatus.Dispatched);
            }

            return processed;
        }

        private async Task<bool> HandleTaskDestinations(WorkflowInstance workflowInstance, WorkflowRevision workflow, TaskExecution task, string correlationId)
        {
            var newTaskExecutions = await CreateTaskDestinations(workflowInstance, workflow, task.TaskId);

            if (newTaskExecutions.Any(task => task.Status == TaskExecutionStatus.Failed))
            {
                return await CompleteTask(task, workflowInstance, correlationId, TaskExecutionStatus.Failed);
            }

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
                    && _conditionalParameterParser.TryParse(taskDest.Conditions, workflowInstance, out var resolvedConditional) is false)
                {
                    _logger.TaskDestinationConditionFalse(resolvedConditional, taskDest.Conditions.CombineConditionString(), taskDest.Name);

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
                    _logger.TaskNotFoundInWorkflow(workflowInstance.PayloadId, taskDest.Name, workflow?.WorkflowId);

                    continue;
                }

                newTaskExecutions.Add(await CreateTaskExecutionAsync(newTask, workflowInstance, null, null, taskId));
            }

            return newTaskExecutions;
        }

        private async Task<bool> DispatchTask(WorkflowInstance workflowInstance, WorkflowRevision workflow, TaskExecution taskExec, string correlationId, Payload? payload = null)
        {
            var task = workflow?.Workflow?.Tasks?.FirstOrDefault(t => t.Id == taskExec.TaskId);

            if (task is null)
            {
                return false;
            }

            using (_logger.BeginScope(new LoggingDataDictionary<string, object> { ["correlationId"] = correlationId, ["taskId"] = task.Id, ["executionId"] = taskExec.ExecutionId }))
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

                var pathOutputArtifacts = new Dictionary<string, string>();
                try
                {
                    pathOutputArtifacts = await _artifactMapper.ConvertArtifactVariablesToPath(outputArtifacts ?? Array.Empty<Artifact>(), workflowInstance.PayloadId, workflowInstance.Id, workflowInstance.BucketId, false);
                }
                catch (FileNotFoundException)
                {
                    _logger.LogTaskDispatchFailure(workflowInstance.PayloadId, taskExec.TaskId, workflowInstance.Id, workflow?.Id, JsonConvert.SerializeObject(pathOutputArtifacts));
                    workflowInstance.Tasks.Add(taskExec);
                    var updateResult = await HandleUpdatingTaskStatus(taskExec, workflowInstance.WorkflowId, TaskExecutionStatus.Failed);
                    if (updateResult is false)
                    {
                        _logger.LoadArtifactAndDBFailure(workflowInstance.PayloadId, taskExec.TaskId, workflowInstance.Id, workflow?.Id);
                    }
                    throw;
                }

                payload ??= await _payloadService.GetByIdAsync(workflowInstance.PayloadId);
                if (payload is not null)
                {
                    taskExec.AttachPatientMetaData(payload.PatientDetails, _logger.AttachedPatientMetadataToTaskExec);
                }

                taskExec.TaskPluginArguments["workflow_name"] = workflow!.Workflow!.Name;
                _logger.LogGeneralTaskDispatchInformation(workflowInstance.PayloadId, taskExec.TaskId, workflowInstance.Id, workflow?.Id, JsonConvert.SerializeObject(pathOutputArtifacts));
                var taskDispatchEvent = EventMapper.ToTaskDispatchEvent(taskExec, workflowInstance, pathOutputArtifacts, correlationId, _storageConfiguration);
                var jsonMesssage = new JsonMessage<TaskDispatchEvent>(taskDispatchEvent, MessageBrokerConfiguration.WorkflowManagerApplicationId, taskDispatchEvent.CorrelationId, Guid.NewGuid().ToString());

                await _messageBrokerPublisherService.Publish(TaskDispatchRoutingKey, jsonMesssage.ToMessage());

                await _taskExecutionStatsRepository.CreateAsync(taskExec, workflowInstance.WorkflowId, correlationId);

                return await HandleUpdatingTaskStatus(taskExec, workflowInstance.WorkflowId, TaskExecutionStatus.Dispatched);
            }
        }

        private async Task<bool> ExportRequest(WorkflowInstance workflowInstance, TaskExecution taskExec, string[] exportDestinations, IList<string> dicomImages, string correlationId, List<string> plugins)
        {
            var exportRequestEvent = EventMapper.ToExportRequestEvent(dicomImages, exportDestinations, taskExec.TaskId, workflowInstance.Id, correlationId, plugins);
            var jsonMesssage = new JsonMessage<ExportRequestEvent>(exportRequestEvent, MessageBrokerConfiguration.WorkflowManagerApplicationId, exportRequestEvent.CorrelationId, Guid.NewGuid().ToString());

            await _messageBrokerPublisherService.Publish(ExportRequestRoutingKey, jsonMesssage.ToMessage());
            return true;
        }

        private async Task<bool> ClinicalReviewTimeOutEvent(WorkflowInstance workflowInstance, TaskExecution taskExec, string correlationId)
        {
            var cancelationRequestEvent = EventMapper.GenerateTaskCancellationEvent("", taskExec.ExecutionId, workflowInstance.Id, taskExec.TaskId, FailureReason.TimedOut, "Timed out");
            var jsonMesssage = new JsonMessage<TaskCancellationEvent>(cancelationRequestEvent, MessageBrokerConfiguration.WorkflowManagerApplicationId, correlationId, Guid.NewGuid().ToString());

            _logger.TaskTimedOut(taskExec.TaskId, workflowInstance.Id, taskExec.Timeout);
            await _messageBrokerPublisherService.Publish(ClinicalReviewTimeoutRoutingKey, jsonMesssage.ToMessage());
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

                var task = await CreateTaskExecutionAsync(firstTask, workflowInstance, message.Bucket, message.PayloadId.ToString());
                task.TaskPluginArguments["workflow_name"] = workflow.Workflow.Name;

                tasks.Add(task);
                if (task.Status == TaskExecutionStatus.Failed)
                {
                    workflowInstance.Status = Status.Failed;
                }
            }

            workflowInstance.Tasks = tasks;

            return workflowInstance;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        public async Task<TaskExecution> CreateTaskExecutionAsync(TaskObject task,
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                                                  WorkflowInstance workflowInstance,
                                                  string? bucketName = null,
                                                  string? payloadId = null,
                                                  string? previousTaskId = null)
        {
            Guard.Against.Null(workflowInstance, nameof(workflowInstance));

            var workflowInstanceId = workflowInstance.Id;

            bucketName ??= workflowInstance.BucketId;

            payloadId ??= workflowInstance.PayloadId;

            Guard.Against.Null(task, nameof(task));
            Guard.Against.NullOrWhiteSpace(task.Type, nameof(task.Type));
            Guard.Against.NullOrWhiteSpace(task.Id, nameof(task.Id));
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrWhiteSpace(bucketName, nameof(bucketName));
            Guard.Against.NullOrWhiteSpace(payloadId, nameof(payloadId));

            var executionId = Guid.NewGuid().ToString();
            var newTaskArgs = GetTaskArgs(task, workflowInstance);

            if (task.TimeoutMinutes == -1)
            {
                task.TimeoutMinutes = _defaultTaskTimeoutMinutes;
            }
            if (_defaultPerTaskTypeTimeoutMinutes is not null && _defaultPerTaskTypeTimeoutMinutes.ContainsKey(task.Type))
            {
                task.TimeoutMinutes = _defaultPerTaskTypeTimeoutMinutes[task.Type];
            }

            var inputArtifacts = new Dictionary<string, string>();
            var artifactFound = true;
            if (task?.Artifacts?.Input.IsNullOrEmpty() is false)
            {
                artifactFound = _artifactMapper.TryConvertArtifactVariablesToPath(task?.Artifacts?.Input
                    ?? Array.Empty<Artifact>(), payloadId, workflowInstanceId, bucketName, true, out inputArtifacts);
            }

            return new TaskExecution()
            {
                ExecutionId = executionId,
                TaskType = task?.Type ?? "UnknownTaskType",
                TaskPluginArguments = newTaskArgs,
                TaskStartTime = DateTime.UtcNow,
                TaskId = task?.Id ?? "0",
                WorkflowInstanceId = workflowInstanceId,
                Status = artifactFound ? TaskExecutionStatus.Created : TaskExecutionStatus.Failed,
                Reason = artifactFound ? FailureReason.None : FailureReason.ExternalServiceError,
                InputArtifacts = inputArtifacts,
                OutputDirectory = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}",
                ResultMetadata = { },
                PreviousTaskId = previousTaskId ?? string.Empty,
                TimeoutInterval = task?.TimeoutMinutes ?? _defaultTaskTimeoutMinutes,
            };
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
