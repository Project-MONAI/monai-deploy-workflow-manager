using Monai.Deploy.WorkflowManager.Contracts.Models;
using Newtonsoft.Json;
using System;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class WorkflowInstance
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "workflow_id")]
        public Guid WorkflowId { get; set; }

        [JsonProperty(PropertyName = "payload_id")]
        public Guid PayloadId { get; set; }

        [JsonProperty(PropertyName = "start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "bucket_id")]
        public string BucketId { get; set; }

        [JsonProperty(PropertyName = "input_metadata")]
        public InputMataData InputMataData { get; set; }

        [JsonProperty(PropertyName = "tasks")]
        public TaskObject[] Tasks { get; set; }
    }
}
