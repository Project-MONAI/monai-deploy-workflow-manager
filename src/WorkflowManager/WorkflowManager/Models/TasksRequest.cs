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

using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Common.Models
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
