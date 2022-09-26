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

namespace Monai.Deploy.WorkflowManager.TaskManager.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 1000, Level = LogLevel.Error, Message = "Database call failed in {methodName}.")]
        public static partial void DatabaseException(this ILogger logger, string methodName, Exception ex);

        [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Task dispatch event saved {executionId}.")]
        public static partial void TaskDispatchEventSaved(this ILogger logger, string executionId);

        [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Task dispatch event deleted {executionId}.")]
        public static partial void TaskDispatchEventDeleted(this ILogger logger, string executionId);
    }
}
