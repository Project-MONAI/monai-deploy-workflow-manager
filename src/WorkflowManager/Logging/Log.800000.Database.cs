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

using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 800000, Level = LogLevel.Error, Message = "Failed to create and save payload.")]
        public static partial void DbPayloadCreationError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 800001, Level = LogLevel.Error, Message = "Failed to associate workflow instances to payload.")]
        public static partial void DbUpdateWorkflowInstanceError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 800002, Level = LogLevel.Error, Message = "Failed to load tasks from the database.")]
        public static partial void DbGetAllTasksError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 800004, Level = LogLevel.Error, Message = "Failed to load task '{taskId}' from the database.")]
        public static partial void DbGetTasksError(this ILogger logger, string taskId, Exception ex);

        [LoggerMessage(EventId = 800005, Level = LogLevel.Error, Message = "Failed to load workflow instances from the database.")]
        public static partial void DbGetWorkflowInstancesError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 800006, Level = LogLevel.Error, Message = "Failed to create and save workflow instance.")]
        public static partial void DbCreateWorkflowInstancesError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 800007, Level = LogLevel.Error, Message = "Failed to update workflow instance '{workflowInstanceId}'.")]
        public static partial void DbUpdateWorkflowInstancesError(this ILogger logger, string workflowInstanceId, Exception ex);

        [LoggerMessage(EventId = 800008, Level = LogLevel.Error, Message = "Failed to save task status '{taskId}' with '{status}'.")]
        public static partial void DbUpdateTaskStatusError(this ILogger logger, string taskId, TaskExecutionStatus status, Exception ex);

        [LoggerMessage(EventId = 800009, Level = LogLevel.Error, Message = "Failed to update task output artifact '{taskId}'.")]
        public static partial void DbUpdateTaskOutputArtifactError(this ILogger logger, string taskId, Exception ex);

        [LoggerMessage(EventId = 800010, Level = LogLevel.Error, Message = "Failed to update workflow instance '{workflowInstanceId}' with '{status}'.")]
        public static partial void DbUpdateWorkflowInstanceStatusError(this ILogger logger, string workflowInstanceId, Status status, Exception ex);

        [LoggerMessage(EventId = 800011, Level = LogLevel.Error, Message = "Failed to load task '{taskId}' from the database.")]
        public static partial void DbGetTaskByIdError(this ILogger logger, string taskId, Exception ex);

        [LoggerMessage(EventId = 800012, Level = LogLevel.Error, Message = "Failed to update tasks for workflow instance '{workflowInstanceId}'.")]
        public static partial void DbUpdateTasksError(this ILogger logger, string workflowInstanceId, Exception ex);

        [LoggerMessage(EventId = 800013, Level = LogLevel.Error, Message = "Database call failed in {methodName}.")]
        public static partial void DatabaseException(this ILogger logger, string methodName, Exception ex);

        [LoggerMessage(EventId = 800014, Level = LogLevel.Error, Message = "Failed to update payload: '{payloadId}'.")]
        public static partial void DbUpdatePayloadError(this ILogger logger, string payloadId, Exception ex);

        [LoggerMessage(EventId = 800015, Level = LogLevel.Error, Message = "Failed to get payloads to delete.")]
        public static partial void DbGetPayloadsToDeleteError(this ILogger logger, Exception ex);
    }
}
