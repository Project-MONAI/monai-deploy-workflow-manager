/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Monai.Deploy.Messaging.Events;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData
{
    public class TaskUpdateTestData
    {
        public string? Name { get; set; }

        public TaskUpdateEvent? TaskUpdateEvent { get; set; }
    }

    public static class TaskUpdatesTestData
    {
        public static string GetRelativePathForOutputArtifacts(string name)
        {
            var workflowInstance = Helper.GetWorkflowInstanceByName(name)?.WorkflowInstance;

            var executionId = workflowInstance?.Tasks.FirstOrDefault(x => x.Status.Equals(TaskExecutionStatus.Accepted))?.ExecutionId;

            return $"{workflowInstance?.PayloadId}/workflows/{workflowInstance?.Id}/{executionId}";
        }

        public static TaskUpdateEvent CreateTaskUpdateEvent(string workflowInstanceName)
        {
            return new TaskUpdateEvent()
            {
                WorkflowInstanceId = Helper.GetWorkflowInstanceByName(workflowInstanceName).WorkflowInstance.Id,
                ExecutionId = Helper.GetWorkflowInstanceByName(workflowInstanceName).WorkflowInstance.Tasks[0].ExecutionId,
                CorrelationId = Guid.NewGuid().ToString(),
                Reason = FailureReason.None,
                Message = "Task Message",
                TaskId = Helper.GetWorkflowInstanceByName(workflowInstanceName).WorkflowInstance.Tasks[0].TaskId,
                Outputs = new List<Messaging.Common.Storage>
                    {
                        new Messaging.Common.Storage()
                        {
                            Name = "output",
                            Endpoint = "//output.dcm",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = GetRelativePathForOutputArtifacts(workflowInstanceName)
                        }
                    },
                Metadata = new Dictionary<string, object>()
                {
                }
            };
        }

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
                Name = "Task_Status_Update_Invalid_Message",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.InvalidMessage,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Runner_Not_Supported",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.RunnerNotSupported,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Meta_String",
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
                        { "metadata_1", "test_1" },
                        { "metadata_2", "test_2" },
                        { "metadata_3", "test_3" },
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Meta_Bool",
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
                        { "metadata_1", true },
                        { "metadata_2", false },
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Status_Update_Meta_Int",
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
                        { "metadata_1", 1 },
                        { "metadata_2", 2 },
                        { "metadata_3", 3 },
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
                Name = "Task_Status_Update_Missing_ExecutionId",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Id,
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
                Name = "Task_Status_Update_Missing_CorrelationId",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Status_Update").WorkflowInstance.Tasks[0].ExecutionId,
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
                Name = "Task_Update_Dispatches_Clinical_Review_False",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Clinical_Review_1").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Clinical_Review_1").WorkflowInstance.Tasks[1].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Clinical_Review_1").WorkflowInstance.Tasks[1].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                         { "acceptance", false },
                        { "reason", "not correct" },
                        { "user_id", "example_user_id" }
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Dispatches_Clinical_Review_True",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Clinical_Review_1").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Clinical_Review_1").WorkflowInstance.Tasks[1].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Clinical_Review_1").WorkflowInstance.Tasks[1].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                         { "acceptance", true },
                        { "user_id", "example_user_id" }
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
                Name = "Task_Update_Task_Destination_Multi_Condition_True",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Multi_Condition_True").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Multi_Condition_True").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Multi_Condition_True").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Task_Destination_Multi_Condition_False",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Multi_Condition_False").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Multi_Condition_False").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Multi_Condition_False").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Task_Destination_Metadata_Condition_True",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Metadata_Condition_True").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Metadata_Condition_True").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Metadata_Condition_True").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_Update_Task_Destination_Metadata_Null_Condition_True",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Metadata_Condition_True").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Metadata_Condition_True").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Metadata_Condition_True").WorkflowInstance.Tasks[0].TaskId,
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
                Name = "Task_Update_Task_Destination_Metadata_Condition_False",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Metadata_Condition_False").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Metadata_Condition_False").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("WFI_Task_Destination_Metadata_Condition_False").WorkflowInstance.Tasks[0].TaskId,
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
                Name = "Task_Update_Task_Multiple_Destination_Condition_Case_Sensitivity",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Workflow_Instance_For_Case_Sensitivity").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("Workflow_Instance_For_Case_Sensitivity").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("Workflow_Instance_For_Case_Sensitivity").WorkflowInstance.Tasks[0].TaskId,
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
                Name = "TwoTask_Context.Dicom.Input_ArtifactMandatory=Null",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=Null").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=Null").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=Null").WorkflowInstance.Tasks[0].TaskId,
                    Outputs = new List<Messaging.Common.Storage>
                    {
                        new Messaging.Common.Storage()
                        {
                            Name = "output.dcm",
                            Endpoint = "//output.dcm",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = GetRelativePathForOutputArtifacts("TwoTask_Context.Dicom.Input_ArtifactMandatory=Null")
                        }
                    },
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "TwoTask_Context.Dicom.Input_ArtifactMandatory=True",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=True").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=True").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=True").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "TwoTask_Context.Dicom.Input_ArtifactMandatory=False",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=False").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=False").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=False").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=True",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=True").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=True").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=True").WorkflowInstance.Tasks[0].TaskId,
                    Outputs = new List<Messaging.Common.Storage>
                    {
                        new Messaging.Common.Storage()
                        {
                            Name = "output",
                            Endpoint = "//output.dcm",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = GetRelativePathForOutputArtifacts("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=True")
                        }
                    },
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False").WorkflowInstance.Tasks[0].TaskId,
                    Outputs = new List<Messaging.Common.Storage>
                    {
                        new Messaging.Common.Storage()
                        {
                            Name = "output",
                            Endpoint = "//output.dcm",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = GetRelativePathForOutputArtifacts("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False")
                        }
                    },
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False_No_Outputs",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=Null",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=Null").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=Null").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=Null").WorkflowInstance.Tasks[0].TaskId,
                    Outputs = new List<Messaging.Common.Storage>
                    {
                        new Messaging.Common.Storage()
                        {
                            Name = "output",
                            Endpoint = "//output.dcm",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = GetRelativePathForOutputArtifacts("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=Null")
                        }
                    },
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=True",
                TaskUpdateEvent = CreateTaskUpdateEvent("TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=True")
            },
            new TaskUpdateTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=False",
                TaskUpdateEvent = CreateTaskUpdateEvent("TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=False")
            },
            new TaskUpdateTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=Null",
                TaskUpdateEvent = CreateTaskUpdateEvent("TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=Null")
            },
            new TaskUpdateTestData()
            {
                Name = "Mandatory_Output",
                TaskUpdateEvent = CreateTaskUpdateEvent("Mandatory_Output")
            },
            new TaskUpdateTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=False_No_Outputs",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=False").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=False").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=False").WorkflowInstance.Tasks[0].TaskId,
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_status_update_for_export_single_dest_1",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_single_dest_1").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_single_dest_1").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_single_dest_1").WorkflowInstance.Tasks[0].TaskId,
                    Outputs = new List<Messaging.Common.Storage>
                    {
                        new Messaging.Common.Storage()
                        {
                            Name = "output",
                            Endpoint = "//output.dcm",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = GetRelativePathForOutputArtifacts("Workflow_Instance_for_export_single_dest_1")
                        }
                    },
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_status_update_for_export_folder",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_folder").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_folder").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_folder").WorkflowInstance.Tasks[0].TaskId,
                    Outputs = new List<Messaging.Common.Storage>
                    {
                        new Messaging.Common.Storage()
                        {
                            Name = "output",
                            Endpoint = "//output.dcm",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = GetRelativePathForOutputArtifacts("Workflow_Instance_for_export_folder")
                        }
                    },
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
            new TaskUpdateTestData()
            {
                Name = "Task_status_update_for_export_multi_dest_1",
                TaskUpdateEvent = new TaskUpdateEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_multi_dest_1").WorkflowInstance.Id,
                    ExecutionId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_multi_dest_1").WorkflowInstance.Tasks[0].ExecutionId,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Reason = FailureReason.None,
                    Message = "Task Message",
                    TaskId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_multi_dest_1").WorkflowInstance.Tasks[0].TaskId,
                    Outputs = new List<Messaging.Common.Storage>
                    {
                        new Messaging.Common.Storage()
                        {
                            Name = "output",
                            Endpoint = "//output.dcm",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = GetRelativePathForOutputArtifacts("Workflow_Instance_for_export_multi_dest_1")
                        }
                    },
                    Metadata = new Dictionary<string, object>()
                    {
                    }
                }
            },
        };
    }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
