using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class InputArtifacts
    {
        [JsonProperty(PropertyName = "path_to_file")]
        public string Key { get; set; }
    }
}
