using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
using Monai.Deploy.WorkloadManager.IntegrationTests.Support;
using Monai.Deploy.WorkloadManager.IntegrationTests.TestData;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TestStepDefinitions
    {
        public TestStepDefinitions(RabbitClientUtil rabbitClientUtil, MongoClientUtil mongoClientUtil)
        {
            RabbitClientUtil = rabbitClientUtil;
            MongoClientUtil = mongoClientUtil;
            Assertions = new Assertions(RabbitClientUtil, MongoClientUtil);
        }

        private RabbitClientUtil RabbitClientUtil { get; set; }

        private MongoClientUtil MongoClientUtil { get; set; }

        private Assertions Assertions { get; set; }

        [When(@"I publish an Export Message Request (.*)")]
        public void WhenIPublishAnExportMessageRequest(string testName)
        {
            var workflowTestData = TestData.WorkflowRequests.TestData.FirstOrDefault(c => c.TestName.Contains(testName));
            if (workflowTestData != null)
            {
                var message = JsonConvert.SerializeObject(workflowTestData.ExportMessageRequest);
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
            Assertions.AssertExportMessageRequest(testName);
        }

        [Given(@"I have a DAG in Mongo (.*)")]
        public void IHaveADagInMongo(string testName)
        {
            var dagTestData = DummyDagTestData.TestData;
            if (dagTestData.DummyDag != null)
            {
                MongoClientUtil.CreateDummyDagDocument(dagTestData.DummyDag);
            }
            else
            {
                throw new Exception($"{testName} does not have any applicable test data, please check and try again!");
            }
        }

        [Then(@"I can retrieve the DAG (.*)")]
        public void ThenICanRetrieveTheDAG(string testName)
        {
            Assertions.AssertMongoDagDocument(testName);
        }
    }
}
