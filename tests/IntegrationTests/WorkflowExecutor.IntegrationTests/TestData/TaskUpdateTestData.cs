using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.WorkflowExecutor.IntegrationTests.TestData
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
                Name = "Task_status_update_for_bucket_minio",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Workflow_instance_for_bucket_minio").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("Workflow_instance_for_bucket_minio").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Succeeded,
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("Workflow_instance_for_bucket_minio").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
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
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Task_Destination_Condition_True",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Condition_True").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Condition_True").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Condition_True").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Task_Destination_Condition_False",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Condition_False").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Condition_False").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Condition_False").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Task_Multiple_Destination_Condition_True",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Multiple_Destination_Condition_True").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Multiple_Destination_Condition_True").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Multiple_Destination_Condition_True").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Task_Multiple_Destination_Condition_False",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Multiple_Destination_Condition_False").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Multiple_Destination_Condition_False").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Multiple_Destination_Condition_False").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Task_Destination_Condition_True_And_False",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Condition_True_And_False").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Condition_True_And_False").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Condition_True_And_False").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Task_Destination_Invalid_Condition",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Invalid_Condition").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Invalid_Condition").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Invalid_Condition").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Artifact_Mandatory_Double_Null",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Artifact_WFI_Mandatory_Double_Null").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("Artifact_WFI_Mandatory_Double_Null").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("Artifact_WFI_Mandatory_Double_Null").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Artifact_Mandatory_Double_True",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Artifact_WFI_Mandatory_Double_True").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("Artifact_WFI_Mandatory_Double_True").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("Artifact_WFI_Mandatory_Double_True").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Artifact_Mandatory_Double_False",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Artifact_WFI_Mandatory_Double_False").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("Artifact_WFI_Mandatory_Double_False").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("Artifact_WFI_Mandatory_Double_False").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Artifact_Mandatory_Double_Null_TASK_ID",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Artifact_WFI_Mandatory_Double_Null_TASK_ID").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("Artifact_WFI_Mandatory_Double_Null_TASK_ID").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("Artifact_WFI_Mandatory_Double_Null_TASK_ID").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    },
                    Outputs = new List<Messaging.Common.Storage>
                    {
                        new Messaging.Common.Storage()
                        {
                            Name = "input1",
                            Endpoint = "//test_1",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test1",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "02a865ea-064c-43f5-a3c7-35cd23fa89af/workflows/87530a73-4f7f-4d4f-8498-8ce97e4c89c6/8f1b1fde-9c9e-4007-b424-329532920dae/07051db3-3c1d-4bf2-8764-ba45dc918e74.dcm"
                        }
                    }
                }
            },
        };
    }
}
