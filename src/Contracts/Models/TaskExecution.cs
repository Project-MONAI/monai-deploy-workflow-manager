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

using System;
using System.Collections.Generic;
using Monai.Deploy.Messaging.Events;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class TaskExecution
    {
        [JsonProperty(PropertyName = "execution_id")]
        public string ExecutionId { get; set; }

        [JsonProperty(PropertyName = "task_type")]
        public string TaskType { get; set; }

        [JsonProperty(PropertyName = "task_start_time")]
        public DateTime? TaskStartTime { get; set; }

        [JsonProperty(PropertyName = "execution_stats")]
        public Dictionary<string, string> ExecutionStats { get; set; }

        [JsonProperty(PropertyName = "task_plugin_arguments")]
        public Dictionary<string, string> TaskPluginArguments { get; set; }

        [JsonProperty(PropertyName = "task_id")]
        public string TaskId { get; set; }

        [JsonProperty(PropertyName = "previous_task_id")]
        public string PreviousTaskId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public TaskExecutionStatus Status { get; set; }

        [JsonProperty(PropertyName = "reason")]
        public FailureReason Reason { get; set; }

        [JsonProperty(PropertyName = "input_artifacts")]
        public Dictionary<string, string> InputArtifacts { get; set; }

        [JsonProperty(PropertyName = "output_artifacts")]
        public Dictionary<string, string> OutputArtifacts { get; set; }

        [JsonProperty(PropertyName = "output_directory")]
        public string OutputDirectory { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        [JsonProperty(PropertyName = "input_parameters")]
        public Dictionary<string, object> InputParameters { get; set; }
    }
}
