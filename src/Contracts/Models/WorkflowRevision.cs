using System;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class WorkflowRevision : ISoftDeleteable
    {
        [BsonId]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "workflow_id")]
        public string WorkflowId { get; set; }

        [JsonProperty(PropertyName = "revision")]
        public int Revision { get; set; }

        [JsonProperty(PropertyName = "workflow")]
        public Workflow Workflow { get; set; }

        [JsonIgnore]
        public DateTime? Deleted { get; set; } = null;

        [JsonIgnore]
        public bool IsDeleted { get => Deleted is not null; }
    }
}
