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
using Monai.Deploy.WorkflowManager.Common.Contracts.Constants;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.POCO;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8601 // Possible null reference assignment.

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData
{
    public class WorkflowInstanceTestData
    {
        public string? Name { get; set; }

        public WorkflowInstance? WorkflowInstance { get; set; }
    }

    public static class WorkflowInstancesTestData
    {
        public static WorkflowInstance CreateWorkflowInstance(string workflowName)
        {
            var id = Guid.NewGuid().ToString();
            var payloadId = Guid.NewGuid().ToString();
            var executionId = Guid.NewGuid().ToString();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            return new WorkflowInstance()
            {
                Id = id,
                AeTitle = Helper.GetWorkflowByName(workflowName).WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                WorkflowId = Helper.GetWorkflowByName(workflowName).WorkflowRevision.WorkflowId,
                PayloadId = payloadId,
                StartTime = DateTime.UtcNow,
                Status = Status.Created,
                BucketId = TestExecutionConfig.MinioConfig.Bucket,
                InputMetaData = new Dictionary<string, string>()
                {
                    { "", "" }
                },
                Tasks = new List<TaskExecution>
                {
                    new TaskExecution()
                    {
                        ExecutionId = executionId,
                        WorkflowInstanceId = id,
                        TaskId = Helper.GetWorkflowByName(workflowName).WorkflowRevision.Workflow.Tasks[0].Id,
                        TaskType = Helper.GetWorkflowByName(workflowName).WorkflowRevision?.Workflow.Tasks[0].Type,
                        Status = TaskExecutionStatus.Accepted,
                        InputArtifacts = null,
                        OutputArtifacts = null,
                        OutputDirectory = $"{payloadId}/workflows/{id}/{executionId}"
                    }
                }
            };
        }


        public static List<WorkflowInstanceTestData> TestData = new List<WorkflowInstanceTestData>()
        {
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_instance_for_bucket_minio",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Workflow_Revision_for_bucket_minio")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "5450c3a9-2b19-45b0-8b17-fb10f89d1b2d",
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "pizza",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Accepted,
                            OutputDirectory = "none"
                        }
                    }
                }
            },

            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_For_Case_Sensitivity",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Workflow_Revision_For_Case_Sensitivity")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "C6EBA8CE-9707-4AB5-8D0A-3F88EAB24A80",
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "router",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Accepted,
                            OutputDirectory = "none"
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Existing_WFI_Created",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Created").WorkflowRequestMessage.PayloadId.ToString(),
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "2dbd1af7-b699-4467-8e99-05a0c22422b4",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Created
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Existing_WFI_Dispatched",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Dispatched")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Dispatched").WorkflowRequestMessage.PayloadId.ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "7d7c8b83-6628-413c-9912-a89314e5e2d5",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },

            new WorkflowInstanceTestData()
            {
                Name = "ExportComplete_WFI_Dispatched",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Complete_Request_Workflow_Dispatched")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Helper.GetWorkflowRequestByName("Complete_WF_Dispatched").WorkflowRequestMessage.PayloadId.ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "7d7c8b83-6628-413c-9912-a89314e5e2d5",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = TaskTypeConstants.DicomExportTask,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },

            new WorkflowInstanceTestData()
            {
                Name = "Existing_WFI_Created_Static_PayloadId",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "c2219298-44ec-44d6-b9c7-b2c3e5abaf45",
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "2dbd1af7-b699-4467-8e99-05a0c22422b4",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Created
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Existing_WFI_Dispatched_Static_PayloadId",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Dispatched")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "f31d4065-a3f9-4906-a2a5-d18bcd040274",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "7d7c8b83-6628-413c-9912-a89314e5e2d5",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Status_Update",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Task_Status_Update_Workflow")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Task_Status_Update_Workflow")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    BucketId = "monai",
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Task_Status_Update_Workflow")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Task_Status_Update_Workflow")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Status_Succeeded",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Task_Update_9",
                    WorkflowId = "5A99B6B4-8ADF-45CA-A664-882C85399AEE",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Guid.NewGuid().ToString(),
                            TaskType = "Multi_task_5",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Succeeded
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Status_Failed",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Task_Update_10",
                    WorkflowId = "5A99B6B4-8ADF-45CA-A664-882C85399AEE",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Guid.NewGuid().ToString(),
                            TaskType = "Multi_task_6",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Failed
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Status_Canceled",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Task_Update_11",
                    WorkflowId = "5A99B6B4-8ADF-45CA-A664-882C85399AEE",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Guid.NewGuid().ToString(),
                            TaskType = "Multi_task_7",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Canceled
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Clinical_Review_1",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Clinical_Review_1")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Clinical_Review_1")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "monai",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Clinical_Review_1")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Clinical_Review_1")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Succeeded
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Clinical_Review_1")?.WorkflowRevision?.Workflow?.Tasks[1]?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Clinical_Review_1")?.WorkflowRevision?.Workflow?.Tasks[1]?.Type,
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Multi_Task_1",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "monai",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Multi_Task_2",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_2")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_2")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "monai",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_2")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_2")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Multi_Task_3",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_3")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_3")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "monai",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_3")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id ?? "",
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_3")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type ?? "",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Multi_Task_Dispatched",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "monai",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Dispatched
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks[1].Id ?? "",
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks[1].Type ?? "",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Multi_Task_Accepted",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "monai",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Dispatched
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks[1].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks[1].Type,
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Accepted
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Multi_Task_Succeeded",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "monai",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Dispatched
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks[1].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1")?.WorkflowRevision?.Workflow?.Tasks[1].Type,
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Succeeded
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Multi_Independent_Task",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "monai",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),

                            TaskId = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Invalid_Task_Destination",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "monai",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Static_1",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "bff4cfd0-3af3-4e2b-9f3c-de2a6f2b9569",
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Created").WorkflowRequestMessage.PayloadId.ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "2dbd1af7-b699-4467-8e99-05a0c22422b4",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Created
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Static_2",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "97749d29-8f75-4169-8cf4-1093a1f38c07",
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Dispatched")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Dispatched").WorkflowRequestMessage.PayloadId.ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "7d7c8b83-6628-413c-9912-a89314e5e2d5",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Single_Task_Completed",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault().Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault().Type,
                            Status = TaskExecutionStatus.Dispatched,
                           // OutputArtifacts = "" // Need to add artifacts
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault().Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault().Type,
                            Status = TaskExecutionStatus.Accepted
                           // InputArtifacts = "" // Need to add artifacts
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Destination_Metadata_Condition_True",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Metadata_Condition_True")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Metadata_Condition_True")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "c5c3636b-81dd-44a9-8c4b-71adec7d47b2",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Metadata_Condition_True")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Metadata_Condition_True")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Destination_Metadata_Null_Condition_True",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Metadata_Condition_True")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Metadata_Condition_True")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "c5c3636b-81dd-44a9-8c4b-71adec7d47b2",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Metadata_Condition_True")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Metadata_Condition_True")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Destination_Condition_True",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Condition_True")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Condition_True")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Condition_True")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Condition_True")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Destination_Multi_Condition_True",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Multi_Condition_True")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Multi_Condition_True")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Multi_Condition_True")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Multi_Condition_True")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Destination_Multi_Condition_False",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Multi_Condition_False")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Multi_Condition_False")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Multi_Condition_False")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Multi_Condition_False")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Destination_Metadata_Condition_False",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Metadata_Condition_False")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Metadata_Condition_False")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "c5c3636b-81dd-44a9-8c4b-71adec7d47b2",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Metadata_Condition_False")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Metadata_Condition_False")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Destination_Condition_False",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Condition_False")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Condition_False")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Condition_False")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Condition_False")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Multiple_Destination_Condition_True",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Multiple_Destination_Single_Condition_True")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Multiple_Destination_Single_Condition_True")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Multiple_Destination_Single_Condition_True")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Multiple_Destination_Single_Condition_True")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Multiple_Destination_Condition_False",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Multiple_Destination_Single_Condition_False")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Multiple_Destination_Single_Condition_False")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Multiple_Destination_Single_Condition_False")?.WorkflowRevision?.Workflow?.Tasks?.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Multiple_Destination_Single_Condition_False")?.WorkflowRevision?.Workflow?.Tasks?.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Single_Task_Completed",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Dispatched,
                           // OutputArtifacts = "" // Need to add artifacts
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Accepted
                           // InputArtifacts = "" // Need to add artifacts
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Destination_Condition_True_And_False",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Multiple_Condition_True_And_False")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Multiple_Condition_True_And_False")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Multiple_Condition_True_And_False")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Multiple_Condition_True_And_False")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WFI_Task_Destination_Invalid_Condition",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Task_Destination_Invalid_Condition")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Task_Destination_Invalid_Condition")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = "bucket_1",
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Task_Destination_Invalid_Condition")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Task_Destination_Invalid_Condition")?.WorkflowRevision?.Workflow?.Tasks.FirstOrDefault()?.Type,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "TwoTask_Context.Dicom.Input_ArtifactMandatory=Null",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=Null").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=Null").WorkflowRevision.WorkflowId,
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=Null").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=Null").WorkflowRevision?.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Accepted,
                            InputArtifacts = null,
                            OutputArtifacts = null
                        },
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "TwoTask_Context.Dicom.Input_ArtifactMandatory=True",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=True").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=True").WorkflowRevision.WorkflowId,
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=True").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=True").WorkflowRevision?.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Accepted,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "ATwoTask_Context.Dicom.Input_ArtifactMandatory=False",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=False").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=False").WorkflowRevision.WorkflowId,
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=False").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("TwoTask_Context.Dicom.Input_ArtifactMandatory=False").WorkflowRevision?.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Accepted,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=True",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=True").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=True").WorkflowRevision.WorkflowId,
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=True").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=True").WorkflowRevision?.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Accepted,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False").WorkflowRevision.WorkflowId,
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False").WorkflowRevision?.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Accepted,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=Null",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=Null").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=Null").WorkflowRevision.WorkflowId,
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=Null").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=Null").WorkflowRevision?.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Accepted,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_for_export_single_dest_1",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Workflow_Revision_for_export_single_dest_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Workflow_Revision_for_export_single_dest_1").WorkflowRevision.WorkflowId,
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Workflow_Revision_for_export_single_dest_1").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Workflow_Revision_for_export_single_dest_1").WorkflowRevision?.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Accepted,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_for_export_folder",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "99333d69-806f-420a-932f-2cc23501f018",
                    AeTitle = Helper.GetWorkflowByName("Workflow_Revision_for_export_folder").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Workflow_Revision_for_export_folder").WorkflowRevision.WorkflowId,
                    PayloadId = "fd1e99c1-341a-4400-aa28-3fa89d874968",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "f8b6118e-bf4f-4a12-8a03-71f4a7469154",
                            TaskId = Helper.GetWorkflowByName("Workflow_Revision_for_export_folder").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Workflow_Revision_for_export_folder").WorkflowRevision?.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Accepted,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                            OutputDirectory = "fd1e99c1-341a-4400-aa28-3fa89d874968/workflows/99333d69-806f-420a-932f-2cc23501f018/f8b6118e-bf4f-4a12-8a03-71f4a7469154",
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_for_export_multi_dest_1",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_1").WorkflowRevision.WorkflowId,
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_1").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_1").WorkflowRevision?.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Accepted,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_for_export_multi_dest_2",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_2").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_2").WorkflowRevision.WorkflowId,
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_2").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_2").WorkflowRevision?.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Succeeded,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_2").WorkflowRevision.Workflow.Tasks[1].Id,
                            TaskType = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_2").WorkflowRevision?.Workflow.Tasks[1].Type,
                            Status = TaskExecutionStatus.Dispatched,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=True",
                WorkflowInstance = CreateWorkflowInstance("TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=True")
            },
            new WorkflowInstanceTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=False",
                WorkflowInstance = CreateWorkflowInstance("TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=False")
            },
            new WorkflowInstanceTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=Null",
                WorkflowInstance = CreateWorkflowInstance("TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=Null")
            },
            new WorkflowInstanceTestData()
            {
                Name = "Mandatory_Output",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "73dcde74-8f9c-47d2-9b91-b62c9ad2235c",
                    AeTitle = Helper.GetWorkflowByName("Mandatory_Output").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Mandatory_Output").WorkflowRevision.WorkflowId,
                    PayloadId = "85dce342-e5b0-4a40-9725-9a34f8e1fda0",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "77250b38-c262-4367-97b8-0094439cca94",
                            TaskId = Helper.GetWorkflowByName("Mandatory_Output").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Mandatory_Output").WorkflowRevision?.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Accepted,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                            OutputDirectory = "85dce342-e5b0-4a40-9725-9a34f8e1fda0/workflows/73dcde74-8f9c-47d2-9b91-b62c9ad2235c/77250b38-c262-4367-97b8-0094439cca94"
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WorkflowInstance_TaskApi_1",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "44a63094-9e36-4ba4-9fea-8e9b76aa875b",
                    AeTitle = "Ae_test",
                    WorkflowId = "a971f5f8-68fa-4cd0-ad34-f20b66675d21",
                    PayloadId = "e908ff53-d808-4c9b-82b6-698b8c60e811",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "8ff3ea90-0113-4071-9b92-5068956daeff",
                            TaskId = "7b8ea05b-8abe-4848-928d-d55f5eef1bc3",
                            TaskType = "router",
                            Status = TaskExecutionStatus.Accepted,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "a1cd5b89-85e8-4d32-b9aa-bdbc0f4bbba5",
                            TaskId = "953c0236-5292-4186-80ee-ef7d4073220b",
                            TaskType = "export",
                            Status = TaskExecutionStatus.Succeeded,
                            InputArtifacts = new Dictionary<string, string>()
                            {
                                {"key_1", "value_1" }
                            },
                            OutputArtifacts = new Dictionary<string, string>()
                            {
                                {"key_1", "value_1" }
                            },
                            OutputDirectory = "payload_id/dcm",
                            Reason = FailureReason.None,
                            PreviousTaskId = "PreviousTask",
                            ExecutionStats = new Dictionary<string, string>()
                            {
                                {"key_1", "value_1" }
                            },
                            ResultMetadata = new Dictionary<string, object>()
                            {
                                {"key_1", "value_1" },
                                {"key_2", 1 }
                            },
                            TaskPluginArguments = new Dictionary<string, string>()
                            {
                                {"key_1", "value_1" }
                            },
                            TaskStartTime = DateTime.UtcNow
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "d1e0a3b7-3026-4cf7-ba04-71c1f50d98f6",
                            TaskId = "b3b537ae-79e8-4d13-9154-982ee4743595",
                            TaskType = "argo",
                            Status = TaskExecutionStatus.Created,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "666faff9-c702-48a9-ae37-5d92e1f6b324",
                            TaskId = "aad3762a-5c49-499b-a368-e5f9b98408e4",
                            TaskType = "export",
                            Status = TaskExecutionStatus.Dispatched,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "3b30b992-a87b-4885-9176-824b751f076e",
                            TaskId = "1ecdcc90-a999-48fa-a507-99d4ea7c9cb0",
                            TaskType = "argo",
                            Status = TaskExecutionStatus.Canceled,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "d9b54e70-d016-476d-b9c3-86ffb17ef786",
                            TaskId = "a08920eb-0895-4272-ad81-54f2046d8438",
                            TaskType = "argo",
                            Status = TaskExecutionStatus.Succeeded,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "0c01f526-e574-40df-b21b-99cd16f5c305",
                            TaskId = "93d8e1c5-39b1-43f0-9bac-372c438b91c0",
                            TaskType = "argo",
                            Status = TaskExecutionStatus.Failed,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WorkflowInstance_TaskApi_2",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "44a63094-9e36-4ba4-9fea-8e9b76aa875b2",
                    AeTitle = "Ae_test",
                    WorkflowId = "a971f5f8-68fa-4cd0-ad34-f20b66675d214",
                    PayloadId = "e908ff53-d808-4c9b-82b6-698b8c60e8111",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "8ff3ea90-0113-4071-9b92-5068f956daeff",
                            TaskId = "7b8ea05b-8abe-4848-928d-d55f5eef1bc3",
                            TaskType = "router",
                            Status = TaskExecutionStatus.Accepted,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "a1cd5b89-85e8-4d32-b9aa-bdbc0ff4bbba5",
                            TaskId = "953c0236-5292-4186-80ee-ef7d4073220b",
                            TaskType = "export",
                            Status = TaskExecutionStatus.Succeeded,
                            InputArtifacts = new Dictionary<string, string>()
                            {
                                {"key_1", "value_1" }
                            },
                            OutputArtifacts = new Dictionary<string, string>()
                            {
                                {"key_1", "value_1" }
                            },
                            OutputDirectory = "payload_id/dcm",
                            Reason = FailureReason.None,
                            PreviousTaskId = "PreviousTask",
                            ExecutionStats = new Dictionary<string, string>()
                            {
                                {"key_1", "value_1" }
                            },
                            ResultMetadata = new Dictionary<string, object>()
                            {
                                {"key_1", "value_1" },
                                {"key_2", 1 }
                            },
                            TaskPluginArguments = new Dictionary<string, string>()
                            {
                                {"key_1", "value_1" }
                            },
                            TaskStartTime = DateTime.UtcNow
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "d1e0a3b7-3026-42cf7-ba04-71c1f50d98f6",
                            TaskId = "b3b537ae-79e8-4d13-9154-982ee4743595",
                            TaskType = "argo",
                            Status = TaskExecutionStatus.Created,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "666faff9-c702-481a9-ae37-5d92e1f6b324",
                            TaskId = "aad3762a-5c49-499b-a368-e5f9b98408e4",
                            TaskType = "export",
                            Status = TaskExecutionStatus.Dispatched,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "3b30b992-a87b-48865-9176-824b751f076e",
                            TaskId = "1ecdcc90-a999-48fa-a507-99d4ea7c9cb0",
                            TaskType = "argo",
                            Status = TaskExecutionStatus.Canceled,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "d9b54e70-d016-476d-b9c3-86ff8b17ef786",
                            TaskId = "a08920eb-0895-4272-ad81-54f2046d8438",
                            TaskType = "argo",
                            Status = TaskExecutionStatus.Succeeded,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "0c01f526-e574-40df-b21b-99cd16f5c305",
                            TaskId = "93d8e1c5-39b1-43f0-9bac-372c438b91c0",
                            TaskType = "argo",
                            Status = TaskExecutionStatus.Failed,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "WorkflowInstance_TaskApi_3",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "44a63094-9e36-4ba4-9fea-8e9b76aa875b2",
                    AeTitle = "Ae_test",
                    WorkflowId = "a971f5f8-68fa-4cd0-ad34-f20b66675d214",
                    PayloadId = "e908ff53-d808-4c9b-82b6-698b8c60e8111",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    BucketId = TestExecutionConfig.MinioConfig.Bucket,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "8ff3ea90-0113-4071-9b92-5068f956daeff",
                            TaskId = "7b8ea05b-8abe-4848-928d-d55f5eef1bc3",
                            TaskType = "router",
                            Status = TaskExecutionStatus.Succeeded,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "a1cd5b89-85e8-4d32-b9aa-bdbc0ff4bbba5",
                            TaskId = "953c0236-5292-4186-80ee-ef7d4073220b",
                            TaskType = "export",
                            Status = TaskExecutionStatus.Succeeded,
                            InputArtifacts = new Dictionary<string, string>()
                            {
                                {"key_1", "value_1" }
                            },
                            OutputArtifacts = new Dictionary<string, string>()
                            {
                                {"key_1", "value_1" }
                            },
                            OutputDirectory = "payload_id/dcm",
                            Reason = FailureReason.None,
                            PreviousTaskId = "PreviousTask",
                            ExecutionStats = new Dictionary<string, string>()
                            {
                                {"key_1", "value_1" }
                            },
                            ResultMetadata = new Dictionary<string, object>()
                            {
                                {"key_1", "value_1" },
                                {"key_2", 1 }
                            },
                            TaskPluginArguments = new Dictionary<string, string>()
                            {
                                {"key_1", "value_1" }
                            },
                            TaskStartTime = DateTime.UtcNow
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "d1e0a3b7-3026-42cf7-ba04-71c1f50d98f6",
                            TaskId = "b3b537ae-79e8-4d13-9154-982ee4743595",
                            TaskType = "argo",
                            Status = TaskExecutionStatus.Succeeded,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "666faff9-c702-481a9-ae37-5d92e1f6b324",
                            TaskId = "aad3762a-5c49-499b-a368-e5f9b98408e4",
                            TaskType = "export",
                            Status = TaskExecutionStatus.Succeeded,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "3b30b992-a87b-48865-9176-824b751f076e",
                            TaskId = "1ecdcc90-a999-48fa-a507-99d4ea7c9cb0",
                            TaskType = "argo",
                            Status = TaskExecutionStatus.Canceled,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "d9b54e70-d016-476d-b9c3-86ff8b17ef786",
                            TaskId = "a08920eb-0895-4272-ad81-54f2046d8438",
                            TaskType = "argo",
                            Status = TaskExecutionStatus.Failed,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "0c01f526-e574-40df-b21b-99cd16f5c305",
                            TaskId = "93d8e1c5-39b1-43f0-9bac-372c438b91c0",
                            TaskType = "argo",
                            Status = TaskExecutionStatus.Failed,
                            InputArtifacts = null,
                            OutputArtifacts = null,
                        },
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Acknowledge_Failed_1_Task",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "25dff711-efc5-4eeb-bccc-2bb996400a20",
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Created").WorkflowRequestMessage.PayloadId.ToString(),
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Failed,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Failed,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Acknowledge_PartialFailed_1_Task",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "25dff711-efc5-4eeb-bccc-2bb996400a20",
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Created").WorkflowRequestMessage.PayloadId.ToString(),
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Succeeded,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.PartialFail,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Acknowledge_Already_Failed_1_Task",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "25dff711-efc5-4eeb-bccc-2bb996400a20",
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Created").WorkflowRequestMessage.PayloadId.ToString(),
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Failed,
                    AcknowledgedWorkflowErrors = new DateTime(2000, 01, 01, 12, 00, 00, DateTimeKind.Utc),
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Failed,
                            AcknowledgedTaskErrors = new DateTime(2000, 01, 01, 12, 00, 00, DateTimeKind.Utc),

    }
}
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Acknowledge_Failed_2_Task",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "25dff711-efc5-4eeb-bccc-2bb996400a20",
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Created").WorkflowRequestMessage.PayloadId.ToString(),
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Failed,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Failed,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Failed,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Acknowledge_Not_Failed_1_Task",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "25dff711-efc5-4eeb-bccc-2bb996400a20",
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Created").WorkflowRequestMessage.PayloadId.ToString(),
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Failed,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Accepted,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Acknowledge_Not_Failed_Instance",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "25dff711-efc5-4eeb-bccc-2bb996400a20",
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Created").WorkflowRequestMessage.PayloadId.ToString(),
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Failed,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_For_Failed_Partial",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Guid.NewGuid().ToString(),
                    PayloadId = "C6EBA8CE-9707-4AB5-8D0A-3F88EAB24A80",
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "router",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Accepted,
                            OutputDirectory = "none"
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_For_PayloadData_Payload_PayloadStatus_1_1",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "a5c3633b-31dd-44c9-8a1a-71adec3d47c1",
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Failed,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_For_PayloadData_Payload_PayloadStatus_1_2",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "a5c3633b-31dd-44c9-8a1a-71adec3d47c1",
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Failed,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_For_PayloadData_Payload_PayloadStatus_2_1",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "a5c3633b-31dd-44c9-8a1a-71adec3d47c2",
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Succeeded,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Failed,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_For_PayloadData_Payload_PayloadStatus_2_2",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "a5c3633b-31dd-44c9-8a1a-71adec3d47c2",
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Failed,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Failed,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_For_PayloadData_Payload_PayloadStatus_3_1",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "a5c3633b-31dd-44c9-8a1a-71adec3d47c3",
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Failed,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_For_PayloadData_Payload_PayloadStatus_3_2",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "a5c3633b-31dd-44c9-8a1a-71adec3d47c3",
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Succeeded,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Failed,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_For_PayloadData_Payload_PayloadStatus_3_3",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "a5c3633b-31dd-44c9-8a1a-71adec3d47c3",
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Failed,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "", "" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "First_Task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Failed,
                        }
                    }
                }
            },
            new WorkflowInstanceTestData()
            {
                Name = "Workflow_Instance_For_Artifact_ReceivedEvent_1",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Workflow_Revision_For_Artifact_ReceivedEvent_1")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = "c4c3633b-c1dd-c4c9-8a1a-71adec3d47c3",
                    BucketId = "bucket1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "root_task",
                            OutputDirectory = "payloadId/workflows/workflowInstanceId/executionId/",
                            TaskType = "router_task",
                            Status = TaskExecutionStatus.Succeeded,
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            WorkflowInstanceId = "d32d5769-4ecf-4639-a048-6ecf2cced04a",
                            TaskId = "e545de90-c936-40ab-ad11-19ef07f49607",
                            Status = TaskExecutionStatus.Dispatched,
                            TaskType = "remote_task",
                            OutputArtifacts = new Dictionary<string, string>()
                            {
                                { "key1", "value1" }
                            },
                        },
                    }
                }
            }
        };
    }
}
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
