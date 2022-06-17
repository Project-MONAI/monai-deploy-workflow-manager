// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Logging
{
    public static class LoggerHelpers
    {
        public static object ToTaskCompleteObject(TaskExecution task, string workflowInstanceId, string correlationId, string taskStatus)
        {
            return new
            {
                ExecutionId = task.ExecutionId,
                TaskId = task.TaskId,
                WorkflowInstanceId = workflowInstanceId,
                CorrelationId = correlationId,
                TaskStatus = taskStatus,
                TaskType = task.TaskType,
                TaskStatsObject = "", //Comes later
            };
        }
    }
}
