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
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Storage.API;
using Monai.Deploy.Storage.S3Policy.Policies;
using Monai.Deploy.TaskManager.API;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Shared;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Monai.Deploy.WorkflowManager.TaskManager.Database;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.TaskManager.Tests
{
    public class TaskExecutionStatsTests
    {
        private const string NOT_ARGO = "notArgo";
        private readonly Mock<ILogger<TaskManager>> _logger;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
        private readonly Mock<IServiceScope> _serviceScope;
        private readonly Mock<ITaskExecutionStatsRepository> _executionStatsRepo;
        private readonly Mock<IStorageService> _storageService;
        private readonly Mock<IStorageAdminService> _storageAdminService;
        private readonly Mock<IMessageBrokerPublisherService> _messageBrokerPublisherService;
        private readonly Mock<IMessageBrokerSubscriberService> _messageBrokerSubscriberService;
        private readonly Mock<ITestRunnerCallback> _testRunnerCallback;
        private readonly Mock<ITestMetadataRepositoryCallback> _testMetadataRepositoryCallback;
        private readonly Mock<ITaskDispatchEventService> _taskDispatchEventService;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TaskExecutionStatsTests()
        {
            _logger = new Mock<ILogger<TaskManager>>();
            _options = Options.Create(new WorkflowManagerOptions());
            _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            _serviceScope = new Mock<IServiceScope>();
            _storageService = new Mock<IStorageService>();
            _messageBrokerPublisherService = new Mock<IMessageBrokerPublisherService>();
            _messageBrokerSubscriberService = new Mock<IMessageBrokerSubscriberService>();
            _storageAdminService = new Mock<IStorageAdminService>();
            _executionStatsRepo = new Mock<ITaskExecutionStatsRepository>();
            _testRunnerCallback = new Mock<ITestRunnerCallback>();
            _testMetadataRepositoryCallback = new Mock<ITestMetadataRepositoryCallback>();
            _taskDispatchEventService = new Mock<ITaskDispatchEventService>();
            _cancellationTokenSource = new CancellationTokenSource();

            _serviceScopeFactory.Setup(p => p.CreateScope()).Returns(_serviceScope.Object);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IMessageBrokerPublisherService)))
                .Returns(_messageBrokerPublisherService.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IMessageBrokerSubscriberService)))
                .Returns(_messageBrokerSubscriberService.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(ITestRunnerCallback)))
                .Returns(_testRunnerCallback.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(ITestMetadataRepositoryCallback)))
                .Returns(_testMetadataRepositoryCallback.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IStorageService)))
                .Returns(_storageService.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IStorageAdminService)))
                .Returns(_storageAdminService.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(ITaskDispatchEventService)))
                .Returns(_taskDispatchEventService.Object);
            serviceProvider
                 .Setup(x => x.GetService(typeof(ITaskExecutionStatsRepository)))
                 .Returns(_executionStatsRepo.Object);

            _serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
            _logger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            _options.Value.TaskManager.PluginAssemblyMappings.Add(PluginStrings.Argo, typeof(TestPlugin).AssemblyQualifiedName);
            _options.Value.TaskManager.PluginAssemblyMappings.Add(NOT_ARGO, typeof(TestPlugin).AssemblyQualifiedName);
            _options.Value.TaskManager.MetadataAssemblyMappings.Add(PluginStrings.Argo, typeof(TestMetadataRepository).AssemblyQualifiedName);
            _options.Value.TaskManager.MetadataAssemblyMappings.Add(NOT_ARGO, typeof(TestMetadataRepository).AssemblyQualifiedName);
            _options.Value.Storage.Settings["accessKey"] = "key";
            _options.Value.Storage.Settings["accessToken"] = "token";
        }

        [Fact(DisplayName = "ExecuteTask - sets Execution Stats on start")]
        public async Task ExecuteTask_SetsExecutionStatsOnStart()
        {
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;
            _testRunnerCallback
                .Setup(p => p.GenerateExecuteTaskResult())
                .Returns(new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None });

            var message = GenerateTaskDispatchEvent();
            message.Body.TaskPluginType = PluginStrings.Argo;
            var resetEvent = new CountdownEvent(2);

            _storageAdminService.Setup(a => a.CreateUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<PolicyRequest[]>()
                )).ReturnsAsync(new Amazon.SecurityToken.Model.Credentials()
                {
                    AccessKeyId = "validaccesskeyid",
                    SecretAccessKey = "b",
                });

            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(_options.Value.Messaging.Topics.TaskDispatchRequest, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(false);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Acknowledge(It.IsAny<MessageBase>()))
                .Callback(() => resetEvent.Signal());
            _messageBrokerPublisherService
                .Setup(p => p.Publish(It.IsAny<string>(), It.IsAny<Message>()))
                .Callback(() => resetEvent.Signal());
            _storageService.Setup(p => p.CreateTemporaryCredentialsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Amazon.SecurityToken.Model.Credentials
                {
                    AccessKeyId = Guid.NewGuid().ToString(),
                    SecretAccessKey = Guid.NewGuid().ToString()
                });

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.Wait(5000));

            _executionStatsRepo.Verify(p => p.CreateAsync(It.Is<TaskDispatchEventInfo>(e =>
            e.Event.CorrelationId == message.Body.CorrelationId &&
            e.Event.ExecutionId == message.Body.ExecutionId &&
            e.Event.TaskId == message.Body.TaskId &&
            e.Event.WorkflowInstanceId == message.Body.WorkflowInstanceId
            )), Times.Once);

        }

        [Fact(DisplayName = "Task Manager - metadata used to fill in ExecutionStats")]
        public async Task TaskManager_Metadata_Used_To_Fill_In_ExecutionStats()
        {
            var stats = new Dictionary<string, string>() { { "", "" } };
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;
            _testRunnerCallback
                .Setup(p => p.GenerateExecuteTaskResult())
                .Returns(new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None });
            _testRunnerCallback
                .Setup(p => p.GenerateGetStatusResult())
                .Returns(new ExecutionStatus { Status = TaskExecutionStatus.Succeeded, FailureReason = FailureReason.None, Stats = GetTestExecutionStats() });

            _testMetadataRepositoryCallback
                .Setup(p => p.GenerateRetrieveMetadataResult())
                .Returns(new Dictionary<string, object>());

            var resetEvent = new CountdownEvent(2);

            var taskDispatchEventMessage = GenerateTaskDispatchEvent();
            taskDispatchEventMessage.Body.TaskPluginType = "argo";
            _taskDispatchEventService.Setup(p => p.GetByTaskExecutionIdAsync(It.IsAny<string>()))
               .ReturnsAsync(new API.Models.TaskDispatchEventInfo(taskDispatchEventMessage.Body));
            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(_options.Value.Messaging.Topics.TaskDispatchRequest, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() =>
                    {
                        messageReceivedCallback(CreateMessageReceivedEventArgs(taskDispatchEventMessage));
                    }).ConfigureAwait(false);
                });

            var taskCallbackEventMessage = GenerateTaskCallbackEvent(taskDispatchEventMessage);
            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(_options.Value.Messaging.Topics.TaskCallbackRequest, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    Assert.True(resetEvent.Wait(5000));
                    resetEvent.Reset(2);
                    await Task.Run(() =>
                    {
                        messageReceivedCallback(CreateMessageReceivedCallbackEventArgs(taskCallbackEventMessage));
                    }).ConfigureAwait(false);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Acknowledge(It.IsAny<MessageBase>()))
                .Callback(() =>
                resetEvent.Signal());
            _messageBrokerPublisherService
                .Setup(p => p.Publish(It.IsAny<string>(), It.IsAny<Message>()))
                .Callback(() =>
                resetEvent.Signal());
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() =>
                resetEvent.Signal());
            _storageService.Setup(p => p.CreateTemporaryCredentialsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Amazon.SecurityToken.Model.Credentials
                {
                    AccessKeyId = Guid.NewGuid().ToString(),
                    SecretAccessKey = Guid.NewGuid().ToString()
                });
            _storageAdminService.Setup(a => a.CreateUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<PolicyRequest[]>()
                )).ReturnsAsync(new Amazon.SecurityToken.Model.Credentials()
                {
                    AccessKeyId = "validaccesskeyid",
                    SecretAccessKey = "b",
                });

            _testMetadataRepositoryCallback.Setup(p => p.GenerateRetrieveMetadataResult());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.Wait(10000));

            _executionStatsRepo.Verify(p => p.UpdateExecutionStatsAsync(It.Is<TaskUpdateEvent>(m =>
                m.Status == TaskExecutionStatus.Succeeded &&
                m.ExecutionStats.ContainsKey("finishedAt")

            )), Times.Once);


        }

        private static JsonMessage<TaskCallbackEvent> GenerateTaskCallbackEvent(JsonMessage<TaskDispatchEvent>? taskDispatchEventMessage = null)
        {
            return new JsonMessage<TaskCallbackEvent>(
                            new TaskCallbackEvent
                            {
                                CorrelationId = taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.CorrelationId,
                                ExecutionId = taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.Body.ExecutionId,
                                WorkflowInstanceId = taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.Body.WorkflowInstanceId,
                                TaskId = taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.Body.TaskId,
                                Identity = Guid.NewGuid().ToString(),
                            },
                            Guid.NewGuid().ToString(),
                            taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.CorrelationId,
                            "1");
        }

        private static JsonMessage<TaskDispatchEvent> GenerateTaskDispatchEvent()
        {
            var correlationId = Guid.NewGuid().ToString();
            var message = new JsonMessage<TaskDispatchEvent>(
                            new TaskDispatchEvent
                            {
                                CorrelationId = correlationId,
                                PayloadId = Guid.NewGuid().ToString(),
                                ExecutionId = Guid.NewGuid().ToString(),
                                TaskPluginType = PluginStrings.Argo,
                                WorkflowInstanceId = Guid.NewGuid().ToString(),
                                TaskId = Guid.NewGuid().ToString()
                            },
                            Guid.NewGuid().ToString(),
                            correlationId,
                            "1");
            message.Body.Inputs.Add(new Messaging.Common.Storage
            {
                Name = Guid.NewGuid().ToString(),
                Endpoint = Guid.NewGuid().ToString(),
                Credentials = new Messaging.Common.Credentials
                {
                    AccessKey = Guid.NewGuid().ToString(),
                    AccessToken = Guid.NewGuid().ToString()
                },
                Bucket = Guid.NewGuid().ToString(),
                RelativeRootPath = Guid.NewGuid().ToString(),
            });
            message.Body.IntermediateStorage = new Messaging.Common.Storage
            {
                Name = Guid.NewGuid().ToString(),
                Endpoint = Guid.NewGuid().ToString(),
                Credentials = new Messaging.Common.Credentials
                {
                    AccessKey = Guid.NewGuid().ToString(),
                    AccessToken = Guid.NewGuid().ToString()
                },
                Bucket = Guid.NewGuid().ToString(),
                RelativeRootPath = Guid.NewGuid().ToString(),
            };
            return message;
        }

        private static MessageReceivedEventArgs CreateMessageReceivedEventArgs<T>(JsonMessage<T> message)
        {
            return new MessageReceivedEventArgs(message.ToMessage(), CancellationToken.None);
        }

        private static MessageReceivedEventArgs CreateMessageReceivedCallbackEventArgs(JsonMessage<TaskCallbackEvent> message)
        {
            message.Body.Metadata.Add("fred", "george");
            var args = new MessageReceivedEventArgs(message.ToMessage(), CancellationToken.None);
            return args;
        }

        private Dictionary<string, string> GetTestExecutionStats()
        {
            return new Dictionary<string, string> {
                { "startedAt","2023-03-30T08:32:45+00:00"},
                { "finishedAt","2023-03-30T08:33:00+00:00"},
                { "podStartTime0","2023-03-30T08:33:00+00:00"},
                { "podFinishTime0","2023-03-30T08:36:00+00:00"},
                { "someStepModelName", "{\"startedAt\":\"2023-03-30T08:32:45+00:00\",\"finishedAt\":\"2023-03-30T08:32:49+00:00\",\"boundaryID\":\"md-simple-workflow-mhmzz\",\"displayName\":\"md-workflow-entrypoint\",\"hostNodeName\":\"kind-control-plane\",\"id\":\"md-simple-workflow-mhmzz-2184607586\",\"inputs\":{\"parameters\":[{\"name\":\"message\",\"value\":\"hello argo\"}]},\"name\":\"md-simple-workflow-mhmzz[0].md-workflow-entrypoint\",\"outputs\":{ \"artifacts\":[{ \"name\":\"main-logs\",\"s3\":{ \"key\":\"md-simple-workflow-mhmzz/md-simple-workflow-mhmzz-argosay-2184607586/main.log\"} }],\"exitCode\":\"0\"},\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{ \"cpu\":3,\"memory\":2},\"templateName\":\"argosay\",\"templateScope\":\"local/md-simple-workflow-mhmzz\",\"type\":\"Pod\"}" },
                { "someStepModelName_part1", "{\"startedAt\":\"2023-03-29T08:32:45+00:00\",\"finishedAt\":\"2023-03-31T08:32:49+00:00\",\"boundaryID\":\"md-simple-workflow-mhmzz\",\"displayName\":\"md-workflow-entrypoint\",\"hostNodeName\":\"kind-control-plane\",\"id\":\"md-simple-workflow-mhmzz-2184607586\",\"inputs\":{\"parameters\":[{\"name\":\"message\",\"value\":\"hello argo\"}]},\"name\":\"md-simple-workflow-mhmzz[0].md-workflow-entrypoint\",\"outputs\":{ \"artifacts\":[{ \"name\":\"main-logs\",\"s3\":{ \"key\":\"md-simple-workflow-mhmzz/md-simple-workflow-mhmzz-argosay-2184607586/main.log\"} }],\"exitCode\":\"0\"},\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{ \"cpu\":3,\"memory\":2},\"templateName\":\"argosay\",\"templateScope\":\"local/md-simple-workflow-mhmzz\",\"type\":\"Step\"}" },
                { "someStepModelName_part2", "{\"startedAt\":\"2023-03-30T08:32:55+00:00\",\"finishedAt\":\"2023-03-30T08:32:59+00:00\",\"boundaryID\":\"md-simple-workflow-mhmzz-964148019\",\"displayName\":\"send-message\",\"hostNodeName\":\"kind-control-plane\",\"id\":\"md-simple-workflow-mhmzz-3707092479\",\"inputs\":{\"parameters\":[{\"name\":\"event\",}]},\"name\":\"md-simple-workflow-mhmzz.onExit[0].send-message\",\"outputs\":{\"artifacts\":[{\"archive\":{\"none\":{}},\"name\":\"output\",\"path\":\"/tmp\",\"s3\":{\"accessKeySecret\":{\"key\":\"accessKey\",\"name\":\"argo-task-638157619629820483\"},\"bucket\":\"bucket1\",\"endpoint\":\"minio:9000\",\"insecure\":true,\"key\":\"00000000-1000-0000-0000-000000000000/workflows/e5275aac-7e19-493a-85f2-1921af9f830a/15536613-292b-405a-9d52-83b6c49e1e4c/messaging\",\"secretKeySecret\":{\"key\":\"secretKey\",\"name\":\"argo-task-638157619629820483\"}}},{\"name\":\"main-logs\",\"s3\":{\"key\":\"md-simple-workflow-mhmzz/md-simple-workflow-mhmzz-send-message-3707092479/main.log\"}}],\"exitCode\":\"0\"},\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":1,\"memory\":5},\"templateName\":\"send-message\",\"templateScope\":\"local/md-simple-workflow-mhmzz\",\"type\":\"Pod\"}" }
            };
        }
    }
}
