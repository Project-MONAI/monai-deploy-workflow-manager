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

        public static object ToLogControllerStartObject(string httpType, string path, string queryString, object body, string version, string environment)
        {
            return new
            {
                StartTime = DateTime.UtcNow,
                HttpType = httpType,
                path = path,
                queryString = queryString,
                body = body,
                version = version,
                environment = environment,
            };
        }

        public static object ToLogControllerEndObject(string httpType, string path, string queryString, string statusCode, string version, string environment)
        {
            return new
            {
                EndTime = DateTime.UtcNow,
                HttpType = httpType,
                path = path,
                queryString = queryString,
                statusCode = statusCode,
                version = version,
                environment = environment,
            };
        }
    }
}
