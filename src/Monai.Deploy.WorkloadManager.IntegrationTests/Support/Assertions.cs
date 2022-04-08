using Monai.Deploy.WorkloadManager.IntegrationTests.Models;
using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
using Monai.Deploy.WorkloadManager.IntegrationTests.TestData;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Support
{
    public class Assertions
    {
        public Assertions(RabbitClientUtil rabbitClientUtil, MongoClientUtil mongoClientUtil)
        {
            RabbitClientUtil = rabbitClientUtil;
            MongoClientUtil = mongoClientUtil;
        }

        private RabbitClientUtil RabbitClientUtil { get; set; }

        private MongoClientUtil MongoClientUtil { get; set; }

        public void AssertExportMessageRequest(string testName)
        {
            string? messagesString = null;
            var counter = 0;
            while (messagesString == null && counter <= 10)
            {
                messagesString = RabbitClientUtil.ReturnMessagesFromQueue(TestExecutionConfig.RabbitConfig.WorkflowRequestQueue);
                if (!string.IsNullOrEmpty(messagesString))
                {
                    var workflowMessage = JsonConvert.DeserializeObject<Workflow>(messagesString);
                    var workflowTestData = TestData.WorkflowRequests.TestData.FirstOrDefault(c => c.TestName.Contains(testName));
                    workflowMessage.Equals(workflowTestData);
                    break;
                }
                counter++;
                Thread.Sleep(1000);
            }

            if (string.IsNullOrEmpty(messagesString))
            {
                throw new Exception($"{TestExecutionConfig.RabbitConfig.WorkflowRequestQueue} returned 0 messages. Please check the logs");
            }
        }

        public void AssertTaskDispatchMessage(string testName)
        {
            string? messagesString = null;
            var counter = 0;
            while (messagesString == null && counter <= 10)
            {
                messagesString = RabbitClientUtil.ReturnMessagesFromQueue(TestExecutionConfig.RabbitConfig.TaskDispatchQueue);
                if (!string.IsNullOrEmpty(messagesString))
                {
                    var workflowMessage = JsonConvert.DeserializeObject<TaskObject>(messagesString);
                    var workflowTestData = TestData.WorkflowRequests.TestData.FirstOrDefault(c => c.TestName.Contains(testName));
                    workflowMessage.Equals(workflowTestData);
                    break;
                }
                counter++;
                Thread.Sleep(1000);
            }

            if (string.IsNullOrEmpty(messagesString))
            {
                throw new Exception($"{TestExecutionConfig.RabbitConfig.WorkflowRequestQueue} returned 0 messages. Please check the logs");
            }
        }

        public void AssertMongoDagDocument(string testName)
        {
            DummyDag document = null;
            var counter = 0;
            var dagTestData = DummyDagTestData.TestData;
            while (document == null && counter <= 10)
            {
                document = MongoClientUtil.GetDummyDagDocument(dagTestData.DummyDag.Id);
                if (document != null)
                {
                    document.Equals(dagTestData.DummyDag);
                    break;
                }
                counter++;
                Thread.Sleep(1000);
            }

            if (document == null)
            {
                throw new Exception($"{dagTestData.DummyDag.Id} returned 0 documents. Please check the logs");
            }
        }
    }
}
