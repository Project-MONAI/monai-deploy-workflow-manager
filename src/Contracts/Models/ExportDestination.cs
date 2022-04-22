using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class ExportDestination
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "conditions")]
        public Evaluator[] Conditions { get; set; }

        [JsonProperty(PropertyName = "artifacts")]
        public Artifact[] Artifacts { get; set; }
    }
}
