using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Models
{
    public class PatientMetadata
    {
        [JsonProperty(PropertyName = "patient_name")]
        [Required]
        public string PatientName { get; set; }

        [JsonProperty(PropertyName = "patient_id")]
        [Required]
        public string PatientId { get; set; }

        [JsonProperty(PropertyName = "patient_dob")]
        [Required]
        public string PatientDob { get; set; }

        [JsonProperty(PropertyName = "patient_sex")]
        [Required]
        public string PatientSex { get; set; }
    }
}
