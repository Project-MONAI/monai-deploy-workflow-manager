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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class ExecutionContext
    {
        [JsonProperty(PropertyName = "execution_id")]
        public string ExecutionId { get; set; }

        [JsonProperty(PropertyName = "task_id")]
        public string TaskId { get; set; }

        [JsonProperty(PropertyName = "input_dir")]
        public string InputDir { get; set; }

        [JsonProperty(PropertyName = "output_dir")]
        public string OutputDir { get; set; }

        [JsonProperty(PropertyName = "task")]
        public Dictionary<string, string> Task { get; set; }

        [JsonProperty(PropertyName = "start_time")]
        public decimal StartTime { get; set; }

        [JsonProperty(PropertyName = "end_time")]
        public decimal EndTime { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "error_msg")]
        public string ErrorMsg { get; set; }

        [JsonProperty(PropertyName = "result")]
        public Dictionary<string, string> Result { get; set; }
    }
}
