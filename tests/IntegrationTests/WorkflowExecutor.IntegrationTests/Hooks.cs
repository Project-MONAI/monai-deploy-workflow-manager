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

using BoDi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Polly;
using Polly.Retry;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManagerIntegrationTests
{
    /// <summary>
    /// Hooks class for setting up the integration tests.
    /// </summary>
    [Binding]
    public class Hooks
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hooks"/> class.
        /// </summary>
        /// <param name="objectContainer"></param>
        public Hooks(IObjectContainer objectContainer)
        {
            ObjectContainer = objectContainer;
        }

        private static HttpClient? HttpClient { get; set; }
        public static AsyncRetryPolicy? RetryPolicy { get; private set; }
        private static RabbitPublisher? WorkflowPublisher { get; set; }
        private static RabbitConsumer? TaskDispatchConsumer { get; set; }
        private static RabbitPublisher? TaskUpdatePublisher { get; set; }
        private static RabbitConsumer? ExportRequestConsumer { get; set; }
        private static RabbitPublisher? ExportCompletePublisher { get; set; }
        private static MongoClientUtil? MongoClient { get; set; }
        private static MinioClientUtil? MinioClient { get; set; }
        private IObjectContainer ObjectContainer { get; set; }
        private static IHost? Host { get; set; }

        /// <summary>
        /// Runs before all tests to create static implementions of Rabbit and Mongo clients as well as starting the WorkflowManager using WebApplicationFactory.
        /// </summary>
        [BeforeTestRun(Order = 0)]
        public static void Init()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.Test.json")
                .Build();

            TestExecutionConfig.RabbitConfig.Host = config.GetValue<string>("WorkflowManager:messaging:publisherSettings:endpoint");
            TestExecutionConfig.RabbitConfig.WebPort = 15672;
            TestExecutionConfig.RabbitConfig.User = config.GetValue<string>("WorkflowManager:messaging:publisherSettings:username");
            TestExecutionConfig.RabbitConfig.Password = config.GetValue<string>("WorkflowManager:messaging:publisherSettings:password");
            TestExecutionConfig.RabbitConfig.VirtualHost = config.GetValue<string>("WorkflowManager:messaging:publisherSettings:virtualHost");
            TestExecutionConfig.RabbitConfig.Exchange = config.GetValue<string>("WorkflowManager:messaging:publisherSettings:exchange");
            TestExecutionConfig.RabbitConfig.WorkflowRequestQueue = config.GetValue<string>("WorkflowManager:messaging:topics:workflowRequest");
            TestExecutionConfig.RabbitConfig.TaskDispatchQueue = "md.tasks.dispatch";
            TestExecutionConfig.RabbitConfig.TaskCallbackQueue = "md.tasks.callback";
            TestExecutionConfig.RabbitConfig.TaskUpdateQueue = "md.tasks.update";
            TestExecutionConfig.RabbitConfig.ExportCompleteQueue = config.GetValue<string>("WorkflowManager:messaging:topics:exportComplete");
            TestExecutionConfig.RabbitConfig.ExportRequestQueue = $"{config.GetValue<string>("WorkflowManager:messaging:topics:exportRequestPrefix")}.{config.GetValue<string>("WorkflowManager:messaging:dicomAgents:scuAgentName")}";

            TestExecutionConfig.MongoConfig.ConnectionString = config.GetValue<string>("WorkloadManagerDatabase:ConnectionString");
            TestExecutionConfig.MongoConfig.Database = config.GetValue<string>("WorkloadManagerDatabase:DatabaseName");
            TestExecutionConfig.MongoConfig.WorkflowCollection = config.GetValue<string>("WorkloadManagerDatabase:WorkflowCollectionName");
            TestExecutionConfig.MongoConfig.WorkflowInstanceCollection = config.GetValue<string>("WorkloadManagerDatabase:WorkflowInstanceCollectionName");
            TestExecutionConfig.MongoConfig.PayloadCollection = config.GetValue<string>("WorkloadManagerDatabase:PayloadCollectionName");

            TestExecutionConfig.MinioConfig.Endpoint = config.GetValue<string>("WorkflowManager:storage:settings:endpoint");
            TestExecutionConfig.MinioConfig.AccessKey = config.GetValue<string>("WorkflowManager:storage:settings:accessKey");
            TestExecutionConfig.MinioConfig.AccessToken = config.GetValue<string>("WorkflowManager:storage:settings:accessToken");
            TestExecutionConfig.MinioConfig.Bucket = config.GetValue<string>("WorkflowManager:storage:settings:bucket");
            TestExecutionConfig.MinioConfig.Region = config.GetValue<string>("WorkflowManager:storage:settings:region");

            TestExecutionConfig.ApiConfig.BaseUrl = "http://localhost:5000";

            RabbitConnectionFactory.DeleteAllQueues();

            MongoClient = new MongoClientUtil();
            MinioClient = new MinioClientUtil();
            HttpClient = new HttpClient();
            RetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(retryCount: 20, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));

            Host = WorkflowExecutorStartup.StartWorkflowExecutor();
            RabbitConnectionFactory.SetRabbitConnection();
        }

        /// <summary>
        /// Runs before all tests to check that the WorkflowManager consumer is started.
        /// </summary>
        /// <returns>Error if the WorkflowManager consumer is not started.</returns>
        /// <exception cref="Exception"></exception>
        [BeforeTestRun(Order = 1)]
        public static async Task CheckWorkflowConsumerStarted()
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                var response = await WorkflowExecutorStartup.GetQueueStatus(HttpClient, TestExecutionConfig.RabbitConfig.VirtualHost, TestExecutionConfig.RabbitConfig.TaskUpdateQueue);
                var content = response.Content.ReadAsStringAsync().Result;

                if (content.Contains("error"))
                {
                    throw new Exception("Workflow Executor not started!");
                }
                else
                {
                    Console.WriteLine("Workfow Executor started. Integration Tests will begin.");
                }
            });

            WorkflowPublisher = new RabbitPublisher(TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.WorkflowRequestQueue);
            TaskDispatchConsumer = new RabbitConsumer(TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskDispatchQueue);
            TaskUpdatePublisher = new RabbitPublisher(TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskUpdateQueue);
            ExportCompletePublisher = new RabbitPublisher(TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.ExportCompleteQueue);
            ExportRequestConsumer = new RabbitConsumer(TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.ExportRequestQueue);
        }

        /// <summary>
        /// Adds Rabbit and Mongo clients to Specflow IoC container for test scenario being executed.
        /// </summary>
        [BeforeScenario]
        public void SetUp(ScenarioContext scenarioContext, ISpecFlowOutputHelper outputHelper)
        {
            ObjectContainer.RegisterInstanceAs(WorkflowPublisher, "WorkflowPublisher");
            ObjectContainer.RegisterInstanceAs(TaskDispatchConsumer, "TaskDispatchConsumer");
            ObjectContainer.RegisterInstanceAs(TaskUpdatePublisher, "TaskUpdatePublisher");
            ObjectContainer.RegisterInstanceAs(ExportCompletePublisher, "ExportCompletePublisher");
            ObjectContainer.RegisterInstanceAs(ExportRequestConsumer, "ExportRequestConsumer");
            ObjectContainer.RegisterInstanceAs(MongoClient);
            ObjectContainer.RegisterInstanceAs(MinioClient);
            var dataHelper = new DataHelper(TaskDispatchConsumer, ExportRequestConsumer, MongoClient);
            ObjectContainer.RegisterInstanceAs(dataHelper);
            var apiHelper = new ApiHelper(HttpClient);
            ObjectContainer.RegisterInstanceAs(apiHelper);

            MongoClient.ListAllCollections(outputHelper, scenarioContext.ScenarioInfo.Title);
        }

        [BeforeTestRun(Order = 2)]
        [AfterTestRun(Order = 0)]
        [AfterScenario]
        public static void ClearTestData()
        {
            RabbitConnectionFactory.PurgeAllQueues();
            MongoClient?.DeleteAllWorkflowRevisionDocuments();
            MongoClient?.DeleteAllWorkflowInstances();
            MongoClient?.DeleteAllPayloadDocuments();
        }

        [BeforeTestRun(Order = 3)]
        public static async Task CreateBucket()
        {
            await MinioClient.CreateBucket(TestExecutionConfig.MinioConfig.Bucket);
        }

        /// <summary>
        /// Runs after all tests to closes Rabbit connections.
        /// </summary>
        [AfterTestRun(Order = 1)]
        public static void StopServices()
        {
            Host?.StopAsync();
        }
    }
}
