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

namespace Monai.Deploy.WorkflowManager.TaskManager.Docker.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Docker plugin initialized: base URL={baseUrl}.")]
        public static partial void Initialized(this ILogger logger, string baseUrl);

        [LoggerMessage(EventId = 1001, Level = LogLevel.Error, Message = "Error generating Container Specification.")]
        public static partial void ErrorGeneratingContainerSpecification(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 1002, Level = LogLevel.Error, Message = "Error deploying Container.")]
        public static partial void ErrorDeployingContainer(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Docker container created: container Id={containerId}.")]
        public static partial void CreatedContainer(this ILogger logger, string containerId);

        [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "Docker container started: container Id={containerId}.")]
        public static partial void StartedContainer(this ILogger logger, string containerId);

        [LoggerMessage(EventId = 1005, Level = LogLevel.Information, Message = "Docker container terminated: container Id={containerId}.")]
        public static partial void TerminatedContainer(this ILogger logger, string containerId);
    }
}
