using System;
using System.Collections.Generic;
using Monai.Deploy.Messaging.Common;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class Payload
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "payload_id")]
        public string PayloadId { get; set; }

        [JsonProperty(PropertyName = "workflows")]
        public IEnumerable<string> Workflows { get; set; }

        [JsonProperty(PropertyName = "workflow_instance_ids")]
        public IEnumerable<string> WorkflowInstanceIds { get; set; }

        [JsonProperty(PropertyName = "file_count")]
        public int FileCount { get; set; }

        [JsonProperty(PropertyName = "correlation_id")]
        public string CorrelationId { get; set; }

        [JsonProperty(PropertyName = "bucket")]
        public string Bucket { get; set; }

        [JsonProperty(PropertyName = "calling_aetitle")]
        public string CallingAeTitle { get; set; }

        [JsonProperty(PropertyName = "called_aetitle")]
        public string CalledAeTitle { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty(PropertyName = "files")]
        public IList<BlockStorageInfo> Files { get; set; }

        [JsonProperty(PropertyName = "patient_details")]
        public PatientDetails PatientDetails { get; set; }
    }
}
