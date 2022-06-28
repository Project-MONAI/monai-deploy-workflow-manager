// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class DicomValue
    {
        [JsonProperty(PropertyName = "vr")]
        public string Vr { get; set; }

        [JsonProperty(PropertyName = "Value")]
        public object[] Value { get; set; }
    }
}
