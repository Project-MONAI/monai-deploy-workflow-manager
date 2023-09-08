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
        [LoggerMessage(EventId = 900000, Level = LogLevel.Trace, Message = "{serviceName} started.")]
        public static partial void ServiceStarted(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 900001, Level = LogLevel.Trace, Message = "{serviceName} starting.")]
        public static partial void ServiceStarting(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 900002, Level = LogLevel.Trace, Message = "{serviceName} is stopping.")]
        public static partial void ServiceStopping(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 900003, Level = LogLevel.Information, Message = "Waiting for {serviceName} to stop.")]
        public static partial void ServiceStopPending(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 900004, Level = LogLevel.Warning, Message = "{serviceName} canceled.")]
        public static partial void ServiceCancelled(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 900005, Level = LogLevel.Warning, Message = "{serviceName} canceled.")]
        public static partial void ServiceCancelledWithException(this ILogger logger, string serviceName, Exception ex);

        [LoggerMessage(EventId = 900006, Level = LogLevel.Trace, Message = "{serviceName} Worker completed in: {elapsedMillis} milliseconds.")]
        public static partial void ServiceCompleted(this ILogger logger, string serviceName, int elapsedMillis);

        [LoggerMessage(EventId = 900007, Level = LogLevel.Error, Message = "Recovering connection to storage service:  {reason}.")]
        public static partial void MessagingServiceErrorRecover(this ILogger logger, string reason);
    }
}
