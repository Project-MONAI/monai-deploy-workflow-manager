using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Support
{
    public class MongoClientUtil
    {
        public MongoClientUtil()
        {
            var connectionString = $"mongodb://{TestExecutionConfig.MongoConfig.User}:" +
                $"{TestExecutionConfig.MongoConfig.Password}@" +
                $"{TestExecutionConfig.MongoConfig.Host}:" +
                $"{TestExecutionConfig.MongoConfig.Password}";
            Client = new MongoClient(connectionString);
        }

        private MongoClient Client { get; set; }

        private IMongoDatabase Database { get; set; }

        public void GetDatabase(string dbName)
        {
            Database = Client.GetDatabase($"{dbName}");
        }

        public void CreateCollection(string collectionName)
        {
            Database.CreateCollection($"{collectionName}");
        }

        public void CreateDagDocument(BsonDocument dag)
        {
            var collection = Database.GetCollection<BsonDocument>("workflow_dag");
            collection.InsertOne(dag);
        }

        public BsonDocument GetWorkflowDocument()
        {
            var collection = Database.GetCollection<BsonDocument>("workflows");
            return collection.Find(new BsonDocument()).FirstOrDefault();
        }
    }
}
