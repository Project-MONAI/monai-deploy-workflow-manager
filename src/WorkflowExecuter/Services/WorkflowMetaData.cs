// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Services
{
    public class WorkflowMetaData
    {
        [JsonProperty(PropertyName = "workflowInstanceId")]
        public string? WorkflowInstanceId { get; set; }


        [JsonProperty(PropertyName = "tasks")]
        public List<TaskMetadata>? Metadata { get; set; }
    }

    public class TaskMetadata
    {
        [JsonProperty(PropertyName = "taskId")]
        public string? TaskId { get; set; }

        [JsonProperty(PropertyName = "taskExecutionId")]
        public string? TaskExecutionId { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, object>? Metadata { get; set; }

        public static List<TaskMetadata> Create(Dictionary<string, object> metadata, string executionId, string taskId)
        {
            Guard.Against.NullOrEmpty(metadata, nameof(metadata));
            Guard.Against.NullOrEmpty(taskId, nameof(taskId));
            Guard.Against.NullOrEmpty(executionId, nameof(executionId));

            var taskMetadata = new TaskMetadata
            {
                Metadata = metadata,
                TaskExecutionId = executionId,
                TaskId = taskId
            };
            return new List<TaskMetadata> { taskMetadata };
        }
    }
}
