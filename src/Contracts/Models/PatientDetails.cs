using System;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class PatientDetails
    {
        [JsonProperty(PropertyName = "patient_id")]
        public string PatientId { get; set; }

        [JsonProperty(PropertyName = "patient_name")]
        public string PatientName { get; set; }

        [JsonProperty(PropertyName = "patient_sex")]
        public string PatientSex { get; set; }

        [JsonProperty(PropertyName = "patient_dob")]
        public DateTime? PatientDob { get; set; }
    }
}
