using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class Evaluator
    {
        [JsonProperty(PropertyName = "correlation_id")]
        public string CorrelationId { get; set; }
    }
}
