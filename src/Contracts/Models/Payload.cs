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

using System;
using System.Collections.Generic;
using Monai.Deploy.Messaging.Common;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class Payload
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "payload_id")]
        public string PayloadId { get; set; }

        [JsonProperty(PropertyName = "workflows")]
        public IEnumerable<string> Workflows { get; set; }

        [JsonProperty(PropertyName = "workflow_instance_ids")]
        public IEnumerable<string> WorkflowInstanceIds { get; set; }

        [JsonProperty(PropertyName = "file_count")]
        public int FileCount { get; set; }

        [JsonProperty(PropertyName = "correlation_id")]
        public string CorrelationId { get; set; }

        [JsonProperty(PropertyName = "bucket")]
        public string Bucket { get; set; }

        [JsonProperty(PropertyName = "calling_aetitle")]
        public string CallingAeTitle { get; set; }

        [JsonProperty(PropertyName = "called_aetitle")]
        public string CalledAeTitle { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty(PropertyName = "files")]
        public IList<BlockStorageInfo> Files { get; set; }

        [JsonProperty(PropertyName = "patient_details")]
        public PatientDetails PatientDetails { get; set; }
    }
}
