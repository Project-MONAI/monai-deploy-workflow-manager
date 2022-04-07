using FluentAssertions;
using Monai.Deploy.WorkloadManager.IntegrationTests.Models;
using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
using Monai.Deploy.WorkloadManager.IntegrationTests.Support;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TestStepDefinitions
    {
        public TestStepDefinitions(RabbitClientUtil rabbitClientUtil)
        {
            RabbitClientUtil = rabbitClientUtil;
            // MongoClientUtil = mongoClientUtil;
        }

        private RabbitClientUtil RabbitClientUtil { get; set; }

        //private MongoClientUtil MongoClientUtil { get; set; }

        [Given(@"I have a Rabbit connection")]
        public void GivenIHaveARabbitConnection()
        {
            RabbitClientUtil.CreateQueue(TestExecutionConfig.RabbitConfig.PublisherQueue);
        }

        [When(@"I publish an event")]
        public void WhenIPublishAnEvent()
        {
            var message = JsonConvert.SerializeObject(TestData.Workflows.WorkflowTestData[0]); // change to lambda expression
            RabbitClientUtil.PublishMessage(message, TestExecutionConfig.RabbitConfig.PublisherQueue);
        }

        [Then(@"I can see the event")]
        public void ThenICanSeeTheEvent()
        {
            var messagesString = RabbitClientUtil.ReturnMessagesFromQueue(TestExecutionConfig.RabbitConfig.PublisherQueue);
            var workflowMessage = JsonConvert.DeserializeObject<Workflow>(messagesString); //change to lambda expression
            workflowMessage.Description.Should().Be("Test_Description");
        }

        [Given(@"I have a Mongo connection")]
        public void GivenIHaveAMongoConnection()
        {
        }

        [When(@"I save a DAG")]
        public void WhenISaveADAG()
        {
        }

        [Then(@"I can retrieve the DAG")]
        public void ThenICanRetrieveTheDAG()
        {
        }
    }
}
