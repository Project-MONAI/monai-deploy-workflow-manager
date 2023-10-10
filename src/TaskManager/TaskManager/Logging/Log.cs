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

namespace Monai.Deploy.WorkflowManager.TaskManager.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{ServiceName} started.")]
        public static partial void ServiceStarted(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "{ServiceName} is stopping.")]
        public static partial void ServiceStopping(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "{ServiceName} canceled.")]
        public static partial void ServiceCancelled(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "{ServiceName} canceled.")]
        public static partial void ServiceCancelledWithException(this ILogger logger, string serviceName, Exception ex);

        [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "{ServiceName} may be disposed.")]
        public static partial void ServiceDisposed(this ILogger logger, string serviceName, Exception ex);

        [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "{ServiceName} is running.")]
        public static partial void ServiceRunning(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Waiting for {ServiceName} to stop.")]
        public static partial void ServiceStopPending(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 8, Level = LogLevel.Error, Message = "Error querying database.")]
        public static partial void ErrorQueryingDatabase(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 9, Level = LogLevel.Critical, Message = "Type '{type}' cannot be found.")]
        public static partial void TypeNotFound(this ILogger logger, string type);

        [LoggerMessage(EventId = 10, Level = LogLevel.Critical, Message = "Instance of '{type}' cannot be found.")]
        public static partial void InstanceOfTypeNotFound(this ILogger logger, string type);

        [LoggerMessage(EventId = 11, Level = LogLevel.Critical, Message = "Instance of '{type}' cannot be found.")]
        public static partial void ServiceInvalidOrCancelled(this ILogger logger, string type, Exception ex);

        [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "{ServiceName} starting.")]
        public static partial void ServiceStarting(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 13, Level = LogLevel.Critical, Message = "Failed to start {ServiceName}.")]
        public static partial void ServiceFailedToStart(this ILogger logger, string serviceName, Exception ex);

        [LoggerMessage(EventId = 100, Level = LogLevel.Error, Message = "Error processing message, message ID={messageId}, correlation ID={correlationId}.")]
        public static partial void ErrorProcessingMessage(this ILogger logger, string? messageId, string? correlationId, Exception ex);

        [LoggerMessage(EventId = 101, Level = LogLevel.Warning, Message = "Insufficient resource available for task execution.")]
        public static partial void NoResourceAvailableForExecution(this ILogger logger);

        [LoggerMessage(EventId = 102, Level = LogLevel.Warning, Message = "Invalid message received, message ID={messageId}, correlation ID={correlationId}.")]
        public static partial void InvalidMessageReceived(this ILogger logger, string? messageId, string? correlationId, Exception ex);

        [LoggerMessage(EventId = 103, Level = LogLevel.Warning, Message = "Unsupported event {messageDescription}.")]
        public static partial void UnsupportedEvent(this ILogger logger, string? messageDescription);

        [LoggerMessage(EventId = 104, Level = LogLevel.Debug, Message = "Sending Nack message for {eventType} without re-queueing.")]
        public static partial void SendingRejectMessageNoRequeue(this ILogger logger, string? eventType);

        [LoggerMessage(EventId = 105, Level = LogLevel.Information, Message = "Nack message sent for {eventType} without re-queueing.")]
        public static partial void RejectMessageNoRequeueSent(this ILogger logger, string eventType);

        [LoggerMessage(EventId = 106, Level = LogLevel.Debug, Message = "Sending {eventType}, Status={reason} .")]
        public static partial void SendingTaskUpdateMessage(this ILogger logger, string eventType, FailureReason reason);

        [LoggerMessage(EventId = 107, Level = LogLevel.Information, Message = "{eventType} sent.")]
        public static partial void TaskUpdateMessageSent(this ILogger logger, string eventType);

        [LoggerMessage(EventId = 108, Level = LogLevel.Error, Message = "Error sending message {eventType}.")]
        public static partial void ErrorSendingMessage(this ILogger logger, string eventType, Exception ex);

        [LoggerMessage(EventId = 109, Level = LogLevel.Warning, Message = "Unable to query for job status, no activate executor associated with execution ID={executionId}.")]
        public static partial void NoActiveExecutorWithTheId(this ILogger logger, string executionId);

        [LoggerMessage(EventId = 110, Level = LogLevel.Error, Message = "Exception initialising task runner: '{assemblyName}'.")]
        public static partial void UnsupportedRunner(this ILogger logger, string assemblyName, Exception ex);

        [LoggerMessage(EventId = 111, Level = LogLevel.Debug, Message = "Sending acknowledgment message for {eventType}.")]
        public static partial void SendingAckMessage(this ILogger logger, string eventType);

        [LoggerMessage(EventId = 112, Level = LogLevel.Information, Message = "Acknowledgment message sent for {eventType}.")]
        public static partial void AckMessageSent(this ILogger logger, string eventType);

        [LoggerMessage(EventId = 113, Level = LogLevel.Error, Message = "Error executing task plugin.")]
        public static partial void ErrorExecutingTask(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 114, Level = LogLevel.Warning, Message = "Plug-in exceeded time limit and has been removed: execution ID={executionId}.")]
        public static partial void RunnerTimedOut(this ILogger logger, string executionId);

        [LoggerMessage(EventId = 115, Level = LogLevel.Error, Message = "Error generating temporary storage credentials.")]
        public static partial void GenerateTemporaryCredentialsException(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 116, Level = LogLevel.Error, Message = "Metadata retrieval failed.")]
        public static partial void MetadataRetrievalFailed(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 117, Level = LogLevel.Warning, Message = "Metadata feature unavailable for {plugin}.")]
        public static partial void MetadataPluginUndefined(this ILogger logger, string plugin);

        [LoggerMessage(EventId = 118, Level = LogLevel.Error, Message = "Error removing storage user account {username}.")]
        public static partial void ErrorRemovingStorageUserAccount(this ILogger logger, string username, Exception exception);

        [LoggerMessage(EventId = 119, Level = LogLevel.Debug, Message = "Processing task cancellation event")]
        public static partial void PrecessingTaskCancellationEvent(this ILogger logger);

        [LoggerMessage(EventId = 120, Level = LogLevel.Error, Message = "Recovering connection to storage service:  {reason}.")]
        public static partial void MessagingServiceErrorRecover(this ILogger logger, string reason);

        [LoggerMessage(EventId = 121, Level = LogLevel.Error, Message = "Exception handling task : '{assemblyName}' timeout.")]
        public static partial void ExectionTimingOutTask(this ILogger logger, string assemblyName, Exception ex);
    }
}
