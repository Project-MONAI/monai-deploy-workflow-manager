// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Logging
{
    public static class LoggerHelpers
    {
        public static object ToTaskCompleteObject(TaskExecution task, WorkflowInstance workflowInstance, PatientDetails patientDetails, string correlationId, string taskStatus)
        {
            return new
            {
                ExecutionId = task.ExecutionId,
                TaskId = task.TaskId,
                WorkflowInstanceId = workflowInstance.Id,
                WorkflowId = workflowInstance.WorkflowId,
                CorrelationId = correlationId,
                TaskStatus = taskStatus,
                TaskType = task.TaskType,
                TaskStartTime = task.TaskStartTime,
                TaskEndTime = DateTime.UtcNow,
                TaskStatsObject = task.ExecutionStats,
                PatientDetails = patientDetails
            };
        }
    }
}
