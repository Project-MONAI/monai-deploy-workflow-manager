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
                Name = "Task_Status_Update",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].TaskId,
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
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].ExecutionId,
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
                Name = "Task_Status_Update_Missing_TaskId",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].ExecutionId,
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
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Id,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].TaskId,
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
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].ExecutionId,
                    Status = TaskExecutionStatus.Unknown,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].TaskId,
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
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].TaskId,
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
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Succeeded").WorkflowInstance.Id,
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
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Failed").WorkflowInstance.Id,
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
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Canceled").WorkflowInstance.Id,
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
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Dispatches_Single_Task",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Multi_Task_1").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Multi_Task_1").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Multi_Task_1").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Dispatches_Multi_Tasks",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Multi_Task_2").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Multi_Task_2").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Multi_Task_2").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_To_Dispatch_Single_Task",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Multi_Task_3").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Multi_Task_3").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Multi_Task_3").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Independent_Task",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Multi_Independent_Task").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Multi_Independent_Task").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Multi_Independent_Task").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Independent_Task",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Multi_Independent_Task").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Multi_Independent_Task").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Multi_Independent_Task").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Invalid_Task_Destination",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Invalid_Task_Destination").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Invalid_Task_Destination").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Invalid_Task_Destination").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "output_artefact_file",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Invalid_Task_Destination").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Invalid_Task_Destination").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Invalid_Task_Destination").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "output_artefact_dir",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Invalid_Task_Destination").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Invalid_Task_Destination").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Invalid_Task_Destination").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
        };
    }
}
