using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.TestData
{
    public class TaskDispatchTestData
    {
        public string? Name { get; set; }

        public TaskDispatchEvent? TaskDispatchEvent { get; set; }
    }

    public static class TaskDispatchesTestData
    {
        public static List<TaskDispatchTestData> TestData = new List<TaskDispatchTestData>()
        {
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
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
            },
        };
    }
}
