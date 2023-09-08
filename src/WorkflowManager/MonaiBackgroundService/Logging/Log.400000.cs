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

namespace Monai.Deploy.WorkflowManager.MonaiBackgroundService.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 400000, Level = LogLevel.Warning, Message = "Task {taskId} started at {startTime} and been running for {duration}, Timing out task. ExecutionId: {executionId}, CorrelationId: {correlationId}")]
        public static partial void TimingOutTask(this ILogger logger, string taskId, string startTime, string duration, string executionId, string correlationId);

        [LoggerMessage(EventId = 400002, Level = LogLevel.Warning, Message = "CancellationEvent triggered, Identity: {identity}, WorkflowInstanceId: {workflowInstanceId}")]
        public static partial void TimingOutTaskCancellationEvent(this ILogger logger, string identity, string workflowInstanceId);

        [LoggerMessage(EventId = 400003, Level = LogLevel.Error, Message = "Worker encountered an error when running: {errorMessage}")]
        public static partial void WorkerException(this ILogger logger, string errorMessage);
    }
}
