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
            RabbitClientUtil.CreateQueue(TestExecutionConfig.RabbitConfig.WorkflowRequestQueue);
        }

        [When(@"I publish an event (.*)")]
        public void WhenIPublishAnEvent(string testName)
        {
            var workflowTestData = TestData.Workflows.WorkflowTestData.FirstOrDefault(c => c.TestName.Contains(testName));
            if (workflowTestData != null)
            {
                var message = JsonConvert.SerializeObject(workflowTestData.Workflow);
                RabbitClientUtil.PublishMessage(message, TestExecutionConfig.RabbitConfig.WorkflowRequestQueue);
            }
            else
            {
                throw new Exception($"{testName} does not have any applicable test data, please check and try again!");
            }
        }

        [Then(@"I can see the event (.*)")]
        public void ThenICanSeeTheEvent(string testName)
        {
            var messagesString = RabbitClientUtil.ReturnMessagesFromQueue(TestExecutionConfig.RabbitConfig.WorkflowRequestQueue);
            var workflowMessage = JsonConvert.DeserializeObject<Workflow>(messagesString);
            var workflowTestData = TestData.Workflows.WorkflowTestData.FirstOrDefault(c => c.TestName.Contains(testName));
            workflowMessage.Description.Should().Be(workflowTestData.Workflow.Description);
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
