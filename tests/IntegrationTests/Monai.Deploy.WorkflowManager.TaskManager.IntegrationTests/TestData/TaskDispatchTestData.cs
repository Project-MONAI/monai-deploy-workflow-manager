﻿using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.TestData
{
    public class TaskDispatchTestData
    {
        public string? Name { get; set; }

        public TaskDispatchEvent? TaskDispatchEvent { get; set; }
    }

    public static class TaskDispatchesTestData
    {
        public static List<TaskDispatchTestData> TestData = new List<TaskDispatchTestData>()
        {
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "argo"
                }
            },
        };
    }
}
