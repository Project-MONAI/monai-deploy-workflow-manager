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

namespace Monai.Deploy.WorkflowManager.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 500000, Level = LogLevel.Information, Message = "{ServiceName} subscribed to {RoutingKey} messages.")]
        public static partial void EventSubscription(this ILogger logger, string serviceName, string routingKey);

        [LoggerMessage(EventId = 500001, Level = LogLevel.Error, Message = "Workflow request message is invalid: {validationErrors}.")]
        public static partial void FailedToValidateWorkflowRequestEvent(this ILogger logger, string validationErrors);

        [LoggerMessage(EventId = 500002, Level = LogLevel.Error, Message = "Task update message is invalid.")]
        public static partial void FailedToValidateTaskUpdateEvent(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 500003, Level = LogLevel.Error, Message = "Export complete message is invalid.")]
        public static partial void FailedToValidateExportCompleteEvent(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 500004, Level = LogLevel.Error, Message = "Workflow request message {messageId} is invalid and has been rejected without being requeued.")]
        public static partial void WorkflowRequestRejectValidationError(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 500005, Level = LogLevel.Error, Message = "Workflow request message {messageId} failed unexpectedly and has been requeued.")]
        public static partial void WorkflowRequestRequeuePayloadCreateError(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 500006, Level = LogLevel.Error, Message = "Workflow request message {messageId} failed unexpectedly and has been requeued.")]
        public static partial void WorkflowRequestRequeuePayloadProcessError(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 500007, Level = LogLevel.Error, Message = "Workflow request message {messageId} failed unexpectedly and has been requeued.")]
        public static partial void WorkflowRequestRequeueUnknownError(this ILogger logger, string messageId, Exception ex);

        [LoggerMessage(EventId = 500008, Level = LogLevel.Error, Message = "Task update message {messageId} is invalid and has been rejected without being requeued.")]
        public static partial void TaskUpdateRejectValiationError(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 500009, Level = LogLevel.Error, Message = "Task update message {messageId} failed unexpectedly and has been requeued.")]
        public static partial void TaskUpdateRequeueProcessingError(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 500010, Level = LogLevel.Error, Message = "Task update message {messageId} failed unexpectedly and has been requeued.")]
        public static partial void TaskUpdateRequeueUnknownError(this ILogger logger, string messageId, Exception ex);

        [LoggerMessage(EventId = 500011, Level = LogLevel.Error, Message = "Export complete message {messageId} is invalid and has been rejected without being requeued.")]
        public static partial void ExportCompleteRejectValiationError(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 500012, Level = LogLevel.Error, Message = "Export complete message {messageId} failed unexpectedly and has been requeued.")]
        public static partial void ExportCompleteRequeueProcessingError(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 500013, Level = LogLevel.Error, Message = "Export complete message {messageId} failed unexpectedly and has been requeued.")]
        public static partial void ExportCompleteRequeueUnknownError(this ILogger logger, string messageId, Exception ex);

        [LoggerMessage(EventId = 500014, Level = LogLevel.Debug, Message = "Workflow request message received.")]
        public static partial void WorkflowRequestReceived(this ILogger logger);

        [LoggerMessage(EventId = 500015, Level = LogLevel.Debug, Message = "Task update message received.")]
        public static partial void TaskUpdateReceived(this ILogger logger);

        [LoggerMessage(EventId = 500016, Level = LogLevel.Debug, Message = "Export complete message received.")]
        public static partial void ExportCompleteReceived(this ILogger logger);

        [LoggerMessage(EventId = 500017, Level = LogLevel.Debug, Message = "ArtifactReceived message so not creating payload.")]
        public static partial void WorkflowContinuation(this ILogger logger);

        [LoggerMessage(EventId = 500018, Level = LogLevel.Debug, Message = "ArtifactReceived message received.")]
        public static partial void ArtifactReceivedReceived(this ILogger logger);

        [LoggerMessage(EventId = 500019, Level = LogLevel.Error, Message = "ArtifactReceived message {messageId} failed unexpectedly (no workflowId or TaskId ?) and has been requeued.")]
        public static partial void ArtifactReceivedRequeuePayloadCreateError(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 500020, Level = LogLevel.Error, Message = "ArtifactReveived message {messageId} failed unexpectedly and has been requeued.")]
        public static partial void ArtifactReceivedRequeueUnknownError(this ILogger logger, string messageId, Exception ex);

        [LoggerMessage(EventId = 500021, Level = LogLevel.Error, Message = "ArtifactReveived message {messageId} is invalid and has been rejected without being requeued.")]
        public static partial void ArtifactReceivedRejectValidationError(this ILogger logger, string messageId);

    }
}
