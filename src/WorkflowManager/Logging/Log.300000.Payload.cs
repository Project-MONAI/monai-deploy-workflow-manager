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

namespace Monai.Deploy.WorkflowManager.Common.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 300000, Level = LogLevel.Information, Message = "Payload already exists for {payloadId}. This is likely due to being requeued.")]
        public static partial void PayloadAlreadyExists(this ILogger logger, string payloadId);

        [LoggerMessage(EventId = 300001, Level = LogLevel.Information, Message = "Payload created {payloadId}.")]
        public static partial void PayloadCreated(this ILogger logger, string payloadId);

        [LoggerMessage(EventId = 300002, Level = LogLevel.Error, Message = "Failed to create payload due to database error.")]
        public static partial void FailedToCreatedPayload(this ILogger logger);

        [LoggerMessage(EventId = 300003, Level = LogLevel.Error, Message = "Failed to get patient details in bucket {bucketId}. Payload: {payloadId}")]
        public static partial void FailedToGetPatientDetails(this ILogger logger, string payloadId, string bucketId, Exception ex);

        [LoggerMessage(EventId = 300004, Level = LogLevel.Information, Message = "Payload updated {payloadId}.")]
        public static partial void PayloadUpdated(this ILogger logger, string payloadId);

        [LoggerMessage(EventId = 300005, Level = LogLevel.Error, Message = "Failed to update payload {payloadId} due to database error.")]
        public static partial void PayloadUpdateFailed(this ILogger logger, string payloadId);

        [LoggerMessage(EventId = 300006, Level = LogLevel.Error, Message = "Failed to delete payload {payloadId} from storage.")]
        public static partial void PayloadDeleteFailed(this ILogger logger, string payloadId, Exception ex);
    }
}
