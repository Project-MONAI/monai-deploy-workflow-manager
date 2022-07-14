// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Diagnostics;
using BoDi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;

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
        private static RabbitPublisher? WorkflowPublisher { get; set; }
        private static RabbitConsumer? TaskDispatchConsumer { get; set; }
        private static RabbitPublisher? TaskUpdatePublisher { get; set; }
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
                .AddJsonFile("appsettings.Development.json")
                .Build();

            TestExecutionConfig.RabbitConfig.Host = "localhost";
            TestExecutionConfig.RabbitConfig.Port = 15672;
            TestExecutionConfig.RabbitConfig.User = "admin";
            TestExecutionConfig.RabbitConfig.Password = "admin";
            TestExecutionConfig.RabbitConfig.VirtualHost = "monaideploy";
            TestExecutionConfig.RabbitConfig.Exchange = "monaideploy";
            TestExecutionConfig.RabbitConfig.WorkflowRequestQueue = config.GetValue<string>("WorkflowManager:messaging:topics:workflowRequest");
            TestExecutionConfig.RabbitConfig.TaskDispatchQueue = "md.tasks.dispatch";
            TestExecutionConfig.RabbitConfig.TaskCallbackQueue = "md.tasks.callback";
            TestExecutionConfig.RabbitConfig.TaskUpdateQueue = "md.tasks.update";
            TestExecutionConfig.RabbitConfig.WorkflowCompleteQueue = config.GetValue<string>("WorkflowManager:messaging:topics:exportComplete");

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

            MongoClient = new MongoClientUtil();
            MinioClient = new MinioClientUtil();
            Host = WorkflowExecutorStartup.StartWorkflowExecutor();
            HttpClient = new HttpClient();
        }

        /// <summary>
        /// Runs before all tests to check that the WorkflowManager consumer is started.
        /// </summary>
        /// <returns>Error if the WorkflowManager consumer is not started.</returns>
        /// <exception cref="Exception"></exception>
        [BeforeTestRun(Order = 1)]
        public static async Task CheckWorkflowConsumerStarted()
        {
            var response = await WorkflowExecutorStartup.GetConsumers(HttpClient);
            var content = response.Content.ReadAsStringAsync().Result;

            for (var i = 1; i <= 10; i++)
            {
                if (string.IsNullOrEmpty(content) || content == "[]")
                {
                    Debug.Write($"Workflow consumer not started. Recheck times {i}");
                    response = await WorkflowExecutorStartup.GetConsumers(HttpClient);
                    content = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    Debug.Write("Consumer started. Integration tests will begin!");
                    break;
                }

                if (i == 10)
                {
                    throw new Exception("Workflow Mangaer Consumer not started! Integration tests will not continue");
                }

                Thread.Sleep(1000);
            }

            WorkflowPublisher = new RabbitPublisher(RabbitConnectionFactory.GetConnectionFactory(), TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.WorkflowRequestQueue);
            TaskDispatchConsumer = new RabbitConsumer(RabbitConnectionFactory.GetConnectionFactory(), TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskDispatchQueue);
            TaskUpdatePublisher = new RabbitPublisher(RabbitConnectionFactory.GetConnectionFactory(), TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskUpdateQueue);
        }

        /// <summary>
        /// Adds Rabbit and Mongo clients to Specflow IoC container for test scenario being executed.
        /// </summary>
        [BeforeScenario]
        public void SetUp()
        {
            ObjectContainer.RegisterInstanceAs(WorkflowPublisher, "WorkflowPublisher");
            ObjectContainer.RegisterInstanceAs(TaskDispatchConsumer, "TaskDispatchConsumer");
            ObjectContainer.RegisterInstanceAs(TaskUpdatePublisher, "TaskUpdatePublisher");
            ObjectContainer.RegisterInstanceAs(MongoClient);
            ObjectContainer.RegisterInstanceAs(MinioClient);
            var dataHelper = new DataHelper(TaskDispatchConsumer, MongoClient);
            ObjectContainer.RegisterInstanceAs(dataHelper);
            var apiHelper = new ApiHelper(HttpClient);
            ObjectContainer.RegisterInstanceAs(apiHelper);
        }

        [BeforeTestRun(Order = 1)]
        [AfterTestRun(Order = 0)]
        public static void ClearTestData()
        {
            MongoClient?.DeleteAllWorkflowRevisionDocuments();
            MongoClient?.DeleteAllWorkflowInstances();
            MongoClient?.DeleteAllPayloadDocuments();
        }

        [AfterScenario]
        public void DeleteTestData()
        {
            var dataHelper = ObjectContainer.Resolve<DataHelper>();

            if (dataHelper.WorkflowRevisions.Count > 0)
            {
                foreach (var workflowRevision in dataHelper.WorkflowRevisions)
                {
                    MongoClient?.DeleteWorkflowRevisionDocumentByWorkflowId(workflowRevision.WorkflowId);
                }
            }

            if (dataHelper.WorkflowInstances.Count > 0)
            {
                foreach (var workflowInstance in dataHelper.WorkflowInstances)
                {
                    MongoClient?.DeleteWorkflowInstance(workflowInstance.Id);
                }
            }

            if (dataHelper.Payload.Count > 0)
            {
                foreach (var payload in dataHelper.Payload)
                {
                    MongoClient?.DeletePayloadDocumentByPayloadId(payload.PayloadId);
                }
            }

            if (dataHelper.WorkflowRequestMessage != null)
            {
                MongoClient?.DeletePayloadDocumentByPayloadId(dataHelper.WorkflowRequestMessage.PayloadId.ToString());

                foreach (var workflowRevision in dataHelper.WorkflowRevisions)
                {
                    MongoClient.DeleteWorkflowRevisionDocumentByWorkflowId(workflowRevision.WorkflowId);
                }
            }
        }

        /// <summary>
        /// Runs after all tests to closes Rabbit connections.
        /// </summary>
        [AfterTestRun(Order = 1)]
        public static void StopServices()
        {
            WorkflowPublisher?.CloseConnection();

            TaskDispatchConsumer?.CloseConnection();

            //Host?.StopAsync();
        }
    }
}
