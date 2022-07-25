// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using MongoDB.Driver;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support
{
    public class MongoClientUtil
    {
        private MongoClient Client { get; set; }
        private IMongoDatabase Database { get; set; }
        private IMongoCollection<TaskDispatchEventInfo> TaskDispatchEventInfoCollection { get; set; }
        private RetryPolicy RetryMongo { get; set; }
        private RetryPolicy<List<TaskDispatchEventInfo>> RetryTaskDispatchEventInfo { get; set; }

        public MongoClientUtil()
        {
            Client = new MongoClient(TestExecutionConfig.MongoConfig.ConnectionString);
            Database = Client.GetDatabase($"{TestExecutionConfig.MongoConfig.Database}");
            TaskDispatchEventInfoCollection = Database.GetCollection<TaskDispatchEventInfo>($"{TestExecutionConfig.MongoConfig.TaskDispatchEventCollection}");
            RetryMongo = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1000));
            RetryTaskDispatchEventInfo = Policy<List<TaskDispatchEventInfo>>.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1000));
        }

        #region TaskDispatchEventInfo

        public List<TaskDispatchEventInfo> GetTaskDispatchEventInfoByExecutionId(string executionId)
        {
            var res = RetryTaskDispatchEventInfo.Execute(() =>
            {
                return TaskDispatchEventInfoCollection.Find(x => x.Event.ExecutionId == executionId).ToList();
            });
            return res;
        }

        #endregion TaskDispatchEventInfo

        public void DropDatabase(string dbName)
        {
            Client.DropDatabase(dbName);
        }
    }
}
