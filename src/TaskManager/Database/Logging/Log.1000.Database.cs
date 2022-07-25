// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Logging;

namespace Monai.Deploy.WorkflowManager.TaskManager.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 1000, Level = LogLevel.Error, Message = "Database call failed in {methodName}.")]
        public static partial void DatabaseException(this ILogger logger, string methodName, Exception ex);

        [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Task dispatch event saved {taskExecutionId}.")]
        public static partial void TaskDispatchEventSaved(this ILogger logger, string taskExecutionId);

        [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Task dispatch event deleted {taskExecutionId}.")]
        public static partial void TaskDispatchEventDeleted(this ILogger logger, string taskExecutionId);
    }
}
