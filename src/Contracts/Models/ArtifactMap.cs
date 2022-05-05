// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class ArtifactMap
    {
        [JsonProperty(PropertyName = "input")]
        public Artifact[] Input { get; set; }

        [JsonProperty(PropertyName = "output")]
        public Artifact[] Output { get; set; }
    }
}
