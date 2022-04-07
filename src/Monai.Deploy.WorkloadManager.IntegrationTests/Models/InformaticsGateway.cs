using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Models
{
    public class InformaticsGateway
    {
        [JsonProperty(PropertyName = "ae_title")]
        public string AeTitle { get; set; }

        [JsonProperty(PropertyName = "data_origins")]
        public string[] DataOrigins { get; set; }

        [JsonProperty(PropertyName = "export_destinations")]
        public string[] ExportDestinations { get; set; }
    }
}
