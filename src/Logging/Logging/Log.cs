// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Logging;

namespace Monai.Deploy.WorkflowManager.Logging.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{ServiceName} started.")]
        public static partial void ServiceStarted(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "{ServiceName} starting.")]
        public static partial void ServiceStarting(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "{ServiceName} is stopping.")]
        public static partial void ServiceStopping(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Waiting for {ServiceName} to stop.")]
        public static partial void ServiceStopPending(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "{ServiceName} canceled.")]
        public static partial void ServiceCancelled(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "{ServiceName} canceled.")]
        public static partial void ServiceCancelledWithException(this ILogger logger, string serviceName, Exception ex);

        [LoggerMessage(EventId = 7, Level = LogLevel.Warning, Message = "{ServiceName} may be disposed.")]
        public static partial void ServiceDisposed(this ILogger logger, string serviceName, Exception ex);

        [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "{ServiceName} is running.")]
        public static partial void ServiceRunning(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 9, Level = LogLevel.Critical, Message = "Failed to start {ServiceName}.")]
        public static partial void ServiceFailedToStart(this ILogger logger, string serviceName, Exception ex);

        [LoggerMessage(EventId = 10, Level = LogLevel.Critical, Message = "Type '{type}' cannot be found.")]
        public static partial void TypeNotFound(this ILogger logger, string type);

        [LoggerMessage(EventId = 11, Level = LogLevel.Critical, Message = "Instance of '{type}' cannot be found.")]
        public static partial void InstanceOfTypeNotFound(this ILogger logger, string type);

        [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "{ServiceName} subscribed to {RoutingKey} messages.")]
        public static partial void EventSubscription(this ILogger logger, string serviceName, string routingKey);

        [LoggerMessage(EventId = 13, Level = LogLevel.Error, Message = "{message}")]
        public static partial void Exception(this ILogger logger, string message, Exception ex);

        [LoggerMessage(EventId = 14, Level = LogLevel.Error, Message = "The following validaition errors occured: {validationErrors}")]
        public static partial void ValidationErrors(this ILogger logger, string validationErrors);

        [LoggerMessage(EventId = 15, Level = LogLevel.Error, Message = "The following message {messageId} event validation has failed and has been rejected without being requeued.")]
        public static partial void EventRejectedNoQueue(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 16, Level = LogLevel.Error, Message = "The following message {messageId} failed unexpectedly and has been rejected and requeued.")]
        public static partial void EventRejectedRequeue(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 17, Level = LogLevel.Error, Message = "The following transaction {methodName} failed unexpectedly and has been aborted.")]
        public static partial void TransactionFailed(this ILogger logger, string methodName, Exception ex);

        [LoggerMessage(EventId = 18, Level = LogLevel.Error, Message = "The following database call {methodName} failed unexpectedly and has been aborted.")]
        public static partial void DbCallFailed(this ILogger logger, string methodName, Exception ex);

        [LoggerMessage(EventId = 19, Level = LogLevel.Error, Message = "The following task has already been dispatched: {payloadId}, {taskId}")]
        public static partial void TaskPreviouslyDispatched(this ILogger logger, string payloadId, string taskId);

        [LoggerMessage(EventId = 20, Level = LogLevel.Error, Message = "The following task: {taskId} cannot be found in the workflow: {workflowId}. Payload: {payloadId}")]
        public static partial void TaskNotFoundInWorkfow(this ILogger logger, string payloadId, string taskId, string workflowId);

        [LoggerMessage(EventId = 21, Level = LogLevel.Error, Message = "The following task: {taskId} cannot be found in the workflow instance: {workflowInstanceId}.")]
        public static partial void TaskNotFoundInWorkfowInstance(this ILogger logger, string taskId, string workflowInstanceId);

        [LoggerMessage(EventId = 22, Level = LogLevel.Error, Message = "The task execution status for task {taskId} cannot be updated from {oldStatus} to {newStatus}. Payload: {payloadId}")]
        public static partial void TaskStatusUpdateNotValid(this ILogger logger, string payloadId, string taskId, string oldStatus, string newStatus);

        [LoggerMessage(EventId = 23, Level = LogLevel.Error, Message = "The task {taskId} metadata store update failed. Payload: {payloadId}")]
        public static partial void TaskMetaDataUpdateFailed(this ILogger logger, string payloadId, string taskId, Dictionary<string, object> metadata);
    }
}
