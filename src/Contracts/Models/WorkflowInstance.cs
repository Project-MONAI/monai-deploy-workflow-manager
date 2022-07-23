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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class WorkflowInstance
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "ae_title")]
        public string AeTitle { get; set; }

        [JsonProperty(PropertyName = "workflow_id")]
        public string WorkflowId { get; set; }

        [JsonProperty(PropertyName = "payload_id")]
        public string PayloadId { get; set; }

        [JsonProperty(PropertyName = "start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        [JsonProperty(PropertyName = "bucket_id")]
        public string BucketId { get; set; }

        [JsonProperty(PropertyName = "input_metadata")]
        public Dictionary<string, string> InputMetaData { get; set; }

        [JsonProperty(PropertyName = "tasks")]
        public List<TaskExecution> Tasks { get; set; }
    }
}
