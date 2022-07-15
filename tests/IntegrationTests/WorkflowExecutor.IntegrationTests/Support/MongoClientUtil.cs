// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
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
        private IMongoCollection<Payload> PayloadCollection { get; set; }
        private RetryPolicy RetryMongo { get; set; }
        private RetryPolicy<List<Payload>> RetryPayload { get; set; }

        public MongoClientUtil()
        {
            Client = new MongoClient(TestExecutionConfig.MongoConfig.ConnectionString);
            Database = Client.GetDatabase($"{TestExecutionConfig.MongoConfig.Database}");
            WorkflowRevisionCollection = Database.GetCollection<WorkflowRevision>($"{TestExecutionConfig.MongoConfig.WorkflowCollection}");
            WorkflowInstanceCollection = Database.GetCollection<WorkflowInstance>($"{TestExecutionConfig.MongoConfig.WorkflowInstanceCollection}");
            PayloadCollection = Database.GetCollection<Payload>($"{TestExecutionConfig.MongoConfig.PayloadCollection}");
            RetryMongo = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1000));
            RetryPayload = Policy<List<Payload>>.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1000));
        }

        #region WorkflowRevision
        public void CreateWorkflowRevisionDocument(WorkflowRevision workflowRevision)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowRevisionCollection.InsertOne(workflowRevision);
            });
        }

        public void DeleteWorkflowRevisionDocument(string id)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowRevisionCollection.DeleteOne(x => x.Id.Equals(id));
            });
        }

        public void DeleteWorkflowRevisionDocumentByWorkflowId(string workflowId)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowRevisionCollection.DeleteMany(x => x.WorkflowId.Equals(workflowId));
            });
        }

        public void DeleteAllWorkflowRevisionDocuments()
        {
            WorkflowRevisionCollection.DeleteMany("{ }");
        }

        public List<WorkflowRevision> GetWorkflowRevisionsByWorkflowId(string workflowId)
        {
            return WorkflowRevisionCollection.Find(x => x.WorkflowId == workflowId).ToList();
        }
        #endregion

        #region WorkflowInstances
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
        #endregion

        #region Payload
        public void CreatePayloadDocument(Payload payload)
        {
            RetryMongo.Execute(() =>
            {
                PayloadCollection.InsertOne(payload);
            });
        }
        public List<Payload> GetPayloadCollectionByPayloadId(string payloadId)
        {
            var res = RetryPayload.Execute(() =>
            {
                var payloadCollection = PayloadCollection.Find(x => x.PayloadId == payloadId).ToList();
                if (payloadCollection.Count != 0)
                {
                    return payloadCollection;
                }
                else
                {
                    throw new Exception("Payload not found");
                }
            });
            return res;
        }

        public void DeletePayloadDocument(string id)
        {
            RetryMongo.Execute(() =>
            {
                PayloadCollection.DeleteOne(x => x.Id.Equals(id));
            });
        }

        public void DeletePayloadDocumentByPayloadId(string payloadId)
        {
            RetryMongo.Execute(() =>
            {
                PayloadCollection.DeleteMany(x => x.PayloadId.Equals(payloadId));
            });
        }

        public void DeleteAllPayloadDocuments()
        {
            PayloadCollection.DeleteMany("{ }");
        }
        #endregion

        public void DropDatabase(string dbName)
        {
            Client.DropDatabase(dbName);
        }
    }
}
