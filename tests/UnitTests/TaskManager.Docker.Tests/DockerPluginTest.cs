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

using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.SharedTest;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Docker;
using Moq;
using Credentials = Monai.Deploy.Messaging.Common.Credentials;

namespace TaskManager.Docker.Tests
{
    public class DockerPluginTest
    {
        private readonly Mock<ILogger<DockerPlugin>> _logger;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
        private readonly Mock<IServiceScope> _serviceScope;

        private readonly Mock<IDockerClientFactory> _dockerClientFactory;
        private readonly Mock<IDockerClient> _dockerClient;
        private readonly Mock<IStorageService> _storageService;
        private readonly Mock<IContainerStatusMonitor> _containerStatusMonitor;

        public DockerPluginTest()
        {
            _logger = new Mock<ILogger<DockerPlugin>>();
            _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            _serviceScope = new Mock<IServiceScope>();
            _dockerClientFactory = new Mock<IDockerClientFactory>();
            _dockerClient = new Mock<IDockerClient>();
            _storageService = new Mock<IStorageService>();
            _containerStatusMonitor = new Mock<IContainerStatusMonitor>();

            _serviceScopeFactory.Setup(p => p.CreateScope()).Returns(_serviceScope.Object);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IDockerClientFactory)))
                .Returns(_dockerClientFactory.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IStorageService)))
                .Returns(_storageService.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IContainerStatusMonitor)))
                .Returns(_containerStatusMonitor.Object);

            _serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
            _dockerClientFactory.Setup(p => p.CreateClient(It.IsAny<Uri>())).Returns(_dockerClient.Object);

            _logger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        }

        [Fact(DisplayName = "Throws when missing required plug-in arguments")]
        public void GivenDockerPlugIn_WhenInitializedWIthMissingPlugInArguments_ExpectToThrow()
        {
            var message = GenerateTaskDispatchEvent();
            Assert.Throws<InvalidTaskException>(() => new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message));

            foreach (var key in Keys.RequiredParameters.Take(Keys.RequiredParameters.Count - 1))
            {
                message.TaskPluginArguments.Add(key, Guid.NewGuid().ToString());
                Assert.Throws<InvalidTaskException>(() => new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message));
            }
            message.TaskPluginArguments[Keys.RequiredParameters[Keys.RequiredParameters.Count - 1]] = Guid.NewGuid().ToString();
            Assert.Throws<InvalidTaskException>(() => new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message));

            message.TaskPluginArguments[Keys.BaseUrl] = "/api";
            Assert.Throws<InvalidTaskException>(() => new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message));
        }

        [Fact(DisplayName = "Initializes values")]
        public void GivenDockerPlugIn_WhenInitialized_ExpectValuesToBeInitialized()
        {
            var message = GenerateTaskDispatchEventWithValidArguments();

            _ = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);
            _logger.VerifyLogging($"Docker plugin initialized: base URL=http://api-endpoint/, timeout=100 minutes.", LogLevel.Information, Times.Once());
        }

        [Fact(DisplayName = "ExecuteTask - when docker service is unavilable returns failure status")]
        public async Task ExecuteTask_WhenDockerServiceIsUnavailable_ExpectFailureStatus()
        {
            var payloadFiles = new List<VirtualFileInfo>()
            {
                new VirtualFileInfo( "file.dcm",  "path/to/file.dcm", "etag", 1000)
            };
            _dockerClient.Setup(p => p.Images.CreateImageAsync(
                It.IsAny<ImagesCreateParameters>(),
                It.IsAny<AuthConfig>(),
                It.IsAny<IProgress<JSONMessage>>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("error"));
            _storageService.Setup(p => p.ListObjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(payloadFiles);
            _storageService.Setup(p => p.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes("hello")));

            var message = GenerateTaskDispatchEventWithValidArguments();

            var runner = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);
            var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            Assert.Equal(TaskExecutionStatus.Failed, result.Status);
            Assert.Equal(FailureReason.PluginError, result.FailureReason);
            Assert.Equal("error", result.Errors);

            runner.Dispose();
        }

        [Fact(DisplayName = "ExecuteTask - when storage service is unavailable returns failure status")]
        public async Task ExecuteTask_WhenStorageServiceIsUnavailable_ExpectFailureStatus()
        {
            _dockerClient.Setup(p => p.Images.CreateImageAsync(
                It.IsAny<ImagesCreateParameters>(),
                It.IsAny<AuthConfig>(),
                It.IsAny<IProgress<JSONMessage>>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("error"));
            _storageService.Setup(p => p.ListObjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("error"));

            var message = GenerateTaskDispatchEventWithValidArguments();

            var runner = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);
            var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            Assert.Equal(TaskExecutionStatus.Failed, result.Status);
            Assert.Equal(FailureReason.PluginError, result.FailureReason);
            Assert.Equal("error", result.Errors);

            runner.Dispose();
        }

        [Fact(DisplayName = "ExecuteTask - when unable to create container return failure status")]
        public async Task ExecuteTask_WhenFailedToCreateContainer_ExpectFailureStatus()
        {
            var payloadFiles = new List<VirtualFileInfo>()
            {
                new VirtualFileInfo( "file.dcm",  "path/to/file.dcm", "etag", 1000)
            };
            var contianerId = Guid.NewGuid().ToString();

            _dockerClient.Setup(p => p.Images.CreateImageAsync(
                It.IsAny<ImagesCreateParameters>(),
                It.IsAny<AuthConfig>(),
                It.IsAny<IProgress<JSONMessage>>(),
                It.IsAny<CancellationToken>()));
            _dockerClient.Setup(p => p.Containers.CreateContainerAsync(
                It.IsAny<CreateContainerParameters>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("error"));

            _storageService.Setup(p => p.ListObjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(payloadFiles);
            _storageService.Setup(p => p.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes("hello")));

            var message = GenerateTaskDispatchEventWithValidArguments();

            var runner = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);
            var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            Assert.Equal(TaskExecutionStatus.Failed, result.Status);
            Assert.Equal(FailureReason.PluginError, result.FailureReason);
            Assert.Equal("error", result.Errors);
            runner.Dispose();
        }

        [Fact(DisplayName = "ExecuteTask - when unable to monitor container expect task to be accepted")]
        public async Task ExecuteTask_WhenFailedToMonitorContainer_ExpectTaskToBeAccepted()
        {
            var payloadFiles = new List<VirtualFileInfo>()
            {
                new VirtualFileInfo( "file.dcm",  "path/to/file.dcm", "etag", 1000)
            };
            var contianerId = Guid.NewGuid().ToString();

            _dockerClient.Setup(p => p.Images.CreateImageAsync(
                It.IsAny<ImagesCreateParameters>(),
                It.IsAny<AuthConfig>(),
                It.IsAny<IProgress<JSONMessage>>(),
                It.IsAny<CancellationToken>()));
            _dockerClient.Setup(p => p.Containers.CreateContainerAsync(
                It.IsAny<CreateContainerParameters>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateContainerResponse { ID = contianerId, Warnings = new List<string>() { "warning" } });
            _containerStatusMonitor.Setup(p => p.Start(
                It.IsAny<TaskDispatchEvent>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<string>(),
                It.IsAny<ContainerVolumeMount>(),
                It.IsAny<IReadOnlyList<ContainerVolumeMount>>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("error"));
            _storageService.Setup(p => p.ListObjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(payloadFiles);
            _storageService.Setup(p => p.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes("hello")));

            var message = GenerateTaskDispatchEventWithValidArguments();

            var runner = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);
            var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
            Assert.Equal(FailureReason.None, result.FailureReason);
            Assert.Empty(result.Errors);
            runner.Dispose();
        }

        [Fact(DisplayName = "ExecuteTask - do not pull the image when the specified image exists")]
        public async Task ExecuteTask_WhenImageExists_ExpectNotToPull()
        {
            var payloadFiles = new List<VirtualFileInfo>()
            {
                new VirtualFileInfo( "file.dcm",  "path/to/file.dcm", "etag", 1000)
            };
            var contianerId = Guid.NewGuid().ToString();

            _dockerClient.Setup(p => p.Images.CreateImageAsync(
                It.IsAny<ImagesCreateParameters>(),
                It.IsAny<AuthConfig>(),
                It.IsAny<IProgress<JSONMessage>>(),
                It.IsAny<CancellationToken>()));
            _dockerClient.Setup(p => p.Images.ListImagesAsync(
                It.IsAny<ImagesListParameters>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ImagesListResponse>() { new ImagesListResponse() });
            _dockerClient.Setup(p => p.Containers.CreateContainerAsync(
                It.IsAny<CreateContainerParameters>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateContainerResponse { ID = contianerId, Warnings = new List<string>() { "warning" } });

            _storageService.Setup(p => p.ListObjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(payloadFiles);
            _storageService.Setup(p => p.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes("hello")));

            var message = GenerateTaskDispatchEventWithValidArguments();

            var runner = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);
            var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            _dockerClient.Verify(p => p.Images.CreateImageAsync(
                It.IsAny<ImagesCreateParameters>(),
                It.IsAny<AuthConfig>(),
                It.IsAny<IProgress<JSONMessage>>(),
                It.IsAny<CancellationToken>()), Times.Never());
            _dockerClient.Verify(p => p.Images.ListImagesAsync(
                It.IsAny<ImagesListParameters>(),
                It.IsAny<CancellationToken>()), Times.Once());
            runner.Dispose();
        }

        [Fact(DisplayName = "ExecuteTask - pull the image when force by the user even the specified image exists")]
        public async Task ExecuteTask_WhenAlwaysPullIsSet_ExpectToPullEvenWhenImageExists()
        {
            var payloadFiles = new List<VirtualFileInfo>()
            {
                new VirtualFileInfo( "file.dcm",  "path/to/file.dcm", "etag", 1000)
            };
            var contianerId = Guid.NewGuid().ToString();

            _dockerClient.Setup(p => p.Images.CreateImageAsync(
                It.IsAny<ImagesCreateParameters>(),
                It.IsAny<AuthConfig>(),
                It.IsAny<IProgress<JSONMessage>>(),
                It.IsAny<CancellationToken>()));
            _dockerClient.Setup(p => p.Images.ListImagesAsync(
                It.IsAny<ImagesListParameters>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ImagesListResponse>() { new ImagesListResponse() });
            _dockerClient.Setup(p => p.Containers.CreateContainerAsync(
                It.IsAny<CreateContainerParameters>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateContainerResponse { ID = contianerId, Warnings = new List<string>() { "warning" } });

            _storageService.Setup(p => p.ListObjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(payloadFiles);
            _storageService.Setup(p => p.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes("hello")));

            var message = GenerateTaskDispatchEventWithValidArguments();
            message.TaskPluginArguments.Add(Keys.AlwaysPull, bool.TrueString);

            var runner = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);
            var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            _dockerClient.Verify(p => p.Images.CreateImageAsync(
                It.IsAny<ImagesCreateParameters>(),
                It.IsAny<AuthConfig>(),
                It.IsAny<IProgress<JSONMessage>>(),
                It.IsAny<CancellationToken>()), Times.Once());
            _dockerClient.Verify(p => p.Images.ListImagesAsync(
                It.IsAny<ImagesListParameters>(),
                It.IsAny<CancellationToken>()), Times.Never());
            runner.Dispose();
        }

        [Fact(DisplayName = "ExecuteTask - when called with a valid event expect task to be accepted and monitored in the background")]
        public async Task ExecuteTask_WhenCalledWithValidEvent_ExpectTaskToBeAcceptedAndMonitored()
        {
            var payloadFiles = new List<VirtualFileInfo>()
            {
                new VirtualFileInfo( "file.dcm",  "path/to/file.dcm", "etag", 1000)
            };
            var contianerId = Guid.NewGuid().ToString();

            _dockerClient.Setup(p => p.Images.CreateImageAsync(
                It.IsAny<ImagesCreateParameters>(),
                It.IsAny<AuthConfig>(),
                It.IsAny<IProgress<JSONMessage>>(),
                It.IsAny<CancellationToken>()));
            _dockerClient.Setup(p => p.Containers.CreateContainerAsync(
                It.IsAny<CreateContainerParameters>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateContainerResponse { ID = contianerId, Warnings = new List<string>() { "warning" } });

            _storageService.Setup(p => p.ListObjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(payloadFiles);
            _storageService.Setup(p => p.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes("hello")));

            var message = GenerateTaskDispatchEventWithValidArguments();

            var runner = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);
            var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
            Assert.Equal(FailureReason.None, result.FailureReason);
            Assert.Equal(String.Empty, result.Errors);

            runner.Dispose();
        }

        [Fact(DisplayName = "GetStatus - when contianer status is unknown expect failure status")]
        public async Task GetStatus_WhenContainerStatusIsUnknown_ExpectFalureStatus()
        {
            _dockerClient.Setup(p => p.Containers.InspectContainerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new ContainerInspectResponse
                    {
                        State = new ContainerState
                        {
                            Status = Strings.DockerStatusPaused,
                            FinishedAt = DateTime.MinValue.ToString("s"),
                            ExitCode = 100
                        }
                    });

            var message = GenerateTaskDispatchEventWithValidArguments();

            var runner = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);
            var result = await runner.GetStatus("identity", new TaskCallbackEvent(), CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            Assert.Equal(TaskExecutionStatus.Failed, result.Status);
            Assert.Equal(FailureReason.Unknown, result.FailureReason);
            Assert.Equal($"Exit code=100. Status=paused.", result.Errors);

            _dockerClient.Verify(p => p.Containers.InspectContainerAsync(
                It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        }

        [Fact(DisplayName = "GetStatus - when contianer is killed or dead expect failure status")]
        public async Task GetStatus_WhenContainerIsKilledOrDead_ExpectFalureStatus()
        {
            _dockerClient.Setup(p => p.Containers.InspectContainerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new ContainerInspectResponse
                    {
                        State = new ContainerState
                        {
                            OOMKilled = true,
                            FinishedAt = DateTime.MinValue.ToString("s"),
                            ExitCode = 100
                        }
                    });

            var message = GenerateTaskDispatchEventWithValidArguments();

            var runner = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);
            var result = await runner.GetStatus("identity", new TaskCallbackEvent(), CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            Assert.Equal(TaskExecutionStatus.Failed, result.Status);
            Assert.Equal(FailureReason.ExternalServiceError, result.FailureReason);
            Assert.Equal($"Exit code=100", result.Errors);

            _dockerClient.Verify(p => p.Containers.InspectContainerAsync(
                It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        }

        [Fact(DisplayName = "GetStatus - when Docker is unavailable expect failure status")]
        public async Task GetStatus_WhenDockerIsDown_ExpectFalureStatus()
        {
            _dockerClient.Setup(p => p.Containers.InspectContainerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("error"));

            var message = GenerateTaskDispatchEventWithValidArguments();

            var runner = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);
            var result = await runner.GetStatus("identity", new TaskCallbackEvent(), CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            Assert.Equal(TaskExecutionStatus.Failed, result.Status);
            Assert.Equal(FailureReason.ExternalServiceError, result.FailureReason);
            Assert.Equal($"error", result.Errors);

            _dockerClient.Verify(p => p.Containers.InspectContainerAsync(
                It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        }

        [Fact(DisplayName = "GetStatus - Waits until Succeeded Phase")]
        public async Task GetStatus_WhenCalled_ExpectToWaitUntilFinalStatuses()
        {
            var tryCount = 0;
            _dockerClient.Setup(p => p.Containers.InspectContainerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string containerId, CancellationToken cancellationToken) =>
                {
                    if (tryCount++ < 2)
                    {
                        return new ContainerInspectResponse
                        {
                            State = new ContainerState
                            {
                                Status = Strings.DockerStatusRunning,
                                FinishedAt = DateTime.MinValue.ToString("s")
                            }
                        };
                    }

                    return new ContainerInspectResponse
                    {
                        State = new ContainerState
                        {
                            Status = Strings.DockerStatusExited,
                            FinishedAt = DateTime.MinValue.ToString("s")
                        }
                    };
                });

            var message = GenerateTaskDispatchEventWithValidArguments();

            var runner = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);
            var result = await runner.GetStatus("identity", new TaskCallbackEvent(), CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            Assert.Equal(TaskExecutionStatus.Succeeded, result.Status);
            Assert.Equal(FailureReason.None, result.FailureReason);
            Assert.Empty(result.Errors);

            _dockerClient.Verify(p => p.Containers.InspectContainerAsync(
                It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Fact(DisplayName = "HandleTimeout - when called expecte to terminate container")]
        public async Task HandleTimeout_WhenCalled_ExpectToTerminateContainer()
        {
            _dockerClient.Setup(p => p.Containers.KillContainerAsync(It.IsAny<string>(), It.IsAny<ContainerKillParameters>(), It.IsAny<CancellationToken>()));

            var message = GenerateTaskDispatchEventWithValidArguments();

            var runner = new DockerPlugin(_serviceScopeFactory.Object, _logger.Object, message);

            var exception = await Record.ExceptionAsync(async () =>
            {
                await runner.HandleTimeout("identity").ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            });

            Assert.Null(exception);
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
