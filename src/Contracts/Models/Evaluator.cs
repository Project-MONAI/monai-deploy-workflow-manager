using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class Evaluator
    {
        [JsonProperty(PropertyName = "correlation_id")]
        public string CorrelationId { get; set; }

        [JsonProperty(PropertyName = "input")]
        public Artifact Input { get; set; }

        [JsonProperty(PropertyName = "executions")]
        public ExecutionContext Executions { get; set; }

        [JsonProperty(PropertyName = "dicom")]
        public ExecutionContext Dicom { get; set; }
    }
}
