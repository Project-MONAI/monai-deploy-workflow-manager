// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
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

        [JsonProperty(PropertyName = "ref")]
        public string Ref { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "task_destinations")]
        public TaskDestination[] TaskDestinations { get; set; } = System.Array.Empty<TaskDestination>();

        [JsonProperty(PropertyName = "export_destinations")]
        public TaskDestination[] ExportDestinations { get; set; } = System.Array.Empty<TaskDestination>();

        [JsonProperty(PropertyName = "artifacts")]
        public ArtifactMap Artifacts { get; set; } = new ArtifactMap();

        [JsonProperty(PropertyName = "input_parameters")]
        public Dictionary<string, object>? InputParameters { get; set; }
    }
}
