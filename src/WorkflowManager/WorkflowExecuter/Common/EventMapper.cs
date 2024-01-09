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

using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Common
{
    public class GenerateTaskUpdateEventParams
    {
        public string CorrelationId { get; set; } = "";

        public string ExecutionId { get; set; } = "";

        public string WorkflowInstanceId { get; set; } = "";

        public string TaskId { get; set; } = "";

        public FailureReason FailureReason { get; set; }

        public TaskExecutionStatus TaskExecutionStatus { get; set; }

        public Dictionary<string, string> Stats { get; set; } = new();

        public string Errors { get; set; } = "";
    }

    public static class EventMapper
    {
        public static JsonMessage<T> ToJsonMessage<T>(T message, string applicationId, string correlationId) where T : EventBase
        {
            return new JsonMessage<T>(message, applicationId, correlationId);
        }

        public static TaskUpdateEvent GenerateTaskUpdateEvent(GenerateTaskUpdateEventParams eventParams)
        {
            ArgumentNullException.ThrowIfNull(eventParams, nameof(eventParams));

            return new TaskUpdateEvent
            {
                CorrelationId = eventParams.CorrelationId,
                ExecutionId = eventParams.ExecutionId,
                Reason = eventParams.FailureReason,
                Status = eventParams.TaskExecutionStatus,
                ExecutionStats = eventParams.Stats,
                WorkflowInstanceId = eventParams.WorkflowInstanceId,
                TaskId = eventParams.TaskId,
                Message = eventParams.Errors,
                Outputs = new List<Messaging.Common.Storage>(),
            };
        }

        public static TaskCancellationEvent GenerateTaskCancellationEvent(
            string identity,
            string executionId,
            string workflowInstanceId,
            string taskId,
            FailureReason failureReason,
            string message)
        {
            //ArgumentNullException.ThrowIfNull(identity, nameof(identity));
            ArgumentNullException.ThrowIfNull(workflowInstanceId, nameof(workflowInstanceId));

            return new TaskCancellationEvent
            {
                ExecutionId = executionId,
                Reason = failureReason,
                WorkflowInstanceId = workflowInstanceId,
                TaskId = taskId,
                Identity = identity,
                Message = message
            };
        }

        public static TaskDispatchEvent ToTaskDispatchEvent(TaskExecution task,
            WorkflowInstance workflowInstance,
            Dictionary<string, string> outputArtifacts,
            string correlationId,
            StorageServiceConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(task, nameof(task));
            ArgumentNullException.ThrowIfNull(workflowInstance, nameof(workflowInstance));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstance.BucketId, nameof(workflowInstance.BucketId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstance.Id, nameof(workflowInstance.Id));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstance.PayloadId, nameof(workflowInstance.PayloadId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(correlationId, nameof(correlationId));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            var inputs = new List<Messaging.Common.Storage>();
            var outputs = new List<Messaging.Common.Storage>();

            if (task.InputArtifacts is not null)
            {
                foreach (var inArtifact in task.InputArtifacts)
                {
                    inputs.Add(new Messaging.Common.Storage
                    {
                        SecuredConnection = bool.Parse(configuration.Settings["securedConnection"]),
                        Endpoint = configuration.Settings["endpoint"],
                        Bucket = workflowInstance.BucketId,
                        RelativeRootPath = inArtifact.Value,
                        Name = inArtifact.Key
                    });
                }
            }

            if (outputArtifacts is not null)
            {
                foreach (var outArtifact in outputArtifacts)
                {
                    outputs.Add(new Messaging.Common.Storage
                    {
                        SecuredConnection = bool.Parse(configuration.Settings["securedConnection"]),
                        Endpoint = configuration.Settings["endpoint"],
                        Bucket = workflowInstance.BucketId,
                        RelativeRootPath = outArtifact.Value,
                        Name = outArtifact.Key
                    });
                }
            }

            var pluginArgs = task.TaskPluginArguments;
            if (pluginArgs.ContainsKey("reviewed_task_id"))
            {
                var reviewedTask = workflowInstance.Tasks.FirstOrDefault(t => t.TaskId.ToLower() == pluginArgs["reviewed_task_id"]);

                if (reviewedTask is not null)
                {
                    pluginArgs.Add("reviewed_execution_id", reviewedTask.ExecutionId);
                }
            }

            return new TaskDispatchEvent
            {
                WorkflowInstanceId = workflowInstance.Id,
                TaskId = task.TaskId,
                ExecutionId = task.ExecutionId.ToString(),
                CorrelationId = correlationId,
                Status = TaskExecutionStatus.Created,
                TaskPluginArguments = pluginArgs,
                Inputs = inputs,
                Outputs = outputs,
                TaskPluginType = task.TaskType,
                Metadata = { },
                PayloadId = workflowInstance.PayloadId,
                IntermediateStorage = new Messaging.Common.Storage
                {
                    Bucket = workflowInstance.BucketId,
                    RelativeRootPath = task.OutputDirectory,
                    Endpoint = configuration.Settings["endpoint"],
                    Name = task.TaskId,
                    SecuredConnection = bool.Parse(configuration.Settings["securedConnection"])
                }
            };
        }

        public static ExportRequestEvent ToExportRequestEvent(
            IList<string> dicomImages,
            string[] exportDestinations,
            string taskId,
            string workflowInstanceId,
            string correlationId,
            List<string>? plugins = null)
        {
            plugins ??= new List<string>();

            ArgumentNullException.ThrowIfNullOrWhiteSpace(taskId, nameof(taskId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(correlationId, nameof(correlationId));
            Guard.Against.NullOrEmpty(dicomImages, nameof(dicomImages));
            Guard.Against.NullOrEmpty(exportDestinations, nameof(exportDestinations));

            var request = new ExportRequestEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                ExportTaskId = taskId,
                CorrelationId = correlationId,
                Files = dicomImages,
                Destinations = exportDestinations,
                Target = new DataOrigin { DataService = DataService.DIMSE, Destination = exportDestinations[0] }
            };
            request.PluginAssemblies.AddRange(plugins);
            return request;
        }

        public static ExternalAppRequestEvent ToExternalAppRequestEvent(
            IList<string> dicomImages,
            List<DataOrigin> exportDestinations,
            string taskId,
            string workflowInstanceId,
            string correlationId,
            string destinationFolder,
            List<string>? plugins = null)
        {
            plugins ??= new List<string>();

            ArgumentNullException.ThrowIfNullOrWhiteSpace(taskId, nameof(taskId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(correlationId, nameof(correlationId));
            Guard.Against.NullOrEmpty(dicomImages, nameof(dicomImages));
            Guard.Against.NullOrEmpty(exportDestinations, nameof(exportDestinations));

            var request = new ExternalAppRequestEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                ExportTaskId = taskId,
                CorrelationId = correlationId,
                Files = dicomImages,
                Targets = exportDestinations,
                DestinationFolder = destinationFolder
            };
            request.PluginAssemblies.AddRange(plugins);
            return request;
        }
    }
}
