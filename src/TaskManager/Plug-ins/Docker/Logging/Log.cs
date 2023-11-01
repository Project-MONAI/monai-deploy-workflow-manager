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
        [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Docker plugin initialized: base URL={baseUrl}, timeout={timeoutMinutes} minutes.")]
        public static partial void Initialized(this ILogger logger, string baseUrl, double timeoutMinutes);

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

        [LoggerMessage(EventId = 1006, Level = LogLevel.Information, Message = "Input volume mapping host=={hostPath}, container={containerPath}.")]
        public static partial void DockerInputMapped(this ILogger logger, string hostPath, string containerPath);

        [LoggerMessage(EventId = 1007, Level = LogLevel.Information, Message = "Output volume mapping host=={hostPath}, container={containerPath}.")]
        public static partial void DockerOutputMapped(this ILogger logger, string hostPath, string containerPath);

        [LoggerMessage(EventId = 1008, Level = LogLevel.Information, Message = "Environment variabled added {key}={value}.")]
        public static partial void DockerEnvironmentVariableAdded(this ILogger logger, string key, string value);

        [LoggerMessage(EventId = 1009, Level = LogLevel.Error, Message = "Error retreiving status from container {identity}.")]
        public static partial void ErrorGettingStatusFromDocker(this ILogger logger, string identity, Exception ex);

        [LoggerMessage(EventId = 1010, Level = LogLevel.Debug, Message = "Downloading artifact {source} to {target}.")]
        public static partial void DownloadingArtifactFromStorageService(this ILogger logger, string source, string target);

        [LoggerMessage(EventId = 1011, Level = LogLevel.Warning, Message = "No input volumes configured for the task.")]
        public static partial void NoInputVolumesConfigured(this ILogger logger);

        [LoggerMessage(EventId = 1012, Level = LogLevel.Warning, Message = "No files found in bucket {bucket} - {path}.")]
        public static partial void NoFilesFoundIn(this ILogger logger, string bucket, string path);

        [LoggerMessage(EventId = 1013, Level = LogLevel.Warning, Message = "No output volumes configured for the task.")]
        public static partial void NoOutputVolumesConfigured(this ILogger logger);

        [LoggerMessage(EventId = 10014, Level = LogLevel.Information, Message = "Intermediate volume mapping host=={hostPath}, container={containerPath}.")]
        public static partial void DockerIntermediateVolumeMapped(this ILogger logger, string hostPath, string containerPath);

        [LoggerMessage(EventId = 1015, Level = LogLevel.Error, Message = "Error generating volume mounts.")]
        public static partial void ErrorGeneratingVolumeMounts(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = 1016, Level = LogLevel.Error, Message = "Error uploading file {file}.")]
        public static partial void ErrorUploadingFile(this ILogger logger, string file, Exception exception);

        [LoggerMessage(EventId = 1017, Level = LogLevel.Debug, Message = "Uploading {source} to {bucket} - {destination}")]
        public static partial void UploadingFile(this ILogger logger, string source, string bucket, string destination);

        [LoggerMessage(EventId = 1018, Level = LogLevel.Information, Message = "Sending task callback event for completed container {containerId}.")]
        public static partial void SendingCallbackMessage(this ILogger logger, string containerId);

        [LoggerMessage(EventId = 1019, Level = LogLevel.Error, Message = "Error monitoring container {containerId}.")]
        public static partial void ErrorMonitoringContainerStatus(this ILogger logger, string containerId, Exception ex);

        [LoggerMessage(EventId = 1020, Level = LogLevel.Warning, Message = "Timeout waiting for container to complete {containerId}.")]
        public static partial void TimedOutMonitoringContainerStatus(this ILogger logger, string containerId);

        [LoggerMessage(EventId = 1021, Level = LogLevel.Warning, Message = "No files found in {artifactsPath} for upload.")]
        public static partial void NoFilesFoundForUpload(this ILogger logger, string artifactsPath);

        [LoggerMessage(EventId = 1022, Level = LogLevel.Debug, Message = "Content type set to {contentType} for {filename}.")]
        public static partial void ContentTypeForFile(this ILogger logger, string filename, string contentType);

        [LoggerMessage(EventId = 1023, Level = LogLevel.Error, Message = "Error pulling container image {image}.")]
        public static partial void ErrorPullingContainerImage(this ILogger logger, string image, Exception ex);

        [LoggerMessage(EventId = 1024, Level = LogLevel.Error, Message = "Error starting container monitoring for container {container}.")]
        public static partial void ErrorLaunchingContainerMonitor(this ILogger logger, string container, Exception ex);

        [LoggerMessage(EventId = 1025, Level = LogLevel.Warning, Message = "Container '{container}' created with warnings: {warnings}.")]
        public static partial void ContainerCreatedWithWarnings(this ILogger logger, string container, string warnings);

        [LoggerMessage(EventId = 1026, Level = LogLevel.Error, Message = "Error terminating container '{container}'.")]
        public static partial void ErrorTerminatingContainer(this ILogger logger, string container, Exception ex);

        [LoggerMessage(EventId = 1027, Level = LogLevel.Information, Message = "Image does not exist '{image}' locally, attempting to pull.")]
        public static partial void ImageDoesNotExist(this ILogger logger, string image);

        [LoggerMessage(EventId = 1028, Level = LogLevel.Information, Message = "Input volume added {hostPath} = {containerPath}.")]
        public static partial void InputVolumeMountAdded(this ILogger logger, string hostPath, string containerPath);

        [LoggerMessage(EventId = 1029, Level = LogLevel.Information, Message = "Output volume added {hostPath} = {containerPath}.")]
        public static partial void OutputVolumeMountAdded(this ILogger logger, string hostPath, string containerPath);

        [LoggerMessage(EventId = 1030, Level = LogLevel.Information, Message = "Intermediate volume added {hostPath} = {containerPath}.")]
        public static partial void IntermediateVolumeMountAdded(this ILogger logger, string hostPath, string containerPath);

        [LoggerMessage(EventId = 1031, Level = LogLevel.Error, Message = "Error setting directory {path} with permission {user}.")]
        public static partial void ErrorSettingDirectoryPermission(this ILogger logger, string path, string user);
    }
}
