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
                Name = "Task_Status_Update_Valid_1",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_1").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_1").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_1").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Valid_2",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_2").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_2").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_2").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Valid_3",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_3").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_3").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_3").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Valid_4",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_4").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_4").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_4").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_WorkflowInstanceId_Not_Found",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = "303c441f-7181-43cf-b1fd-83e5acec99fa",
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_2").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_2").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_TaskId_Not_Found",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_4").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_4").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = "303c441f-7181-43cf-b1fd-83e5acec99fa",
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Missing_WorkflowId",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_1").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_1").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Missing_TaskId",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_2").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_2").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Missing_ExecutionId",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_3").WorkflowInstance.Id,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_3").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Missing_CorrelationId",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_4").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_4").WorkflowInstance.Tasks[0].ExecutionId,
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_4").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Missing_Status",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_1").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_1").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update_1").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Status_Invalid_When_Succeeded",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Succeeded").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Succeeded").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Succeeded").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Status_Invalid_When_Failed",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Failed").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Failed").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Failed").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Status_Invalid_When_Canceled",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Canceled").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Canceled").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Canceled").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
        };
    }
}
