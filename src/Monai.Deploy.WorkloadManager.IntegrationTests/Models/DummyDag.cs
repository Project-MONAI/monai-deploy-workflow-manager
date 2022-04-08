using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Models
{
    public class DummyDag
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("WorkflowName")]
        public string WorkflowName { get; set; } = null!;

        [BsonElement("TaskName")]
        public string TaskName { get; set; }
    }
}
