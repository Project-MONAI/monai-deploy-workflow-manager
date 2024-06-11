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

namespace Monai.Deploy.WorkflowManager.Common.Contracts.Models
{

    public class Workflow
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "informatics_gateway")]
        public InformaticsGateway? InformaticsGateway { get; set; }

        [JsonProperty(PropertyName = "tasks")]
        public TaskObject[] Tasks { get; set; } = System.Array.Empty<TaskObject>();

        [JsonProperty(PropertyName = "dataRetentionDays")]
        public int? DataRetentionDays { get; set; } = 3;// note. -1 = never delete

        [JsonProperty(PropertyName = "predicate")]
        public string[] Predicate { get; set; } = [];

    }
}
