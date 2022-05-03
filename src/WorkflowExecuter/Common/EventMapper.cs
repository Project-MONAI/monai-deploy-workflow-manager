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
        public static TaskDispatchEvent ToTaskDispatchEvent(TaskExecution task, string workflowId, StorageServiceConfiguration configuration)
        {
            Guard.Against.Null(task, nameof(task));
            Guard.Against.Null(workflowId, nameof(workflowId));
            Guard.Against.Null(configuration, nameof(configuration));

            var inputs = new List<Messaging.Common.Storage>();

            foreach (var inArtifact in task.InputArtifacts)
            {
                inputs.Add(new Messaging.Common.Storage
                {
                    SecuredConnection = bool.Parse(configuration.Settings["securedConnection"]),
                    Endpoint = configuration.Settings["endpoint"],
                    Bucket = configuration.Settings["bucket"],
                    RelativeRootPath = inArtifact.Value,
                    Name = inArtifact.Value
                });
            }

            return new TaskDispatchEvent
            {
                WorkflowId = workflowId,
                TaskId = task.TaskId,
                ExecutionId = task.ExecutionId.ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Status = Messaging.Events.TaskStatus.Created,
                TaskPluginArguments = task.TaskPluginArguments,
                Inputs = inputs,
                Metadata = task.Metadata
            };
        }
    }
}
