using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class TaskDestination
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "conditions")]
        public Evaluator[] Conditions { get; set; }
    }
}
