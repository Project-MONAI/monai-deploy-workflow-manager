﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.TestData
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
                Name = "Existing_WFI_Created",
                WorkflowInstance = new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Created").WorkflowRevision.WorkflowId,
                    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Created").WorkflowRequestMessage.PayloadId.ToString(),
                    StartTime = DateTime.Now,
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
                    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Dispatched").WorkflowRevision.WorkflowId,
                    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Dispatched").WorkflowRequestMessage.PayloadId.ToString(),
                    StartTime = DateTime.Now,
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
                    AeTitle = Helper.GetWorkflowByName("Task_Status_Update_Workflow").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Task_Status_Update_Workflow").WorkflowRevision.WorkflowId,
                    PayloadId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.Now,
                    BucketId = "bucket1",
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
                            TaskId = Helper.GetWorkflowByName("Task_Status_Update_Workflow").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Task_Status_Update_Workflow").WorkflowRevision.Workflow.Tasks[0].Type,
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
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
                    StartTime = DateTime.Now,
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
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.WorkflowId,
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
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Type,
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
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_2").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_2").WorkflowRevision.WorkflowId,
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
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_2").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_2").WorkflowRevision.Workflow.Tasks[0].Type,
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
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_3").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_3").WorkflowRevision.WorkflowId,
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
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_3").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_3").WorkflowRevision.Workflow.Tasks[0].Type,
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
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.WorkflowId,
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
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Dispatched
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Type,
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
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.WorkflowId,
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
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Dispatched
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Type,
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
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.WorkflowId,
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
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Dispatched
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Type,
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
                    AeTitle = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow").WorkflowRevision.WorkflowId,
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
                            TaskId = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow").WorkflowRevision.Workflow.Tasks[0].Type,
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
                    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination").WorkflowRevision.WorkflowId,
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
                            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination").WorkflowRevision.Workflow.Tasks[0].Id,
                            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination").WorkflowRevision.Workflow.Tasks[0].Type,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            },
        };
    }
}
