
using BoDi;
using Microsoft.Extensions.Configuration;
using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
using Monai.Deploy.WorkloadManager.IntegrationTests.Support;

namespace Monai.Deploy.WorkloadManager.IntegrationTests
{
    [Binding]
    public class Hooks
    {
        public Hooks(IObjectContainer objectContainer)
        {
            IocServiceLocator.Init(objectContainer);
            ObjectContainer = objectContainer;
        }

        private IObjectContainer ObjectContainer { get; set; }

        private static RabbitClientUtil RabbitClient { get; set; }

        private static MongoClientUtil MongoClient { get; set; }

        [BeforeTestRun]
        public static void Init()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json")
                .Build();

            TestExecutionConfig.RabbitConfig.Host = config.GetValue<string>("TestExecutionConfig:RabbitConfig:Host");
            TestExecutionConfig.RabbitConfig.Port = config.GetValue<int>("TestExecutionConfig:RabbitConfig:Port");
            TestExecutionConfig.RabbitConfig.User = config.GetValue<string>("TestExecutionConfig:RabbitConfig:User");
            TestExecutionConfig.RabbitConfig.Password = config.GetValue<string>("TestExecutionConfig:RabbitConfig:Password");
            TestExecutionConfig.RabbitConfig.WorkflowRequestQueue = config.GetValue<string>("TestExecutionConfig:RabbitConfig:WorkflowRequestQueue");
            TestExecutionConfig.RabbitConfig.TaskDispatchQueue = config.GetValue<string>("TestExecutionConfig:RabbitConfig:TaskDispatchQueue");
            TestExecutionConfig.RabbitConfig.TaskCallbackQueue = config.GetValue<string>("TestExecutionConfig:RabbitConfig:TaskCallbacQueue");
            TestExecutionConfig.RabbitConfig.WorkflowCompleteQueue = config.GetValue<string>("TestExecutionConfig:RabbitConfig:WorkflowCompleteQueue");

            TestExecutionConfig.MongoConfig.Host = config.GetValue<string>("TestExecutionConfig:MongoConfig:Host");
            TestExecutionConfig.MongoConfig.Port = config.GetValue<int>("TestExecutionConfig:MongoConfig:Port");
            TestExecutionConfig.MongoConfig.User = config.GetValue<string>("TestExecutionConfig:MongoConfig:User");
            TestExecutionConfig.MongoConfig.Password = config.GetValue<string>("TestExecutionConfig:MongoConfig:Password");

            RabbitClient = new RabbitClientUtil();
            //MongoClient = new MongoClientUtil();
        }

        [BeforeScenario]
        public void ClientDependencies()
        {
            ObjectContainer.RegisterInstanceAs(RabbitClient);
            //ObjectContainer.RegisterInstanceAs(MongoClient);
        }

        [AfterScenario]
        public void PurgeQueue()
        {
            RabbitClient.PurgeQueue(TestExecutionConfig.RabbitConfig.WorkflowRequestQueue); // only purges unacked messages
        }

        [AfterTestRun]
        public static void TearDown()
        {
            RabbitClient.DeleteQueue(TestExecutionConfig.RabbitConfig.WorkflowRequestQueue);
            RabbitClient.CloseConnection();
        }
    }
}
