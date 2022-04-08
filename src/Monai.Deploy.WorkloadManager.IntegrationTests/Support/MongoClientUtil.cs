using Monai.Deploy.WorkloadManager.IntegrationTests.Models;
using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
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
                $"{TestExecutionConfig.MongoConfig.Port}";
            Client = new MongoClient(connectionString);
        }

        private MongoClient Client { get; set; }

        private IMongoDatabase Database { get; set; }

        private IMongoCollection<DummyDag> DummyDagCollection { get; set; }

        public void GetDatabase(string dbName)
        {
            Database = Client.GetDatabase($"{dbName}");
        }

        public void GetDagCollection(string name)
        {
            DummyDagCollection = Database.GetCollection<DummyDag>($"{name}");
        }

        public void CreateDummyDagDocument(DummyDag dummyDag)
        {
            DummyDagCollection.InsertOne(dummyDag);
        }

        public DummyDag GetDummyDagDocument(string id)
        {
            return DummyDagCollection.Find(x => x.Id == id).FirstOrDefault();
        }

        public async Task<IList<DummyDag>> GetAllDummyDags()
        {
            return await DummyDagCollection.Find(_ => true).ToListAsync();
        }

        public void DropDatabase(string dbName)
        {
            Client.DropDatabase(dbName);
        }
    }
}
