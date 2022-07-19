using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests
{
    public class TaskCallbackTestData
    {
        public string? Name { get; set; }

        public TaskCallbackEvent? TaskCallbackEvent { get; set; }
    }

    public static class TaskCallbacksTestData
    {
        public static List<TaskCallbackTestData> TestData = new List<TaskCallbackTestData>()
        {
            new TaskCallbackTestData()
            {
                Name = "Task_Callback_Basic",
                TaskCallbackEvent = new TaskCallbackEvent()
                {
                    CorrelationId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic").TaskDispatchEvent.CorrelationId,
                    ExecutionId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic").TaskDispatchEvent.ExecutionId,
                    Identity = "Identity_1",
                    TaskId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic").TaskDispatchEvent.TaskId,
                    WorkflowInstanceId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic").TaskDispatchEvent.WorkflowInstanceId,
                    Metadata = new Dictionary<string, object>()
                    {
                        { "metadata_1", "test_1" },
                        { "metadata_2", "test_2" },
                        { "metadata_3", "test_3" },
                    },
                    Outputs = new List<Messaging.Common.Storage> {
                        new Messaging.Common.Storage()
                        {
                            Name = "output",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    }
                }
            }
        };
    }
}
