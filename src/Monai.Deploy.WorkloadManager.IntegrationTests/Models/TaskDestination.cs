using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Models
{
    public class TaskDestination
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "conditions")]
        public Evaluator[] Conditions { get; set; }
    }
}
