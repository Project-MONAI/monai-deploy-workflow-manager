using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Models
{
    public class ArtifactMap
    {
        [JsonProperty(PropertyName = "input")]
        public Artifact[] Input { get; set; }

        [JsonProperty(PropertyName = "output")]
        public Artifact[] Output { get; set; }
    }
}
