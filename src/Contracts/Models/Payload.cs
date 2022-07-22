using System;
using System.Collections.Generic;
using Monai.Deploy.Messaging.Common;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class Payload
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "payload_id")]
        public string PayloadId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "workflows")]
        public IEnumerable<string> Workflows { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "workflow_instance_ids")]
        public IEnumerable<string> WorkflowInstanceIds { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "file_count")]
        public int FileCount { get; set; }

        [JsonProperty(PropertyName = "correlation_id")]
        public string CorrelationId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "bucket")]
        public string Bucket { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "calling_aetitle")]
        public string CallingAeTitle { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "called_aetitle")]
        public string CalledAeTitle { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty(PropertyName = "files")]
        public IList<BlockStorageInfo> Files { get; set; } = new List<BlockStorageInfo>();

        [JsonProperty(PropertyName = "patient_details")]
        public PatientDetails PatientDetails { get; set; } = new PatientDetails();
    }
}
