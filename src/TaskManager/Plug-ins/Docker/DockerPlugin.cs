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

using Ardalis.GuardClauses;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Docker.Logging;

namespace Monai.Deploy.WorkflowManager.TaskManager.Docker
{
    public class DockerPlugin : TaskPluginBase
    {

        private readonly TimeSpan _containerTimeout = TimeSpan.FromMinutes(5);
        private readonly DockerClient _dockerClient;
        private readonly ILogger<DockerPlugin> _logger;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IServiceScope _scope;

        public DockerPlugin(
            ILogger<DockerPlugin> logger,
            IOptions<WorkflowManagerOptions> options,
            IServiceScopeFactory serviceScopeFactory,
            TaskDispatchEvent taskDispatchEvent)
            : base(taskDispatchEvent)
        {
            Guard.Against.Null(serviceScopeFactory, nameof(serviceScopeFactory));

            _dockerClient = new DockerClientConfiguration(new Uri(Event.TaskPluginArguments[Keys.BaseUrl])).CreateClient();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _scope = serviceScopeFactory.CreateScope() ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            ValidateEvent();
            Initialize();
        }

        private void Initialize()
        {
            _logger.Initialized(Event.TaskPluginArguments[Keys.BaseUrl]);
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
        }


        public override async Task<ExecutionStatus> ExecuteTask(CancellationToken cancellationToken = default)
        {
            var inputVolumeMounts = await SetupInputs(cancellationToken);
            var intermediateVolumeMount = SetupIntermediateVolume();
            var outputVolumeMounts = SetupOutputs();

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
                var response = await _dockerClient.Containers.CreateContainerAsync(parameters);
                containerId = response.ID;
                _logger.CreatedContainer(containerId);

                _ = await _dockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
                _logger.StartedContainer(containerId);
            }
            catch (Exception exception)
            {
                _logger.ErrorDeployingContainer(exception);
                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError, Errors = exception.Message };
            }

            var monitoringTask = Task.Run(async () =>
            {
                var waitingTime = TimeSpan.FromSeconds(1);
                var pollingPeriod = TimeSpan.FromSeconds(1);

                while (waitingTime < _containerTimeout)
                {
                    var response = await _dockerClient.Containers.InspectContainerAsync(containerId);
                    await Task.Delay(pollingPeriod);
                    waitingTime += pollingPeriod;

                    if (!string.IsNullOrEmpty(response.State.FinishedAt))
                    {
                        // TODO: Execute Callback
                        var message = new JsonMessage<TaskCallbackEvent>(new TaskCallbackEvent
                        {
                            CorrelationId = Event.CorrelationId,
                            ExecutionId = Event.ExecutionId,
                            Identity = containerId,
                            Outputs = Event.Outputs ?? new List<Messaging.Common.Storage>(),
                            TaskId = Event.TaskId,
                            WorkflowInstanceId = Event.WorkflowInstanceId,
                        }, applicationId: Strings.ApplicationId, correlationId: Event.CorrelationId);

                        var messageBrokerPublisherService = _scope.ServiceProvider.GetService<IMessageBrokerPublisherService>() ?? throw new ServiceNotFoundException(nameof(IMessageBrokerPublisherService));
                        await messageBrokerPublisherService.Publish(_options.Value.Messaging.Topics.TaskCallbackRequest, message.ToMessage());
                        break;
                    }
                }
            });

            return new ExecutionStatus() { Status = TaskExecutionStatus.Accepted };
        }


        public override async Task<ExecutionStatus> GetStatus(string identity, CancellationToken cancellationToken = default)
        {
            Guard.Against.NullOrWhiteSpace(identity, nameof(identity));

            try
            {
                var response = await _dockerClient.Containers.InspectContainerAsync(identity);

                if (response == null)
                {
                    throw new InvalidOperationException($"Unable to obtain status for container {identity}");
                }

                var status = new ExecutionStatus();
                if (response.State.Running)
                {
                    status.Status = TaskExecutionStatus.Accepted;
                }
                else if (response.State.OOMKilled)
                {
                    status.Status = TaskExecutionStatus.Failed;
                    status.Errors = response.State.Error;
                    status.FailureReason = FailureReason.ExternalServiceError;
                }
                else if (response.State.Dead)
                {
                    status.Status = TaskExecutionStatus.Failed;
                    status.Errors = response.State.Error;
                    status.FailureReason = FailureReason.ExternalServiceError;
                }
                else if (!string.IsNullOrEmpty(response.State.FinishedAt))
                {
                    status.Status = TaskExecutionStatus.Succeeded;
                }

                return status;
            }
            catch (Exception ex)
            {
                _logger.ErrorGettingStatusFromDocker(identity, ex);
                return new ExecutionStatus
                {
                    Status = TaskExecutionStatus.Failed,
                    FailureReason = FailureReason.PluginError,
                    Errors = ex.Message
                };
            }
        }

        public override async Task HandleTimeout(string identity)
        {
            await _dockerClient.Containers.KillContainerAsync(identity, new ContainerKillParameters());
            _logger.TerminatedContainer(identity);
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
                if (key.StartsWith(Keys.EnvironmentVariableKeyPrefix, false, System.Globalization.CultureInfo.InvariantCulture))
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
                }
            };

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

            var storageService = _scope.ServiceProvider.GetRequiredService<IStorageService>();

            // Container Path of the Input Directory.
            var containerPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageContainerPath], Event.ExecutionId, "inputs");
            // Host Path of the Input Directory.
            var hostPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageHostPath], Event.ExecutionId, "inputs");


            foreach (var input in Event.Inputs)
            {
                var objects = await storageService.ListObjectsAsync(input.Bucket, input.RelativeRootPath, true, cancellationToken);
                if (objects.Count == 0)
                {
                    _logger.NoFilesFoundIn(input.Bucket, input.RelativeRootPath);
                    continue;
                }

                var inputDirName = Guid.NewGuid().ToString();

                // Host Path of the Directory for this Input File.
                // <host-path>/id/
                var inputHostDirRoot = Path.Combine(hostPath, inputDirName);
                var inputContainerDirRoot = Path.Combine(containerPath, inputDirName);

                // eg: /monai/input
                var volumeMount = new ContainerVolumeMount(input.Name, inputHostDirRoot);
                volumeMounts.Add(volumeMount);

                // For each file, download from bucket and store in Task Manager Container.
                foreach (var obj in objects)
                {
                    // Task Manager Container Path of the Input File.
                    var filePath = Path.Combine(inputContainerDirRoot, obj.FilePath.Replace(input.RelativeRootPath, "").TrimStart('/'));

                    // Task Manager Container Path of the Directory for this Input File.
                    var fileDirectory = Path.GetDirectoryName(filePath);
                    Directory.CreateDirectory(fileDirectory);

                    _logger.DownloadingArtifactFromStorageService(obj.Filename, filePath);
                    using var stream = await storageService.GetObjectAsync(input.Bucket, obj.FilePath, cancellationToken) as MemoryStream;
                    using var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
                    stream!.WriteTo(fileStream);
                }
            }

            return volumeMounts;
        }

        private ContainerVolumeMount SetupIntermediateVolume()
        {
            if (Event.TaskPluginArguments.ContainsKey(Keys.WorkingDirectory))
            {
                var containerPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageContainerPath], Event.ExecutionId, "temp");
                var hostPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageHostPath], Event.ExecutionId, "temp");

                Directory.CreateDirectory(containerPath);

                return new ContainerVolumeMount(Event.TaskPluginArguments[Keys.WorkingDirectory], hostPath);
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
            var containerPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageContainerPath], Event.ExecutionId, "outputs");

            // Host Path of the Output Directory.
            var hostPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageHostPath], Event.ExecutionId, "outputs");

            foreach (var output in Event.Outputs)
            {
                var outputDirName = Guid.NewGuid();

                var outputHostPath = Path.Combine(hostPath, outputDirName.ToString());

                var outputContainerPath = Path.Combine(containerPath, outputDirName.ToString());
                Directory.CreateDirectory(outputContainerPath);

                volumeMounts.Add(new ContainerVolumeMount(output.Name, outputHostPath));
            }

            return volumeMounts;
        }
    }

    public class ContainerVolumeMount
    {
        public ContainerVolumeMount(string containerPath, string hostPath)
        {
            if (string.IsNullOrEmpty(containerPath))
            {
                throw new ArgumentNullException(nameof(containerPath));
            }

            if (string.IsNullOrEmpty(hostPath))
            {
                throw new ArgumentNullException(nameof(hostPath));
            }

            ContainerPath = containerPath;
            HostPath = hostPath;
        }

        public string ContainerPath { get; }
        public string HostPath { get; }
    }
}
