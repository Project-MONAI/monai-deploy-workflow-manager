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
using System.IO.Abstractions.TestingHelpers;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.Docker;
using Moq;
using Credentials = Monai.Deploy.Messaging.Common.Credentials;

namespace TaskManager.Docker.Tests
{
    public class ContainerStatusMonitorTest
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
        private readonly Mock<ILogger<ContainerStatusMonitor>> _logger;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly Mock<IServiceScope> _serviceScope;

        private readonly Mock<IDockerClientFactory> _dockerClientFactory;
        private readonly Mock<IDockerClient> _dockerClient;
        private readonly Mock<IStorageService> _storageService;
        private readonly Mock<IContentTypeProvider> _contentTypeProvider;
        private readonly Mock<IMessageBrokerPublisherService> _messageBrokerPublisherService;
        private readonly Mock<IContainerStatusMonitor> _containerStatusMonitor;
        private readonly IFileSystem _fileSystem;

        public ContainerStatusMonitorTest()
        {
            _logger = new Mock<ILogger<ContainerStatusMonitor>>();
            _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            _serviceScope = new Mock<IServiceScope>();
            _options = Options.Create(new WorkflowManagerOptions());
            _options.Value.Messaging.PublisherSettings.Add("endpoint", "1.2.2.3/virtualhost");
            _options.Value.Messaging.PublisherSettings.Add("username", "username");
            _options.Value.Messaging.PublisherSettings.Add("password", "password");
            _options.Value.Messaging.PublisherSettings.Add("exchange", "exchange");
            _options.Value.Messaging.PublisherSettings.Add("virtualHost", "vhost");
            _options.Value.Messaging.Topics.TaskCallbackRequest = "md.tasks.callback";
            _dockerClientFactory = new Mock<IDockerClientFactory>();
            _dockerClient = new Mock<IDockerClient>();
            _storageService = new Mock<IStorageService>();
            _contentTypeProvider = new Mock<IContentTypeProvider>();
            _messageBrokerPublisherService = new Mock<IMessageBrokerPublisherService>();
            _containerStatusMonitor = new Mock<IContainerStatusMonitor>();
            _fileSystem = new MockFileSystem();

            _serviceScopeFactory.Setup(p => p.CreateScope()).Returns(_serviceScope.Object);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IDockerClientFactory)))
                .Returns(_dockerClientFactory.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IStorageService)))
                .Returns(_storageService.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IContentTypeProvider)))
                .Returns(_contentTypeProvider.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IMessageBrokerPublisherService)))
                .Returns(_messageBrokerPublisherService.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IContainerStatusMonitor)))
                .Returns(_containerStatusMonitor.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IFileSystem)))
                .Returns(_fileSystem);

            _serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
            _dockerClientFactory.Setup(p => p.CreateClient(It.IsAny<Uri>())).Returns(_dockerClient.Object);

            _logger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        }

        [Fact(DisplayName = "Start - when called without any artifacts expect to send callback event")]
        public async Task Start_WhenCalledWithoutAnyArtifacts_ExpectToSendCallbackEvent()
        {
            _fileSystem.Directory.CreateDirectory("/taskmanagerpath/working");
            _fileSystem.Directory.CreateDirectory("/taskmanagerpath/output");
            var intermediateVolumeMount = new ContainerVolumeMount(new Storage { Bucket = "bucket", RelativeRootPath = "/svc" }, "/containerpath", "/hostpath", "/taskmanagerpath/working");
            var outputVolumeMounts = new List<ContainerVolumeMount>() { new ContainerVolumeMount(new Storage { Bucket = "bucket", RelativeRootPath = "/svc" }, "/containerpath", "/hostpath", "/taskmanagerpath/output") };
            var monitor = new ContainerStatusMonitor(_serviceScopeFactory.Object, _logger.Object, _fileSystem, _options);
            var taskDispatchEvent = GenerateTaskDispatchEventWithValidArguments();

            _storageService.Setup(p => p.PutObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()));
            _dockerClient.Setup(p => p.Containers.InspectContainerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContainerInspectResponse
                {
                    State = new ContainerState
                    {
                        Status = Strings.DockerStatusExited,
                        FinishedAt = DateTime.MinValue.ToString("s")
                    }
                });

            await monitor.Start(taskDispatchEvent, TimeSpan.FromSeconds(3), "container", intermediateVolumeMount, outputVolumeMounts, CancellationToken.None);

            _storageService.Verify(p => p.PutObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>())
                , Times.Never());
            _messageBrokerPublisherService.Verify(p => p.Publish(It.IsAny<string>(), It.IsAny<Monai.Deploy.Messaging.Messages.Message>()), Times.Once());
        }

        [Fact(DisplayName = "Start - when called expect to upload artifacts and send callback event")]
        public async Task Start_WhenCalled_ExpectToUploadArtifactsAndSendCallbackEvent()
        {
            var files = new List<string>() { "/taskmanagerpath/working/a.json", "/taskmanagerpath/output/b.dcm" };
            CreateFiles(files);
            var intermediateVolumeMount = new ContainerVolumeMount(new Storage { Bucket = "bucket", RelativeRootPath = "/svc" }, "/containerpath", "/hostpath", "/taskmanagerpath/working");
            var outputVolumeMounts = new List<ContainerVolumeMount>() { new ContainerVolumeMount(new Storage { Bucket = "bucket", RelativeRootPath = "/svc" }, "/containerpath", "/hostpath", "/taskmanagerpath/output") };
            var monitor = new ContainerStatusMonitor(_serviceScopeFactory.Object, _logger.Object, _fileSystem, _options);
            var taskDispatchEvent = GenerateTaskDispatchEventWithValidArguments();

            _storageService.Setup(p => p.PutObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()));
            _dockerClient.Setup(p => p.Containers.InspectContainerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContainerInspectResponse
                {
                    State = new ContainerState
                    {
                        Status = Strings.DockerStatusExited,
                        FinishedAt = DateTime.MinValue.ToString("s")
                    }
                });

            await monitor.Start(taskDispatchEvent, TimeSpan.FromSeconds(3), "container", intermediateVolumeMount, outputVolumeMounts, CancellationToken.None);

            _storageService.Verify(p => p.PutObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>())
                , Times.Exactly(2));
            _messageBrokerPublisherService.Verify(p => p.Publish(It.IsAny<string>(), It.IsAny<Monai.Deploy.Messaging.Messages.Message>()), Times.Once());
        }

        private void CreateFiles(List<string> files)
        {
            foreach (var file in files)
            {
                var dir = _fileSystem.Path.GetDirectoryName(file);
#pragma warning disable CS8604 // Possible null reference argument.
                _fileSystem.Directory.CreateDirectory(dir);
#pragma warning restore CS8604 // Possible null reference argument.
                using var stream = _fileSystem.File.CreateText(file);
                stream.WriteLine(file);
            }
        }

        private static TaskDispatchEvent GenerateTaskDispatchEventWithValidArguments()
        {
            Environment.SetEnvironmentVariable(Strings.HostTemporaryStorageEnvironmentVariableName, "storage");
            var message = GenerateTaskDispatchEvent();
            message.TaskPluginArguments[Keys.BaseUrl] = "http://api-endpoint/";
            message.TaskPluginArguments[Keys.Command] = "command";
            message.TaskPluginArguments[Keys.ContainerImage] = "image";
            message.TaskPluginArguments[Keys.EntryPoint] = "entrypoint";
            message.TaskPluginArguments[Keys.TemporaryStorageContainerPath] = "path";
            message.TaskPluginArguments[Keys.TaskTimeoutMinutes] = "100";
            message.TaskPluginArguments[Keys.WorkingDirectory] = "/working-dir";
            message.TaskPluginArguments[$"{Keys.EnvironmentVariableKeyPrefix}MyVariable"] = "MyVariable";
            message.TaskPluginArguments["input-dicom"] = "some-path";
            message.TaskPluginArguments["output"] = "some-path";
            return message;
        }

        private static TaskDispatchEvent GenerateTaskDispatchEvent()
        {
            var message = new TaskDispatchEvent
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ExecutionId = Guid.NewGuid().ToString(),
                TaskPluginType = Guid.NewGuid().ToString(),
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                TaskId = Guid.NewGuid().ToString(),
                IntermediateStorage = new Storage
                {
                    Name = Guid.NewGuid().ToString(),
                    Endpoint = Guid.NewGuid().ToString(),
                    Credentials = new Credentials
                    {
                        AccessKey = Guid.NewGuid().ToString(),
                        AccessToken = Guid.NewGuid().ToString()
                    },
                    Bucket = Guid.NewGuid().ToString(),
                    RelativeRootPath = Guid.NewGuid().ToString(),
                }
            };
            message.Inputs.Add(new Storage
            {
                Name = "input-dicom",
                Endpoint = Guid.NewGuid().ToString(),
                Credentials = new Credentials
                {
                    AccessKey = Guid.NewGuid().ToString(),
                    AccessToken = Guid.NewGuid().ToString()
                },
                Bucket = Guid.NewGuid().ToString(),
                RelativeRootPath = Guid.NewGuid().ToString(),
            });
            message.Outputs.Add(new Storage
            {
                Name = "output",
                Endpoint = Guid.NewGuid().ToString(),
                Credentials = new Credentials
                {
                    AccessKey = Guid.NewGuid().ToString(),
                    AccessToken = Guid.NewGuid().ToString()
                },
                Bucket = Guid.NewGuid().ToString(),
                RelativeRootPath = Guid.NewGuid().ToString(),
            });
            return message;
        }
    }
}
