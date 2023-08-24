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

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Monai.Deploy.WorkflowManager.Common.TaskManager.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests
{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
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

        private static RabbitPublisher? TaskDispatchPublisher { get; set; }
        private static RabbitPublisher? TaskCallbackPublisher { get; set; }
        private static RabbitConsumer? TaskUpdateConsumer { get; set; }
        public static RabbitConsumer? ClinicalReviewConsumer { get; private set; }
        public static RabbitConsumer? EmailConsumer { get; private set; }
        private static MinioClientUtil? MinioClient { get; set; }
        private static MongoClientUtil? MongoClient { get; set; }
        public static AsyncRetryPolicy? RetryPolicy { get; private set; }
        private IObjectContainer ObjectContainer { get; set; }
        private static HttpClient? HttpClient { get; set; }
        private static WebApplicationFactory<Program>? WebApplicationFactory { get; set; }

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
            TestExecutionConfig.RabbitConfig.TaskDispatchQueue = "md.tasks.dispatch";
            TestExecutionConfig.RabbitConfig.TaskCallbackQueue = "md.tasks.callback";
            TestExecutionConfig.RabbitConfig.TaskUpdateQueue = "md.tasks.update";
            TestExecutionConfig.RabbitConfig.ClinicalReviewQueue = "aide.clinical_review.request";
            TestExecutionConfig.RabbitConfig.EmailQueue = "aide.notification_email.request";
            TestExecutionConfig.RabbitConfig.TaskCancellationQueue = "md.tasks.cancellation";

            TestExecutionConfig.MongoConfig.ConnectionString = config.GetValue<string>("WorkloadManagerDatabase:ConnectionString");
            TestExecutionConfig.MongoConfig.Database = config.GetValue<string>("WorkloadManagerDatabase:DatabaseName");
            TestExecutionConfig.MongoConfig.TaskDispatchEventCollection = "TaskDispatchEvents";
            TestExecutionConfig.MongoConfig.ExecutionStatsCollection = "ExecutionStats";

            TestExecutionConfig.MinioConfig.Endpoint = config.GetValue<string>("WorkflowManager:storage:settings:endpoint");
            TestExecutionConfig.MinioConfig.AccessKey = config.GetValue<string>("WorkflowManager:storage:settings:accessKey");
            TestExecutionConfig.MinioConfig.AccessToken = config.GetValue<string>("WorkflowManager:storage:settings:accessToken");
            TestExecutionConfig.MinioConfig.Bucket = config.GetValue<string>("WorkflowManager:storage:settings:bucket");
            TestExecutionConfig.MinioConfig.Region = config.GetValue<string>("WorkflowManager:storage:settings:region");

            TestExecutionConfig.ApiConfig.TaskManagerBaseUrl = "http://localhost:5000";

            RabbitConnectionFactory.DeleteAllQueues();

            WebApplicationFactory = WebAppFactory.GetWebApplicationFactory();
            HttpClient = WebApplicationFactory?.CreateClient();

            RetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(retryCount: 20, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
            MongoClient = new MongoClientUtil();
            MinioClient = new MinioClientUtil();

            RabbitConnectionFactory.SetRabbitConnection();
        }

        [BeforeTestRun(Order = 2)]
        [AfterTestRun(Order = 0)]
        [AfterScenario]
        public static void ClearTestData()
        {
            RabbitConnectionFactory.PurgeAllQueues();

            MongoClient?.DeleteAllTaskDispatch();
        }

        // <summary>
        // Runs before all tests to check that the TaskManager consumer is started.
        // </summary>
        // <returns>Error if the TaskManager consumer is not started.</returns>
        // <exception cref = "Exception" ></ exception >
        [BeforeTestRun(Order = 1)]
        public static async Task CheckTaskManagerConsumersStarted()
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                var response = await WebAppFactory.GetQueueStatus(HttpClient, TestExecutionConfig.RabbitConfig.VirtualHost, TestExecutionConfig.RabbitConfig.TaskDispatchQueue);
                var content = response.Content.ReadAsStringAsync().Result;

                if (content.Contains("error"))
                {
                    throw new Exception("Task Manager not started!");
                }
                else
                {
                    Console.WriteLine("Task Manager started. Integration Tests will begin.");
                }
            });

            TaskDispatchPublisher = new RabbitPublisher(TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskDispatchQueue);
            TaskCallbackPublisher = new RabbitPublisher(TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskCallbackQueue);
            TaskUpdateConsumer = new RabbitConsumer(TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskUpdateQueue);
            ClinicalReviewConsumer = new RabbitConsumer(TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.ClinicalReviewQueue);
            EmailConsumer = new RabbitConsumer(TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.EmailQueue);
        }

        [BeforeTestRun(Order = 3)]
        public static async Task CreateBucket()
        {
            await MinioClient.CreateBucket(TestExecutionConfig.MinioConfig.Bucket);
        }

        /// <summary>
        /// Adds Rabbit and Mongo clients to Specflow IoC container for test scenario being executed.
        /// </summary>
        [BeforeScenario]
        public void SetUp(ScenarioContext scenarioContext, ISpecFlowOutputHelper outputHelper)
        {
            ObjectContainer.RegisterInstanceAs(TaskDispatchPublisher, "TaskDispatchPublisher");
            ObjectContainer.RegisterInstanceAs(TaskCallbackPublisher, "TaskCallbackPublisher");
            ObjectContainer.RegisterInstanceAs(TaskUpdateConsumer, "TaskUpdateConsumer");
            ObjectContainer.RegisterInstanceAs(ClinicalReviewConsumer, "ClinicalReviewConsumer");
            ObjectContainer.RegisterInstanceAs(EmailConsumer, "EmailConsumer");
            ObjectContainer.RegisterInstanceAs(MinioClient);
            var dataHelper = new DataHelper(ObjectContainer);
            ObjectContainer.RegisterInstanceAs(dataHelper);

            var apiHelper = new ApiHelper(HttpClient ?? throw new ArgumentException("No HttpClient"));
            ObjectContainer.RegisterInstanceAs(apiHelper);

            MongoClient!.ListAllCollections(outputHelper, scenarioContext.ScenarioInfo.Title);
        }

        /// <summary>
        /// Runs after all tests to closes Rabbit connections.
        /// </summary>
        [AfterTestRun(Order = 1)]
        public static void TearDownRabbit()
        {
            RabbitConnectionFactory.DeleteAllQueues();
            WebApplicationFactory?.Dispose();
        }
    }
}
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
