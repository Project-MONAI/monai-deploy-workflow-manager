using Monai.Deploy.WorkloadManager.IntegrationTests.Models;
using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Support
{
    public class Assertions
    {
        public Assertions(RabbitClientUtil rabbitClientUtil)
        {
            RabbitClientUtil = rabbitClientUtil;
        }

        private RabbitClientUtil RabbitClientUtil { get; set; }

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
    }
}
