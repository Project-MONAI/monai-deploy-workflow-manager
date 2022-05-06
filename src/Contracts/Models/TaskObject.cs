// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class TaskObject
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "args")]
        public Dictionary<string, string> Args { get; set; }

        [JsonProperty(PropertyName = "ref")]
        public string Ref { get; set; }

        [JsonProperty(PropertyName = "task_destinations")]
        public TaskDestination[] TaskDestinations { get; set; }

        [JsonProperty(PropertyName = "export_destinations")]
        public TaskDestination[] ExportDestinations { get; set; }

        [JsonProperty(PropertyName = "artifacts")]
        public ArtifactMap Artifacts { get; set; }
    }
}
