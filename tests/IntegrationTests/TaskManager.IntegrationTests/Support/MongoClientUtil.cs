/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.POCO;
using MongoDB.Driver;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support
{
    public class MongoClientUtil
    {
        private MongoClient Client { get; set; }
        internal IMongoDatabase Database { get; set; }
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

        public void CreateTaskDispatchEventInfo(TaskDispatchEventInfo taskDispatchEventInfo)
        {
            RetryMongo.Execute(() =>
            {
                TaskDispatchEventInfoCollection.InsertOne(taskDispatchEventInfo);
            });
        }

        public List<TaskDispatchEventInfo> GetTaskDispatchEventInfoByExecutionId(string executionId)
        {
            var res = RetryTaskDispatchEventInfo.Execute(() =>
            {
                return TaskDispatchEventInfoCollection.Find(x => x.Event.ExecutionId == executionId).ToList();
            });
            return res;
        }

        public void DeleteAllTaskDispatch()
        {
            RetryMongo.Execute(() =>
            {
                TaskDispatchEventInfoCollection.DeleteMany("{ }");

                var taskDispatch = TaskDispatchEventInfoCollection.Find("{ }").ToList();

                if (taskDispatch.Count > 0)
                {
                    throw new Exception("All task Dispatch Events were not deleted!");
                }
            });
        }

        #endregion TaskDispatchEventInfo

        public void DropDatabase(string dbName)
        {
            Client.DropDatabase(dbName);
        }

        internal void ListAllCollections(ISpecFlowOutputHelper outputHelper, string testFeature)
        {
            var collections = Database.ListCollectionNames().ToList();
            outputHelper.WriteLine($"MongoDB collections found in test feature '{testFeature}': {collections.Count}");
            collections.ForEach(p => outputHelper.WriteLine($"- Collection: {p}"));
        }
    }
}
