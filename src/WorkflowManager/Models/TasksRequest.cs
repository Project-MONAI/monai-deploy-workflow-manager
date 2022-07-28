// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Models
{
    /// <summary>
    /// TasksRequest model.
    /// </summary>
    public class TasksRequest
    {
        /// <summary>
        /// Gets or sets WorkflowInstanceId.
        /// </summary>
        [JsonProperty(PropertyName = "workflowInstanceId")]
        public string WorkflowInstanceId { get; set; }

        /// <summary>
        /// Gets or sets TaskId.
        /// </summary>
        [JsonProperty(PropertyName = "taskId")]
        public string TaskId { get; set; }

        /// <summary>
        /// Gets or sets ExecutionId.
        /// </summary>
        [JsonProperty(PropertyName = "executionId")]
        public string ExecutionId { get; set; }
    }
}
