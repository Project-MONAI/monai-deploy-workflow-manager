// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Diagnostics;
using BoDi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests
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

        private static RabbitPublisher? TaskDispatchPublisher { get; set; }
        private static RabbitPublisher? TaskCallbackPublisher { get; set; }
        private static RabbitConsumer? TaskUpdateConsumer { get; set; }
        public static RabbitConsumer? ClinicalReviewConsumer { get; private set; }
        private static MinioClientUtil? MinioClient { get; set; }
        private IObjectContainer ObjectContainer { get; set; }
        private static IHost? Host { get; set; }
        private static HttpClient? HttpClient { get; set; }

        /// <summary>
        /// Runs before all tests to create static implementions of Rabbit and Mongo clients as well as starting the WorkflowManager using WebApplicationFactory.
        /// </summary>
        [BeforeTestRun(Order = 0)]
        public static void Init()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json")
                .Build();

            TestExecutionConfig.RabbitConfig.Host = "localhost";
            TestExecutionConfig.RabbitConfig.Port = 15672;
            TestExecutionConfig.RabbitConfig.User = "admin";
            TestExecutionConfig.RabbitConfig.Password = "admin";
            TestExecutionConfig.RabbitConfig.VirtualHost = "monaideploy";
            TestExecutionConfig.RabbitConfig.Exchange = "monaideploy";
            TestExecutionConfig.RabbitConfig.TaskDispatchQueue = "md.tasks.dispatch";
            TestExecutionConfig.RabbitConfig.TaskCallbackQueue = "md.tasks.callback";
            TestExecutionConfig.RabbitConfig.TaskUpdateQueue = "md.tasks.update";
            TestExecutionConfig.RabbitConfig.ClinicalReviewQueue = "aide.clinical_review.request";

            TestExecutionConfig.MinioConfig.Endpoint = "localhost:9000";
            TestExecutionConfig.MinioConfig.AccessKey = "minioadmin";
            TestExecutionConfig.MinioConfig.AccessToken = "minioadmin";
            TestExecutionConfig.MinioConfig.Bucket = "test-bucket";
            TestExecutionConfig.MinioConfig.Region = "eu-west-2";

            Host = TaskManagerStartup.StartTaskManager();
            HttpClient = new HttpClient();
            MinioClient = new MinioClientUtil();
        }

        // <summary>
        // Runs before all tests to check that the TaskManager consumer is started.
        // </summary>
        // <returns>Error if the TaskManager consumer is not started.</returns>
        // <exception cref = "Exception" ></ exception >
        [BeforeTestRun(Order = 1)]
        public static async Task CheckTaskManagerConsumersStarted()
        {
            var response = await TaskManagerStartup.GetConsumers(HttpClient);
            var content = response.Content.ReadAsStringAsync().Result;

            for (var i = 1; i <= 10; i++)
            {
                if (string.IsNullOrEmpty(content) || content == "[]")
                {
                    Debug.Write($"Task Manager not started. Recheck times {i}");
                    response = await TaskManagerStartup.GetConsumers(HttpClient);
                    content = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    Debug.Write("Task Manager started. Integration tests will begin!");
                    break;
                }

                if (i == 10)
                {
                    throw new Exception("Task Manager not started! Integration tests will not continue");
                }

                Thread.Sleep(1000);
            }

            TaskDispatchPublisher = new RabbitPublisher(RabbitConnectionFactory.GetConnectionFactory(), TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskDispatchQueue);
            TaskCallbackPublisher = new RabbitPublisher(RabbitConnectionFactory.GetConnectionFactory(), TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskCallbackQueue);
            TaskUpdateConsumer = new RabbitConsumer(RabbitConnectionFactory.GetConnectionFactory(), TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskUpdateQueue);
            ClinicalReviewConsumer = new RabbitConsumer(RabbitConnectionFactory.GetConnectionFactory(), TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.ClinicalReviewQueue);
        }

        /// <summary>
        /// Adds Rabbit and Mongo clients to Specflow IoC container for test scenario being executed.
        /// </summary>
        [BeforeScenario]
        public void SetUp()
        {
            ObjectContainer.RegisterInstanceAs(TaskDispatchPublisher, "TaskDispatchPublisher");
            ObjectContainer.RegisterInstanceAs(TaskCallbackPublisher, "TaskCallbackPublisher");
            ObjectContainer.RegisterInstanceAs(TaskUpdateConsumer, "TaskUpdateConsumer");
            ObjectContainer.RegisterInstanceAs(ClinicalReviewConsumer, "ClinicalReviewConsumer");
            ObjectContainer.RegisterInstanceAs(MinioClient);
            var dataHelper = new DataHelper(ObjectContainer);
            ObjectContainer.RegisterInstanceAs(dataHelper);
        }

        /// <summary>
        /// Runs after all tests to closes Rabbit connections.
        /// </summary>
        [AfterTestRun(Order = 1)]
        public static void TearDownRabbit()
        {
            TaskDispatchPublisher?.CloseConnection();
            TaskUpdateConsumer?.CloseConnection();
            TaskCallbackPublisher?.CloseConnection();
            ClinicalReviewConsumer?.CloseConnection();
            Host?.StopAsync();
        }
    }
}
