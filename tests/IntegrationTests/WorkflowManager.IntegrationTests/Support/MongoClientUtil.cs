// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public class MongoClientUtil
    {
        public MongoClientUtil()
        {
            Client = new MongoClient(TestExecutionConfig.MongoConfig.ConnectionString);
            Database = Client.GetDatabase($"{TestExecutionConfig.MongoConfig.Database}");
            WorkflowRevisionCollection = Database.GetCollection<WorkflowRevision>($"{TestExecutionConfig.MongoConfig.WorkflowCollection}");
            WorkflowInstanceCollection = Database.GetCollection<WorkflowInstance>($"{TestExecutionConfig.MongoConfig.WorkflowInstanceCollection}");
        }

        private MongoClient Client { get; set; }

        private IMongoDatabase Database { get; set; }

        private IMongoCollection<WorkflowRevision> WorkflowRevisionCollection { get; set; }

        private IMongoCollection<WorkflowInstance> WorkflowInstanceCollection { get; set; }

        public void CreateWorkflowRevisionDocument(WorkflowRevision workflowRevision)
        {
            WorkflowRevisionCollection.InsertOne(workflowRevision);
        }

        public void DeleteWorkflowDocument(string id)
        {
            WorkflowRevisionCollection.DeleteOne(x => x.WorkflowId.Equals(id));
        }

        public void DeleteAllWorkflowDocuments()
        {
            WorkflowRevisionCollection.DeleteMany("{ }");
        }

        public void CreateWorkflowInstanceDocument(WorkflowInstance workflowInstance)
        {
            WorkflowInstanceCollection.InsertOne(workflowInstance);
        }

        public WorkflowInstance GetWorkflowInstance(string payloadId)
        {
            return WorkflowInstanceCollection.Find(x => x.PayloadId == payloadId).FirstOrDefault();
        }

        public WorkflowInstance GetWorkflowInstanceById(string id)
        {
            return WorkflowInstanceCollection.Find(x => x.Id == id).FirstOrDefault();
        }

        public List<WorkflowInstance> GetWorkflowInstances(string payloadId)
        {
            return WorkflowInstanceCollection.Find(x => x.PayloadId == payloadId).ToList();
        }

        public void DeleteAllWorkflowInstances()
        {
            WorkflowInstanceCollection.DeleteMany("{ }");
        }

        public void DropDatabase(string dbName)
        {
            Client.DropDatabase(dbName);
        }
    }
}
