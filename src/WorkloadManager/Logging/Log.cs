// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using Microsoft.Extensions.Logging;

namespace Monai.Deploy.WorkloadManager.Logging
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
    }
}
