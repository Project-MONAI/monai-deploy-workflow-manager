using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class InputMataData
    {
        [JsonProperty(PropertyName = "input_type")]
        public string InputType { get; set; }
    }
}
