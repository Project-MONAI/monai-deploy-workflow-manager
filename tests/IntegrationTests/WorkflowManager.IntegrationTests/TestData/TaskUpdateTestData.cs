using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.TestData
{
    public class TaskUpdateTestData
    {
        public string? Name { get; set; }

        public TaskUpdateEvent? TaskUpdateEvent { get; set; }
    }

    public static class TaskUpdatesTestData
    {
        public static List<TaskUpdateTestData> TestData = new List<TaskUpdateTestData>()
        {
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Valid",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("").WorkflowInstance.WorkflowId,
                    ExecutionId = Helper.GetWorkflowInstanceByName("").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = "e703b4f7-ac9b-45a1-b016-36694ca6b4b6",
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    Metadata = new Dictionary<string, object>()
                    {
                        { "", null }
                    }
                }
            },
        };
    }
}
