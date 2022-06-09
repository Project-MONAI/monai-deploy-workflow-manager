// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Common
{
    public static class EventMapper
    {
        public static TaskDispatchEvent ToTaskDispatchEvent(TaskExecution task, string workflowId, string correlationId, StorageServiceConfiguration configuration)
        {
            Guard.Against.Null(task, nameof(task));
            Guard.Against.Null(workflowId, nameof(workflowId));
            Guard.Against.Null(correlationId, nameof(correlationId));
            Guard.Against.Null(configuration, nameof(configuration));

            var inputs = new List<Messaging.Common.Storage>();

            if (task?.InputArtifacts != null)
            {
                foreach (var inArtifact in task.InputArtifacts)
                {
                    inputs.Add(new Messaging.Common.Storage
                    {
                        SecuredConnection = bool.Parse(configuration.Settings["securedConnection"]),
                        Endpoint = configuration.Settings["endpoint"],
                        Bucket = configuration.Settings["bucket"],
                        RelativeRootPath = inArtifact.Value,
                        Name = inArtifact.Key
                    });
                }
            }

            return new TaskDispatchEvent
            {
                WorkflowInstanceId = workflowId,
                TaskId = task.TaskId,
                ExecutionId = task.ExecutionId.ToString(),
                CorrelationId = correlationId,
                Status = TaskExecutionStatus.Created,
                TaskPluginArguments = task.TaskPluginArguments,
                Inputs = inputs,
                TaskPluginType = task.TaskType,
                Metadata = task.Metadata,
                IntermediateStorage = new Messaging.Common.Storage
                {
                    Bucket = configuration.Settings["bucket"],
                    RelativeRootPath = $"{task.OutputDirectory}/tmp",
                    Endpoint = configuration.Settings["endpoint"],
                    Name = task.TaskId,
                    SecuredConnection = bool.Parse(configuration.Settings["securedConnection"])
                }
            };
        }
    }
}
