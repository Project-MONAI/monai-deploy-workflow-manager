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
        public const string ApplicationId = "Docker";

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
            _dockerClient = new DockerClientConfiguration(new Uri(Event.TaskPluginArguments[Keys.BaseUrl])).CreateClient();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _scope = serviceScopeFactory.CreateScope() ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            ValidateEvent();
            Initialize();
        }

        public override async Task<ExecutionStatus> ExecuteTask(CancellationToken cancellationToken = default)
        {
            var inputVolumeMounts = await SetupInputs(cancellationToken);
            var outputVolumeMounts = SetupOutputs();

            CreateContainerParameters parameters;
            try
            {
                parameters = BuildContainerSpecification(inputVolumeMounts, outputVolumeMounts);
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
                        }, applicationId: ApplicationId, correlationId: Event.CorrelationId);

                        var messageBrokerPublisherService = _scope.ServiceProvider.GetService<IMessageBrokerPublisherService>() ?? throw new ServiceNotFoundException(nameof(IMessageBrokerPublisherService));
                        messageBrokerPublisherService.Publish(_options.Value.Messaging.Topics.TaskCallbackRequest, message);

                        return;
                    }
                }
            });

            var status = new ExecutionStatus() { Status = TaskExecutionStatus.Accepted };
            return status;
        }

        public override async Task<ExecutionStatus> GetStatus(string identity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(identity))
            {
                throw new ArgumentException($"'{nameof(identity)}' cannot be null or empty.", nameof(identity));
            }

            var response = await _dockerClient.Containers.InspectContainerAsync(identity);
            if (response == null)
                throw new InvalidOperationException($"Unable to obtain status returned for container {identity}");

            var status = new ExecutionStatus();
            if (response.State.Running)
            {
                status.Status = TaskExecutionStatus.Accepted;
            }
            else if (response.State.OOMKilled)
            {
                status.Status = TaskExecutionStatus.Failed;
                status.Errors = response.State.Error;
            }
            else if (response.State.Dead)
            {
                status.Status = TaskExecutionStatus.Failed;
                status.Errors = response.State.Error;
            }
            else if (!string.IsNullOrEmpty(response.State.FinishedAt))
            {
                status.Status = TaskExecutionStatus.Succeeded;
            }

            return status;
        }

        public override async Task HandleTimeout(string identity)
        {
            await _dockerClient.Containers.KillContainerAsync(identity, new ContainerKillParameters());
            _logger.TerminatedContainer(identity);
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

        private CreateContainerParameters BuildContainerSpecification(IList<ContainerVolumeMount> inputs, IList<ContainerVolumeMount> outputs)
        {
            var commandString = Event.TaskPluginArguments[Keys.Command];
            var command = commandString.Split(',');

            var volumeMounts = new List<Mount>();

            foreach (var input in inputs)
            {
                var mount = new Mount { Type = "bind", ReadOnly = true, Source = input.HostPath, Target = input.ContainerPath };
                volumeMounts.Add(mount);
            }

            foreach (var output in outputs)
            {
                var mount = new Mount { Type = "bind", ReadOnly = false, Source = output.HostPath, Target = output.ContainerPath };
                volumeMounts.Add(mount);
            }

            var parameters = new CreateContainerParameters()
            {
                Name = Event.ExecutionId,
                Cmd = command,
                Image = Event.TaskPluginArguments[Keys.ContainerImage],
                HostConfig = new HostConfig()
                {
                    Mounts = volumeMounts,
                }
            };

            return parameters;
        }

        private async Task<List<ContainerVolumeMount>> SetupInputs(CancellationToken cancellationToken = default)
        {
            var storageService = _scope.ServiceProvider.GetRequiredService<IStorageService>();

            // Task Manager Container Path of the Input Directory.
            var payloadContainerPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageContainerPath], Event.ExecutionId, "inputs");
            Directory.CreateDirectory(payloadContainerPath);

            // Host Path of the Input Directory.
            var hostPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageHostPath], Event.ExecutionId, "inputs");
            Directory.CreateDirectory(hostPath);

            var volumeMounts = new List<ContainerVolumeMount>();

            foreach (var input in Event.Inputs)
            {
                var credentials = new Amazon.SecurityToken.Model.Credentials()
                {
                    AccessKeyId = input.Credentials?.AccessToken,
                    SecretAccessKey = input.Credentials?.AccessKey,
                    SessionToken = input.Credentials?.SessionToken,
                };

                var objects = await storageService.ListObjectsWithCredentialsAsync(input.Bucket, credentials, input.RelativeRootPath, true, cancellationToken);
                var id = Guid.NewGuid();

                // Host Path of the Directory for this Input File.
                // <host-path>/id/
                var inputHostPath = Path.Combine(hostPath, id.ToString());
                Directory.CreateDirectory(inputHostPath);

                // eg: /monai/input
                var volumeMount = new ContainerVolumeMount(input.Name, inputHostPath);
                volumeMounts.Add(volumeMount);

                // For each file, download from bucket and store in Task Manager Container.
                foreach (var obj in objects)
                {
                    // Task Manager Container Path of the Input File.
                    var inputContainerPath = Path.Combine(payloadContainerPath, id.ToString(), obj.Filename.Replace(input.RelativeRootPath, ""));

                    // Task Manager Container Path of the Directory for this Input File.
                    var parentPath = Directory.GetParent(inputContainerPath).Parent.FullName;
                    Directory.CreateDirectory(parentPath);

                    var stream = await storageService.GetObjectWithCredentialsAsync(input.Bucket, obj.Filename, credentials, cancellationToken);
                    var streamWriter = new StreamWriter(inputContainerPath);
                    var bufferSize = 1 * 1024 * 1024;
                    byte[] buffer = new byte[bufferSize];

                    while (stream.Read(buffer, 0, bufferSize) != 0)
                    {
                        streamWriter.Write(buffer);
                    }
                }
            }

            return volumeMounts;
        }

        private List<ContainerVolumeMount> SetupOutputs()
        {
            // Task Manager Container Path of the Output Directory.
            var payloadContainerPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageContainerPath], Event.ExecutionId, "outputs");
            Directory.CreateDirectory(payloadContainerPath);

            // Host Path of the Output Directory.
            var hostPath = Path.Combine(Event.TaskPluginArguments[Keys.TemporaryStorageHostPath], Event.ExecutionId, "outputs");
            Directory.CreateDirectory(hostPath);

            var volumeMounts = new List<ContainerVolumeMount>();

            foreach (var outputs in Event.Outputs)
            {
                var id = Guid.NewGuid();

                // Host Path of the Directory for this Output.
                // <host-path>/id/
                var inputHostPath = Path.Combine(hostPath, id.ToString());
                Directory.CreateDirectory(inputHostPath);

                // eg: /monai/output
                var volumeMount = new ContainerVolumeMount(outputs.Name, inputHostPath);
                volumeMounts.Add(volumeMount);
            }

            return volumeMounts;
        }
    }

    public struct ContainerVolumeMount
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
