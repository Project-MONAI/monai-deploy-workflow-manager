// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
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
                    AeTitle = "Task_Update",
                    WorkflowId = "11b62108-9ce2-4a9e-acb0-8060f19c2740",
                    PayloadId = "85a5638d-6218-4886-bf78-f74f252fc3b6",
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
        };
    }
}
