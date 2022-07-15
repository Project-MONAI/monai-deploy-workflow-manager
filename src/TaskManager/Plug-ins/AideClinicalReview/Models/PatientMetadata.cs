using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Models
{
    public class PatientMetadata
    {
        [JsonProperty(PropertyName = "patient_name")]
        public string PatientName { get; set; }

        [JsonProperty(PropertyName = "patient_id")]
        public string PatientId { get; set; }

        [JsonProperty(PropertyName = "patient_dob")]
        public string PatientDob { get; set; }

        [JsonProperty(PropertyName = "patient_sex")]
        public string PatientSex { get; set; }

        [JsonProperty(PropertyName = "patient_age")]
        public string PatientAge { get; set; }

        [JsonProperty(PropertyName = "patient_hospital_id")]
        public string PatientHospitalId { get; set; }
    }
}
