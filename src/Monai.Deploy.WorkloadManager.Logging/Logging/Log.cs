using Microsoft.Extensions.Logging;

<<<<<<< HEAD:src/WorkflowManager/Logging/Log.cs
using System;
using Microsoft.Extensions.Logging;

namespace Monai.Deploy.WorkflowManager.Logging
=======
namespace Monai.Deploy.WorkloadManager.Logging.Logging
>>>>>>> bd110cf (Add nuget package and add Rabbit Callbacks):src/Monai.Deploy.WorkloadManager.Logging/Logging/Log.cs
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
    }
}
