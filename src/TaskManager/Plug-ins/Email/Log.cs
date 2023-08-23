/*
 * Copyright 2023 MONAI Consortium
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


namespace Monai.Deploy.WorkflowManager.TaskManager.Email
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Error sending message {eventType}.")]
        public static partial void ErrorSendingMessage(this ILogger logger, string eventType, Exception ex);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Sending {eventType}.")]
        public static partial void SendEmailRequestMessage(this ILogger logger, string eventType);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "{eventType} sent.")]
        public static partial void SendEmailRequestMessageSent(this ILogger logger, string eventType);

        [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "No Metadata found in first dcm/json")]
        public static partial void NoMetaDataFound(this ILogger logger);

        [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "No Matching Metadata found in first dcm/json")]
        public static partial void NoMatchingMetaDataFound(this ILogger logger);

        [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "No Metadata requested in event")]
        public static partial void NoMetaDataRequested(this ILogger logger);

        [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Error Getting Metadata requested for file: {fileName} message:{message} ")]
        public static partial void ErrorGettingMetaData(this ILogger logger, string fileName, string message);
    }
}
