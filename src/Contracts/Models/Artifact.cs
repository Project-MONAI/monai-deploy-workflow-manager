using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class Artifact
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
