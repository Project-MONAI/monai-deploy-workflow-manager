using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Models
{
    public class Workflow
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
