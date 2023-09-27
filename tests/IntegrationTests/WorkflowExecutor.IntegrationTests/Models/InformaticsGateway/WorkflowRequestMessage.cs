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

using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.Models
{
    public class WorkflowRequestMessage
    {
        [JsonProperty(PropertyName = "payload_id")]
        public Guid PayloadId { get; set; }

        [JsonProperty(PropertyName = "workflows")]
        public IEnumerable<string> Workflows { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "file_count")]
        public int FileCount { get; set; }

        [JsonProperty(PropertyName = "correlation_id")]
        public string CorrelationId { get; set; } = default!;

        [JsonProperty(PropertyName = "bucket")]
        public string Bucket { get; set; } = default!;

        [JsonProperty(PropertyName = "trigger")]
        [JsonPropertyName("trigger")]
        public DataOrigin DataTrigger { get; set; } = new DataOrigin { DataService = DataService.DIMSE };

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
