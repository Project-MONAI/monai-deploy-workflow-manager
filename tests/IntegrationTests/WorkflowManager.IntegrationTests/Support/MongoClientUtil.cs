// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using MongoDB.Driver;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public class MongoClientUtil
    {
        private MongoClient Client { get; set; }
        private IMongoDatabase Database { get; set; }
        private IMongoCollection<WorkflowRevision> WorkflowRevisionCollection { get; set; }
        private IMongoCollection<WorkflowInstance> WorkflowInstanceCollection { get; set; }
        private RetryPolicy RetryMongo { get; set; }

        public MongoClientUtil()
        {
            Client = new MongoClient(TestExecutionConfig.MongoConfig.ConnectionString);
            Database = Client.GetDatabase($"{TestExecutionConfig.MongoConfig.Database}");
            WorkflowRevisionCollection = Database.GetCollection<WorkflowRevision>($"{TestExecutionConfig.MongoConfig.WorkflowCollection}");
            WorkflowInstanceCollection = Database.GetCollection<WorkflowInstance>($"{TestExecutionConfig.MongoConfig.WorkflowInstanceCollection}");
            RetryMongo = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1000));
        }

        public void CreateWorkflowRevisionDocument(WorkflowRevision workflowRevision)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowRevisionCollection.InsertOne(workflowRevision);
            });
        }

        public void DeleteWorkflowDocument(string id)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowRevisionCollection.DeleteOne(x => x.Id.Equals(id));
            });
        }

        public void DeleteAllWorkflowDocuments()
        {
            WorkflowRevisionCollection.DeleteMany("{ }");
        }

        public void CreateWorkflowInstanceDocument(WorkflowInstance workflowInstance)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowInstanceCollection.InsertOne(workflowInstance);
            });
        }

        public WorkflowInstance GetWorkflowInstance(string payloadId)
        {
            return WorkflowInstanceCollection.Find(x => x.PayloadId == payloadId).FirstOrDefault();
        }

        public WorkflowInstance GetWorkflowInstanceById(string Id)
        {
            return WorkflowInstanceCollection.Find(x => x.Id == Id).FirstOrDefault();
        }

        public List<WorkflowInstance> GetWorkflowInstancesByPayloadId(string payloadId)
        {
            return WorkflowInstanceCollection.Find(x => x.PayloadId == payloadId).ToList();
        }

        public void DeleteAllWorkflowInstances()
        {
            WorkflowInstanceCollection.DeleteMany("{ }");
        }

        public void DeleteWorkflowInstance(string id)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowInstanceCollection.DeleteOne(x => x.Id.Equals(id));
            });
        }

        public void DropDatabase(string dbName)
        {
            Client.DropDatabase(dbName);
        }
    }
}
