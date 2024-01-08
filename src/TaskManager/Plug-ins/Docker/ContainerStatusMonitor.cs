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

using System.IO.Abstractions;
using Ardalis.GuardClauses;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Docker.Logging;

namespace Monai.Deploy.WorkflowManager.TaskManager.Docker
{
    public interface IContainerStatusMonitor
    {
        Task Start(TaskDispatchEvent taskDispatchEvent,
            TimeSpan containerTimeout,
            string containerId,
            ContainerVolumeMount intermediateVolumeMount,
            IReadOnlyList<ContainerVolumeMount> outputVolumeMounts,
            CancellationToken cancellationToken = default);
    }

    public class ContainerStatusMonitor : IContainerStatusMonitor, IDisposable
    {
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IServiceScope _scope;
        private readonly ILogger<ContainerStatusMonitor> _logger;
        private readonly IFileSystem _fileSystem;
        private bool _disposedValue;

        public ContainerStatusMonitor(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ContainerStatusMonitor> logger,
            IFileSystem fileSystem,
            IOptions<WorkflowManagerOptions> options)
        {
            if (serviceScopeFactory is null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }

            _scope = serviceScopeFactory.CreateScope();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task Start(
            TaskDispatchEvent taskDispatchEvent,
            TimeSpan containerTimeout,
            string containerId,
            ContainerVolumeMount intermediateVolumeMount,
            IReadOnlyList<ContainerVolumeMount> outputVolumeMounts,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(taskDispatchEvent, nameof(taskDispatchEvent));
            ArgumentNullException.ThrowIfNull(containerTimeout, nameof(containerTimeout));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(containerId, nameof(containerId));

            var dockerClientFactory = _scope.ServiceProvider.GetService<IDockerClientFactory>() ?? throw new ServiceNotFoundException(nameof(IDockerClientFactory));
            var dockerClient = dockerClientFactory.CreateClient(new Uri(taskDispatchEvent.TaskPluginArguments[Keys.BaseUrl]));

            var pollingPeriod = TimeSpan.FromSeconds(1);

            var timeToRetry = (int)containerTimeout.TotalSeconds;
            while (timeToRetry-- > 0)
            {
                try
                {
                    var response = await dockerClient.Containers.InspectContainerAsync(containerId, cancellationToken).ConfigureAwait(false);

                    if (IsContainerCompleted(response.State))
                    {
                        await UploadOutputArtifacts(intermediateVolumeMount, outputVolumeMounts, cancellationToken).ConfigureAwait(false);
                        await SendCallbackMessage(taskDispatchEvent, containerId).ConfigureAwait(false);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorMonitoringContainerStatus(containerId, ex);
                }
                finally
                {
                    await Task.Delay(pollingPeriod, cancellationToken).ConfigureAwait(false);
                }
            }

            _logger.TimedOutMonitoringContainerStatus(containerId);
        }

        internal static bool IsContainerCompleted(ContainerState state)
        {
            if (Strings.DockerEndStates.Contains(state.Status, StringComparer.InvariantCultureIgnoreCase) &&
                !string.IsNullOrWhiteSpace(state.FinishedAt))
            {
                return true;
            }
            return false;
        }

        private async Task UploadOutputArtifacts(ContainerVolumeMount intermediateVolumeMount, IReadOnlyList<ContainerVolumeMount> outputVolumeMounts, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(outputVolumeMounts, nameof(outputVolumeMounts));

            var storageService = _scope.ServiceProvider.GetService<IStorageService>() ?? throw new ServiceNotFoundException(nameof(IStorageService));
            var contentTypeProvider = _scope.ServiceProvider.GetService<IContentTypeProvider>() ?? throw new ServiceNotFoundException(nameof(IContentTypeProvider));

            if (intermediateVolumeMount is not null)
            {
                await UploadOutputArtifacts(storageService, contentTypeProvider, intermediateVolumeMount.Source, intermediateVolumeMount.TaskManagerPath, cancellationToken).ConfigureAwait(false);
            }

            foreach (var output in outputVolumeMounts)
            {
                await UploadOutputArtifacts(storageService, contentTypeProvider, output.Source, output.TaskManagerPath, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UploadOutputArtifacts(IStorageService storageService, IContentTypeProvider contentTypeProvider, Messaging.Common.Storage destination, string artifactsPath, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(destination, nameof(destination));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(artifactsPath, nameof(artifactsPath));

            IEnumerable<string> files;
            try
            {
                files = _fileSystem.Directory.EnumerateFiles(artifactsPath, "*", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                throw new ContainerMonitorException("Directory doesn't exist or no permission to access the directory.", ex);
            }

            if (!files.Any())
            {
                _logger.NoFilesFoundForUpload(artifactsPath);
            }
            foreach (var file in files)
            {
                try
                {
                    var objectName = file.Replace(artifactsPath, string.Empty).TrimStart('/');
                    objectName = _fileSystem.Path.Combine(destination.RelativeRootPath, objectName);
                    _logger.UploadingFile(file, destination.Bucket, objectName);
                    if (!contentTypeProvider.TryGetContentType(file, out var contentType))
                    {
                        contentType = GetContentType(_fileSystem.Path.GetExtension(file));
                    }
                    _logger.ContentTypeForFile(objectName, contentType);
                    using var stream = _fileSystem.File.OpenRead(file);
                    await storageService.PutObjectAsync(destination.Bucket, objectName, stream, stream.Length, contentType, new Dictionary<string, string>(), cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.ErrorUploadingFile(file, ex);
                }
            }
        }

        private static string GetContentType(string? ext)
        {
            if (string.IsNullOrWhiteSpace(ext))
            {
                return Strings.MimeTypeUnknown;
            }

            return ext.ToLowerInvariant() switch
            {
                Strings.FileExtensionDicom => Strings.MimeTypeDicom,
                _ => Strings.MimeTypeUnknown
            };
        }

        private async Task SendCallbackMessage(TaskDispatchEvent taskDispatchEvent, string containerId)
        {
            ArgumentNullException.ThrowIfNull(taskDispatchEvent, nameof(taskDispatchEvent));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(containerId, nameof(containerId));

            _logger.SendingCallbackMessage(containerId);
            var message = new JsonMessage<TaskCallbackEvent>(new TaskCallbackEvent
            {
                CorrelationId = taskDispatchEvent.CorrelationId,
                ExecutionId = taskDispatchEvent.ExecutionId,
                Identity = containerId,
                Outputs = taskDispatchEvent.Outputs ?? new List<Messaging.Common.Storage>(),
                TaskId = taskDispatchEvent.TaskId,
                WorkflowInstanceId = taskDispatchEvent.WorkflowInstanceId,
            }, applicationId: Strings.ApplicationId, correlationId: taskDispatchEvent.CorrelationId);

            var messageBrokerPublisherService = _scope.ServiceProvider.GetService<IMessageBrokerPublisherService>() ?? throw new ServiceNotFoundException(nameof(IMessageBrokerPublisherService));
            await messageBrokerPublisherService.Publish(_options.Value.Messaging.Topics.TaskCallbackRequest, message.ToMessage()).ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _scope.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
