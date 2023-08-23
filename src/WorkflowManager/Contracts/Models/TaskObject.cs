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
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Common.Contracts.Models
{
    public class TaskObject
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "args")]
        public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();

        [JsonProperty(PropertyName = "task_destinations")]
        public TaskDestination[] TaskDestinations { get; set; } = Array.Empty<TaskDestination>();

        [JsonProperty(PropertyName = "export_destinations")]
        public ExportDestination[] ExportDestinations { get; set; } = Array.Empty<ExportDestination>();

        [JsonProperty(PropertyName = "artifacts")]
        public ArtifactMap Artifacts { get; set; } = new ArtifactMap();

        [JsonProperty(PropertyName = "timeout_minutes")]
        public double TimeoutMinutes { get; set; } = -1;
    }
}
