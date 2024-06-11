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

using System.Diagnostics;
using System.Globalization;
using Ardalis.GuardClauses;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Docker.Logging;

namespace Monai.Deploy.WorkflowManager.TaskManager.Docker
{
    public class DockerPlugin : TaskPluginBase
    {
        private TimeSpan _containerTimeout;
        private readonly IDockerClient _dockerClient;
        private readonly ILogger<DockerPlugin> _logger;
        private readonly IServiceScope _scope;
        private readonly string _hostTemporaryStoragePath;

        public DockerPlugin(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<DockerPlugin> logger,
            TaskDispatchEvent taskDispatchEvent)
            : base(taskDispatchEvent)
        {
            ArgumentNullException.ThrowIfNull(serviceScopeFactory, nameof(serviceScopeFactory));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scope = serviceScopeFactory.CreateScope() ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            ValidateEvent();
            Initialize();

            _hostTemporaryStoragePath = Environment.GetEnvironmentVariable(Strings.HostTemporaryStorageEnvironmentVariableName) ?? throw new ApplicationException($"Environment variable {Strings.HostTemporaryStorageEnvironmentVariableName} is not set.");
            var dockerClientFactory = _scope.ServiceProvider.GetService<IDockerClientFactory>() ?? throw new ServiceNotFoundException(nameof(IDockerClientFactory));
            _dockerClient = dockerClientFactory.CreateClient(new Uri(Event.TaskPluginArguments[Keys.BaseUrl]));
        }

        private void Initialize()
        {
            if (Event.TaskPluginArguments.ContainsKey(Keys.TaskTimeoutMinutes) &&
                int.TryParse(Event.TaskPluginArguments[Keys.TaskTimeoutMinutes], out var timeoutMinutes))
            {
                _containerTimeout = TimeSpan.FromMinutes(timeoutMinutes);
            }
            else
            {
                _containerTimeout = TimeSpan.FromMinutes(5);
            }

            _logger.Initialized(Event.TaskPluginArguments[Keys.BaseUrl], _containerTimeout.TotalMinutes);
        }

        private void ValidateEvent()
        {
            if (Event.TaskPluginArguments is null || Event.TaskPluginArguments.Count == 0)
            {
                throw new InvalidTaskException($"Required parameters to execute Argo workflow are missing: {string.Join(',', Keys.RequiredParameters)}");
            }

            foreach (var key in Keys.RequiredParameters)
            {
                if (!Event.TaskPluginArguments.ContainsKey(key))
                {
                    throw new InvalidTaskException($"Required parameter to execute Argo workflow is missing: {key}");
                }
            }

            if (!Uri.IsWellFormedUriString(Event.TaskPluginArguments[Keys.BaseUrl], UriKind.Absolute))
            {
                throw new InvalidTaskException($"The value '{Event.TaskPluginArguments[Keys.BaseUrl]}' provided for '{Keys.BaseUrl}' is not a valid URI.");
            }

            Event.Inputs.ForEach(p => ValidateStorageMappings(p));
            Event.Outputs.ForEach(p => ValidateStorageMappings(p));
        }

        private void ValidateStorageMappings(Messaging.Common.Storage storage)
        {
            ArgumentNullException.ThrowIfNull(storage, nameof(storage));

            if (!Event.TaskPluginArguments.ContainsKey(storage.Name))
            {
                throw new InvalidTaskException($"The mapping for storage '{storage.Name}' is not defined as an envrionment variable.");
            }
        }

        public override async Task<ExecutionStatus> ExecuteTask(CancellationToken cancellationToken = default)
        {
            List<ContainerVolumeMount> inputVolumeMounts;
            ContainerVolumeMount intermediateVolumeMount;
            List<ContainerVolumeMount> outputVolumeMounts;

            using var loggingScope = _logger.BeginScope(new LoggingDataDictionary<string, object>
            {
                ["correlationId"] = Event.CorrelationId,
                ["workflowInstanceId"] = Event.WorkflowInstanceId,
                ["taskId"] = Event.TaskId,
                ["executionId"] = Event.ExecutionId
            });

            try
            {
                inputVolumeMounts = await SetupInputs(cancellationToken).ConfigureAwait(false);
                intermediateVolumeMount = SetupIntermediateVolume();
                outputVolumeMounts = SetupOutputs();
            }
            catch (Exception exception)
            {
                _logger.ErrorGeneratingVolumeMounts(exception);
                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError, Errors = exception.Message };
            }

            try
            {
                var alwaysPull = Event.TaskPluginArguments.ContainsKey(Keys.AlwaysPull) && Event.TaskPluginArguments[Keys.AlwaysPull].Equals("true", StringComparison.OrdinalIgnoreCase);
                if (alwaysPull || !await ImageExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    // Pull image.
                    _logger.ImageDoesNotExist(Event.TaskPluginArguments[Keys.ContainerImage]);
                    var imageCreateParameters = new ImagesCreateParameters()
                    {
                        FromImage = Event.TaskPluginArguments[Keys.ContainerImage],
                    };
                    await _dockerClient.Images.CreateImageAsync(imageCreateParameters, new AuthConfig(), new Progress<JSONMessage>(), cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                _logger.ErrorPullingContainerImage(Event.TaskPluginArguments[Keys.ContainerImage], exception);
                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError, Errors = exception.Message };
            }

            CreateContainerParameters parameters;
            try
            {
                parameters = BuildContainerSpecification(inputVolumeMounts, intermediateVolumeMount, outputVolumeMounts);
            }
            catch (Exception exception)
            {
                _logger.ErrorGeneratingContainerSpecification(exception);
                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError, Errors = exception.Message };
            }

            string containerId;
            try
            {
                var response = await _dockerClient.Containers.CreateContainerAsync(parameters, cancellationToken).ConfigureAwait(false);
                containerId = response.ID;

                if (response.Warnings.Any())
                {
                    _logger.ContainerCreatedWithWarnings(containerId, string.Join(".", response.Warnings));
                }

                _logger.CreatedContainer(containerId);

                _ = await _dockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters(), cancellationToken).ConfigureAwait(false);
                _logger.StartedContainer(containerId);
            }
            catch (Exception exception)
            {
                _logger.ErrorDeployingContainer(exception);
                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError, Errors = exception.Message };
            }

            try
            {
                var monitor = _scope.ServiceProvider.GetService<IContainerStatusMonitor>() ?? throw new ServiceNotFoundException(nameof(IContainerStatusMonitor));
                _ = Task.Run(async () =>
                {
                    await monitor.Start(Event, _containerTimeout, containerId, intermediateVolumeMount, outputVolumeMounts, cancellationToken);
                });
            }
            catch (Exception exception)
            {
                _logger.ErrorLaunchingContainerMonitor(containerId, exception);
            }

            return new ExecutionStatus()
            {
                Status = TaskExecutionStatus.Accepted,
                Stats = new Dictionary<string, string> { { Strings.IdentityKey, containerId } }
            };
        }

        private async Task<bool> ImageExistsAsync(CancellationToken cancellationToken)
        {
            var imageListParameters = new ImagesListParameters
            {
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    { "reference", new Dictionary<string, bool> { { Event.TaskPluginArguments[Keys.ContainerImage], true } } }
                }
            };

            var results = await _dockerClient.Images.ListImagesAsync(imageListParameters, cancellationToken);
            return results?.Any() ?? false;
        }

        public override async Task<ExecutionStatus> GetStatus(string identity, TaskCallbackEvent callbackEvent, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(identity, nameof(identity));

            try
            {
                var response = await _dockerClient.Containers.InspectContainerAsync(identity, cancellationToken).ConfigureAwait(false);
                var retryCount = 12;
                while ((response == null || !ContainerStatusMonitor.IsContainerCompleted(response.State)) && retryCount-- > 0)
                {
                    await Task.Delay(250, cancellationToken).ConfigureAwait(false);
                    response = await _dockerClient.Containers.InspectContainerAsync(identity, cancellationToken).ConfigureAwait(false);
                }

                if (response == null)
                {
                    throw new InvalidOperationException($"Unable to obtain status for container {identity}");
                }

                var stats = GetExecutuionStats(response);
                if (ContainerStatusMonitor.IsContainerCompleted(response.State))
                {
                    return new ExecutionStatus
                    {
                        Status = TaskExecutionStatus.Succeeded,
                        FailureReason = FailureReason.None,
                        Stats = stats
                    };
                }
                else if (response.State.OOMKilled || response.State.Dead)
                {
                    return new ExecutionStatus
                    {
                        Status = TaskExecutionStatus.Failed,
                        FailureReason = FailureReason.ExternalServiceError,
                        Errors = $"Exit code={response.State.ExitCode}",
                        Stats = stats
                    };
                }
                else
                {
                    return new ExecutionStatus
                    {
                        Status = TaskExecutionStatus.Failed,
                        FailureReason = FailureReason.Unknown,
                        Errors = $"Exit code={response.State.ExitCode}. Status={response.State.Status}.",
                        Stats = stats
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorGettingStatusFromDocker(identity, ex);
                return new ExecutionStatus
                {
                    Status = TaskExecutionStatus.Failed,
                    FailureReason = FailureReason.ExternalServiceError,
                    Errors = ex.Message
                };
            }
        }

        private Dictionary<string, string> GetExecutuionStats(ContainerInspectResponse response)
        {
            ArgumentNullException.ThrowIfNull(response, nameof(response));

            TimeSpan? duration = null;

            if (DateTime.TryParse(response.State.FinishedAt, out var completedTime))
            {
                duration = completedTime.Subtract(response.Created);
            }

            return new Dictionary<string, string>
            {
                { "workflowId", Event.WorkflowInstanceId },
                { "duration", duration.HasValue ? duration.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture) : string.Empty },
                { "startedAt", response.Created.ToString("s")  },
                { "finishedAt", completedTime.ToString("s") }
            };
        }

        public override async Task HandleTimeout(string identity)
        {
            try
            {
                await _dockerClient.Containers.KillContainerAsync(identity, new ContainerKillParameters()).ConfigureAwait(false);
                _logger.TerminatedContainer(identity);
            }
            catch (Exception ex)
            {
                _logger.ErrorTerminatingContainer(identity, ex);
                throw;
            }
        }

        private CreateContainerParameters BuildContainerSpecification(IList<ContainerVolumeMount> inputs, ContainerVolumeMount intermediateVolumeMount, IList<ContainerVolumeMount> outputs)
        {
            var entrypoint = Event.TaskPluginArguments[Keys.EntryPoint].Split(',');
            var command = Event.TaskPluginArguments[Keys.Command].Split(',');

            var volumeMounts = new List<Mount>();

            foreach (var input in inputs)
            {
                var mount = new Mount { Type = "bind", ReadOnly = true, Source = input.HostPath, Target = input.ContainerPath };
                volumeMounts.Add(mount);
                _logger.DockerInputMapped(input.HostPath, input.ContainerPath);
            }

            foreach (var output in outputs)
            {
                var mount = new Mount { Type = "bind", ReadOnly = false, Source = output.HostPath, Target = output.ContainerPath };
                volumeMounts.Add(mount);
                _logger.DockerOutputMapped(output.HostPath, output.ContainerPath);
            }

            if (intermediateVolumeMount is not null)
            {
                volumeMounts.Add(new Mount { Type = "bind", ReadOnly = false, Source = intermediateVolumeMount.HostPath, Target = intermediateVolumeMount.ContainerPath });
                _logger.DockerIntermediateVolumeMapped(intermediateVolumeMount.HostPath, intermediateVolumeMount.ContainerPath);
            }

            var envvars = new List<string>();

            foreach (var key in Event.TaskPluginArguments.Keys)
            {
                if (key.StartsWith(Keys.EnvironmentVariableKeyPrefix, false, CultureInfo.InvariantCulture))
                {
                    var envVarKey = key.Replace(Keys.EnvironmentVariableKeyPrefix, string.Empty);
                    envvars.Add($"{envVarKey}={Event.TaskPluginArguments[key]}");
                    _logger.DockerEnvironmentVariableAdded(envVarKey, Event.TaskPluginArguments[key]);
                }
            }

            var parameters = new CreateContainerParameters()
            {
                Name = Event.ExecutionId,
                Env = envvars,
                Entrypoint = entrypoint,
                Cmd = command,
                Image = Event.TaskPluginArguments[Keys.ContainerImage],
                HostConfig = new HostConfig()
                {
                    Runtime = Strings.RuntimeNvidia,
                    Mounts = volumeMounts,
                },
            };

            if (Event.TaskPluginArguments.ContainsKey(Keys.User))
            {
                parameters.User = Event.TaskPluginArguments[Keys.User];
            }

            return parameters;
        }

        private async Task<List<ContainerVolumeMount>> SetupInputs(CancellationToken cancellationToken = default)
        {
            var volumeMounts = new List<ContainerVolumeMount>();

            if (Event.Inputs.Count == 0)
            {
                _logger.NoInputVolumesConfigured();
                return volumeMounts;
            }

            var storageService = _scope.ServiceProvider.GetService<IStorageService>() ?? throw new ServiceNotFoundException(nameof(IStorageService));

            // Container Path of the Input Directory.
            var containerPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageContainerPath], Event.ExecutionId, "inputs");
            // Host Path of the Input Directory.
            var hostPath = Path.Combine(_hostTemporaryStoragePath, Event.ExecutionId, "inputs");

            foreach (var input in Event.Inputs)
            {
                var objects = await storageService.ListObjectsAsync(input.Bucket, input.RelativeRootPath, true, cancellationToken).ConfigureAwait(false);
                if (objects.Count == 0)
                {
                    _logger.NoFilesFoundIn(input.Bucket, input.RelativeRootPath);
                    continue;
                }

                var inputDirName = input.Name;

                // Host Path of the Directory for this Input File.
                // <host-path>/id/
                var inputHostDirRoot = Path.Combine(hostPath, inputDirName);
                var inputContainerDirRoot = Path.Combine(containerPath, inputDirName);

                // eg: /monai/input
                var volumeMount = new ContainerVolumeMount(input, Event.TaskPluginArguments[input.Name], inputHostDirRoot, inputContainerDirRoot);
                volumeMounts.Add(volumeMount);
                _logger.InputVolumeMountAdded(inputHostDirRoot, inputContainerDirRoot);

                // For each file, download from bucket and store in Task Manager Container.
                foreach (var obj in objects)
                {
                    // Task Manager Container Path of the Input File.
                    var filePath = Path.Combine(inputContainerDirRoot, obj.FilePath.Replace(input.RelativeRootPath, "").TrimStart('/'));

                    // Task Manager Container Path of the Directory for this Input File.
                    var fileDirectory = Path.GetDirectoryName(filePath);
                    Directory.CreateDirectory(fileDirectory!);

                    _logger.DownloadingArtifactFromStorageService(obj.Filename, filePath);
                    using var stream = await storageService.GetObjectAsync(input.Bucket, obj.FilePath, cancellationToken).ConfigureAwait(false) as MemoryStream;
                    using var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
                    stream!.WriteTo(fileStream);
                }
            }

            SetPermission(containerPath);

            return volumeMounts;
        }

        private ContainerVolumeMount SetupIntermediateVolume()
        {
            if (Event.TaskPluginArguments.ContainsKey(Keys.WorkingDirectory))
            {
                var containerPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageContainerPath], Event.ExecutionId, "temp");
                var hostPath = Path.Combine(_hostTemporaryStoragePath, Event.ExecutionId, "temp");

                Directory.CreateDirectory(containerPath);

                var volumeMount = new ContainerVolumeMount(Event.IntermediateStorage, Event.TaskPluginArguments[Keys.WorkingDirectory], hostPath, containerPath);
                _logger.IntermediateVolumeMountAdded(hostPath, containerPath);
                SetPermission(containerPath);
                return volumeMount;
            }

            return default!;
        }

        private List<ContainerVolumeMount> SetupOutputs()
        {
            var volumeMounts = new List<ContainerVolumeMount>();

            if (Event.Outputs.Count == 0)
            {
                _logger.NoOutputVolumesConfigured();
                return volumeMounts;
            }

            // Container Path of the Output Directory.
            var containerRootPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageContainerPath], Event.ExecutionId, "outputs");

            // Host Path of the Output Directory.
            var hostRootPath = Path.Combine(_hostTemporaryStoragePath, Event.ExecutionId, "outputs");

            foreach (var output in Event.Outputs)
            {
                var hostPath = Path.Combine(hostRootPath, output.Name);
                var containerPath = Path.Combine(containerRootPath, output.Name);
                Directory.CreateDirectory(containerPath);

                volumeMounts.Add(new ContainerVolumeMount(output, Event.TaskPluginArguments[output.Name], hostPath, containerPath));
                _logger.OutputVolumeMountAdded(hostPath, containerPath);
            }

            SetPermission(containerRootPath);

            return volumeMounts;
        }

        private void SetPermission(string path)
        {
            if (Event.TaskPluginArguments.ContainsKey(Keys.User))
            {
                if (!System.OperatingSystem.IsWindows())
                {
                    var process = Process.Start("chown", $"-R {Event.TaskPluginArguments[Keys.User]} {path}");
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        _logger.ErrorSettingDirectoryPermission(path, Event.TaskPluginArguments[Keys.User]);
                        throw new SetPermissionException($"chown command exited with code {process.ExitCode}");
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!DisposedValue && disposing)
            {
                _scope.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
