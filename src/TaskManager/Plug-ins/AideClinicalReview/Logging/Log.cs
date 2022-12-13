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

namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Error sending message {eventType}.")]
        public static partial void ErrorSendingMessage(this ILogger logger, string eventType, Exception ex);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Sending {eventType}, Name={name} .")]
        public static partial void SendClinicalReviewRequestMessage(this ILogger logger, string eventType, string name);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "{eventType} sent.")]
        public static partial void SendClinicalReviewRequestMessageSent(this ILogger logger, string eventType);

        [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Task: {taskId} given decision: {decision} at {dateTime} by user {userId} in app {appName}. Reason: {reason}. Note: {note}")]
        public static partial void RecordTaskDecision(this ILogger logger,
                                                        string taskId,
                                                        string decision,
                                                        string dateTime,
                                                        string userId,
                                                        string appName,
                                                        string reason,
                                                        string note);
    }
}
