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
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Common.Contracts.Models
{
    public class PatientDetails
    {
        [JsonProperty(PropertyName = "patient_id")]
        public string? PatientId { get; set; }

        [JsonProperty(PropertyName = "patient_name")]
        public string? PatientName { get; set; }

        [JsonProperty(PropertyName = "patient_sex")]
        public string? PatientSex { get; set; }

        [JsonProperty(PropertyName = "patient_dob")]
        public DateTime? PatientDob { get; set; }

        [JsonProperty(PropertyName = "patient_age")]
        public string? PatientAge { get; set; }

        [JsonProperty(PropertyName = "patient_hospital_id")]
        public string? PatientHospitalId { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
