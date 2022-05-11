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
                    WorkflowId = Helper.GetWorkflowIdByName("Multi_Request_Workflow_Created"),
                    PayloadId = Helper.GetPayloadIdByName("Multi_WF_Created"),
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
                    WorkflowId = Helper.GetWorkflowIdByName("Multi_Request_Workflow_Dispatched"),
                    PayloadId = Helper.GetPayloadIdByName("Multi_WF_Dispatched"),
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
