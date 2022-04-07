using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Models
{
    public class ExportMessageRequest
    {
        [JsonProperty(PropertyName = "payload_id")]
        public Guid PayloadId { get; set; }

        [JsonProperty(PropertyName = "workflows")]
        public IEnumerable<string> Workflows { get; set; }

        [JsonProperty(PropertyName = "file_count")]
        public int FileCount { get; set; }

        [JsonProperty(PropertyName = "correlation_id")]
        public string CorrelationId { get; set; } = default!;

        [JsonProperty(PropertyName = "bucket")]
        public string Bucket { get; set; } = default!;

        [JsonProperty(PropertyName = "calling_aetitle")]
        public string CallingAeTitle { get; set; } = default!;

        [JsonProperty(PropertyName = "called_aetitle")]
        public string CalledAeTitle { get; set; } = default!;

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
