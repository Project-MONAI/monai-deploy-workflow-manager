// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.WorkfowExecuter.Common
{
    public static class EventMapper
    {
        public static TaskDispatchEvent ToTaskDispatchEvent(TaskExecution task, WorkflowInstance workflowInstance, Dictionary<string, string> outputArtifacts, string correlationId, StorageServiceConfiguration configuration)
        {

            Guard.Against.Null(task, nameof(task));
            Guard.Against.Null(workflowInstance, nameof(workflowInstance));
            Guard.Against.NullOrWhiteSpace(workflowInstance.BucketId, nameof(workflowInstance.BucketId));
            Guard.Against.NullOrWhiteSpace(workflowInstance.Id, nameof(workflowInstance.Id));
            Guard.Against.NullOrWhiteSpace(workflowInstance.PayloadId, nameof(workflowInstance.PayloadId));
            Guard.Against.NullOrWhiteSpace(correlationId, nameof(correlationId));
            Guard.Against.Null(configuration, nameof(configuration));

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

            return new TaskDispatchEvent
            {
                WorkflowInstanceId = workflowInstance.Id,
                TaskId = task.TaskId,
                ExecutionId = task.ExecutionId.ToString(),
                CorrelationId = correlationId,
                Status = TaskExecutionStatus.Created,
                TaskPluginArguments = task.TaskPluginArguments,
                Inputs = inputs,
                Outputs = outputs,
                TaskPluginType = task.TaskType,
                Metadata = task.Metadata,
                PayloadId = workflowInstance.PayloadId,
                IntermediateStorage = new Messaging.Common.Storage
                {
                    Bucket = workflowInstance.BucketId,
                    RelativeRootPath = $"{task.OutputDirectory}/tmp",
                    Endpoint = configuration.Settings["endpoint"],
                    Name = task.TaskId,
                    SecuredConnection = bool.Parse(configuration.Settings["securedConnection"])
                }
            };
        }

        public static ExportRequestEvent ToExportRequestEvent(IList<string> dicomImages, string[] exportDestinations, string taskId, string workflowInstanceId, string correlationId)
        {
            Guard.Against.NullOrWhiteSpace(taskId, nameof(taskId));
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrWhiteSpace(correlationId, nameof(correlationId));
            Guard.Against.NullOrEmpty(dicomImages, nameof(dicomImages));
            Guard.Against.NullOrEmpty(exportDestinations, nameof(exportDestinations));

            return new ExportRequestEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                ExportTaskId = taskId,
                CorrelationId = correlationId,
                Files = dicomImages,
                Destinations = exportDestinations
            };
        }
    }
}
