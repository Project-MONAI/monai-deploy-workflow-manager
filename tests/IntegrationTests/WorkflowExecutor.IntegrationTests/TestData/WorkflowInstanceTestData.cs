/*
 * Copyright 2021-2022 MONAI Consortium
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
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8601 // Possible null reference assignment.

namespace Monai.Deploy.WorkflowManager.WorkflowExecutor.IntegrationTests.TestData
{
    public class WorkflowInstanceTestData
    {
        public string? Name { get; set; }

        public WorkflowInstance? WorkflowInstance { get; set; }
    }

    public static class WorkflowInstancesTestData
    {
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
                    WorkflowId = Guid.NewGuid().ToString(),
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
                    WorkflowId = Guid.NewGuid().ToString(),
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
                    WorkflowId = Guid.NewGuid().ToString(),
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
                    StartTime = DateTime.Now,
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
                Name = "WFI_Task_Destination_Condition_True",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Condition_True")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Condition_True")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.Now,
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
                Name = "WFI_Task_Destination_Condition_False",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Condition_False")?.WorkflowRevision?.Workflow?.InformaticsGateway?.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Destination_Single_Condition_False")?.WorkflowRevision?.WorkflowId ?? "",
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
        };
    }
}
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
