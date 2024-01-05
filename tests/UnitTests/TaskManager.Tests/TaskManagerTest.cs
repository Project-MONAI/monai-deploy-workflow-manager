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
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.TaskManager.Tests
{
    public interface ITestRunnerCallback
    {
        ExecutionStatus GenerateExecuteTaskResult();

        ExecutionStatus GenerateGetStatusResult();
    }

    public interface ITestMetadataRepositoryCallback
    {
        Dictionary<string, object> GenerateRetrieveMetadataResult();
    }

    internal sealed class TestPlugin : TaskPluginBase
    {
        private readonly ITestRunnerCallback _testRunnerCallback;

        public TestPlugin(
            IServiceScopeFactory serviceScopeFactory,
            TaskDispatchEvent taskDispatchEvent,
            ITestRunnerCallback testRunnerCallback) : base(taskDispatchEvent)
        {
            _ = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _ = taskDispatchEvent ?? throw new ArgumentNullException(nameof(taskDispatchEvent));
            _testRunnerCallback = testRunnerCallback ?? throw new ArgumentNullException(nameof(testRunnerCallback));
        }

        public override Task<ExecutionStatus> ExecuteTask(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_testRunnerCallback.GenerateExecuteTaskResult());
        }

        public override Task<ExecutionStatus> GetStatus(string identity, TaskCallbackEvent taskCallbackEvent, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_testRunnerCallback.GenerateGetStatusResult());
        }

        public override Task HandleTimeout(string identity) => throw new NotImplementedException();
    }

    internal sealed class TestPluginNoTimeout : TaskPluginBase
    {
        private readonly ITestRunnerCallback _testRunnerCallback;

        public TestPluginNoTimeout(
            IServiceScopeFactory serviceScopeFactory,
            TaskDispatchEvent taskDispatchEvent,
            ITestRunnerCallback testRunnerCallback) : base(taskDispatchEvent)
        {
            _ = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _ = taskDispatchEvent ?? throw new ArgumentNullException(nameof(taskDispatchEvent));
            _testRunnerCallback = testRunnerCallback ?? throw new ArgumentNullException(nameof(testRunnerCallback));
        }

        public override Task<ExecutionStatus> ExecuteTask(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_testRunnerCallback.GenerateExecuteTaskResult());
        }

        public override Task<ExecutionStatus> GetStatus(string identity, TaskCallbackEvent taskCallbackEvent, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_testRunnerCallback.GenerateGetStatusResult());
        }

        public override Task HandleTimeout(string identity) => Task.CompletedTask;
    }

    internal sealed class TestMetadataRepository : MetadataRepositoryBase
    {
        private readonly ITestMetadataRepositoryCallback _testMetadataRepositoryCallback;

        public TestMetadataRepository(
            IServiceScopeFactory serviceScopeFactory,
            TaskDispatchEvent taskDispatchEvent,
            TaskCallbackEvent taskCallbackEvent,
            ITestMetadataRepositoryCallback testMetadataRepositoryCallback) : base(taskDispatchEvent, taskCallbackEvent)
        {
            _ = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _ = taskDispatchEvent ?? throw new ArgumentNullException(nameof(taskDispatchEvent));
            _testMetadataRepositoryCallback = testMetadataRepositoryCallback ?? throw new ArgumentNullException(nameof(testMetadataRepositoryCallback));
        }

        public override Task<Dictionary<string, object>> RetrieveMetadata(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_testMetadataRepositoryCallback.GenerateRetrieveMetadataResult())!;
        }
    }

    public class TaskManagerTest
    {
        // ReSharper disable once InconsistentNaming
        private const string NOT_ARGO = "notArgo";

        private readonly Mock<ILogger<TaskManager>> _logger;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
        private readonly Mock<IServiceScope> _serviceScope;
        private readonly Mock<IStorageService> _storageService;
        private readonly Mock<IStorageAdminService> _storageAdminService;
        private readonly Mock<IMessageBrokerPublisherService> _messageBrokerPublisherService;
        private readonly Mock<IMessageBrokerSubscriberService> _messageBrokerSubscriberService;
        private readonly Mock<ITestRunnerCallback> _testRunnerCallback;
        private readonly Mock<ITestMetadataRepositoryCallback> _testMetadataRepositoryCallback;
        private readonly Mock<ITaskDispatchEventService> _taskDispatchEventService;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TaskManagerTest()
        {
            _logger = new Mock<ILogger<TaskManager>>();
            _options = Options.Create(new WorkflowManagerOptions());
            _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            _serviceScope = new Mock<IServiceScope>();
            _storageService = new Mock<IStorageService>();
            _messageBrokerPublisherService = new Mock<IMessageBrokerPublisherService>();
            _messageBrokerSubscriberService = new Mock<IMessageBrokerSubscriberService>();
            _storageAdminService = new Mock<IStorageAdminService>();
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

            _serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
            _logger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            _options.Value.TaskManager.PluginAssemblyMappings.Add(PluginStrings.Argo, typeof(TestPlugin).AssemblyQualifiedName);
            _options.Value.TaskManager.PluginAssemblyMappings.Add(NOT_ARGO, typeof(TestPlugin).AssemblyQualifiedName);
            _options.Value.TaskManager.MetadataAssemblyMappings.Add(PluginStrings.Argo, typeof(TestMetadataRepository).AssemblyQualifiedName);
            _options.Value.TaskManager.MetadataAssemblyMappings.Add(NOT_ARGO, typeof(TestMetadataRepository).AssemblyQualifiedName);
            _options.Value.Storage.Settings["accessKey"] = "key";
            _options.Value.Storage.Settings["accessToken"] = "token";
        }

        [Fact(DisplayName = "Task Manager starts & stops")]
        public async Task TaskManager_StartStop()
        {
            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            Assert.Equal(ServiceStatus.Running, service.Status);

            await service.StopAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Stopped, service.Status);
        }

        [Fact(DisplayName = "Task Manager - TaskDispatchEvent rejects message (no re-queue) on validation failures")]
        public async Task TaskManager_TaskDispatchEvent_ValidationFailure()
        {
            var message = new JsonMessage<TaskDispatchEvent>(
                new TaskDispatchEvent(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                "1");
            var resetEvent = new ManualResetEvent(false);

            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(_options.Value.Messaging.Topics.TaskDispatchRequest, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Set());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.WaitOne(5000));

            _messageBrokerSubscriberService.Verify(p => p.Reject(It.Is<MessageBase>(m => message.MessageId == m.MessageId), It.Is<bool>(b => !b)), Times.Once());
        }

        [Fact(DisplayName = "Task Manager - TaskDispatchEvent rejects and re-queue message when out of resource")]
        public async Task TaskManager_TaskDispatchEvent_OutOfResource()
        {
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 0;
            var message = GenerateTaskDispatchEvent();
            var resetEvent = new ManualResetEvent(false);

            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(_options.Value.Messaging.Topics.TaskDispatchRequest, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.RequeueWithDelay(It.IsAny<MessageBase>()))
                .Callback(() => resetEvent.Set());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.WaitOne(5000));

            _messageBrokerSubscriberService.Verify(p => p.RequeueWithDelay(It.Is<MessageBase>(m => message.MessageId == m.MessageId)), Times.Once());
        }

        // TODO: https://github.com/Project-MONAI/monai-deploy-workflow-manager/issues/102
        //[Fact(DisplayName = "Task Manager - TaskDispatchEvent rejects message (requeue) when failure to generate temporary storage credentials")]
        //public async Task TaskManager_TaskDispatchEvent_FailedToGenerateTemporaryCredentials()
        //{
        //    _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;
        //    var message = GenerateTaskDispatchEvent();
        //    var resetEvent = new ManualResetEvent(false);

        //    _messageBrokerSubscriberService.Setup(
        //        p => p.SubscribeAsync(It.Is<string>(p => p.Equals(_options.Value.Messaging.Topics.TaskDispatchRequest, StringComparison.OrdinalIgnoreCase)),
        //                         It.IsAny<string>(),
        //                         It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
        //                         It.IsAny<ushort>()))
        //        .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
        //        {
        //            await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
        //        });
        //    _messageBrokerSubscriberService
        //        .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
        //        .Callback(() => resetEvent.Set());
        //    _storageService.Setup(p => p.CreateTemporaryCredentialsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
        //        .ThrowsAsync(new Exception("error"));

        //    var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Objec);
        //    await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
        //    Assert.Equal(ServiceStatus.Running, service.Status);

        //    Assert.True(resetEvent.WaitOne(5000));

        //    _messageBrokerSubscriberService.Verify(p => p.Reject(It.Is<MessageBase>(m => message.MessageId == m.MessageId), It.Is<bool>(b => b)), Times.Once());
        //}

        [Fact(DisplayName = "Task Manager - TaskDispatchEvent rejects message (no-requeue) with unsupported runner")]
        public async Task TaskManager_TaskDispatchEvent_UnsupportedRunner()
        {
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;
            var message = GenerateTaskDispatchEvent();
            var resetEvent = new ManualResetEvent(false);

            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(_options.Value.Messaging.Topics.TaskDispatchRequest, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
                });

            _storageAdminService.Setup(a => a.CreateUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<PolicyRequest[]>()
                )).ReturnsAsync(new Amazon.SecurityToken.Model.Credentials()
                {
                    AccessKeyId = "validaccesskeyid",
                    SecretAccessKey = "b",
                });

            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Set());
            _storageService.Setup(p => p.CreateTemporaryCredentialsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Amazon.SecurityToken.Model.Credentials
                {
                    AccessKeyId = Guid.NewGuid().ToString(),
                    SecretAccessKey = Guid.NewGuid().ToString()
                });

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.WaitOne(5000));

            _messageBrokerSubscriberService.Verify(p => p.Reject(It.Is<MessageBase>(m => message.MessageId == m.MessageId), It.Is<bool>(b => !b)), Times.Once());
        }

        [Fact(DisplayName = "Task Manager - TaskDispatchEvent rejects message (no-requeue) on exception executing runner")]
        public async Task TaskManager_TaskDispatchEvent_ExceptionExecutingRunner()
        {
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;
            _testRunnerCallback.Setup(p => p.GenerateExecuteTaskResult()).Throws(new Exception("error"));

            var message = GenerateTaskDispatchEvent();
            message.Body.TaskPluginType = PluginStrings.Argo;
            var resetEvent = new ManualResetEvent(false);

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
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Set());
            _storageService.Setup(p => p.CreateTemporaryCredentialsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Amazon.SecurityToken.Model.Credentials
                {
                    AccessKeyId = Guid.NewGuid().ToString(),
                    SecretAccessKey = Guid.NewGuid().ToString()
                });

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.WaitOne(5000));

            _testRunnerCallback.Verify(p => p.GenerateExecuteTaskResult(), Times.Once());
            _messageBrokerSubscriberService.Verify(p => p.Reject(It.Is<MessageBase>(m => message.MessageId == m.MessageId), It.Is<bool>(b => !b)), Times.Once());
        }

        [Fact(DisplayName = "Task Manager - TaskDispatchEvent rejects message when unable to create user accounts")]
        public async Task TaskManager_TaskDispatchEvent_RejectWhenUnalbeToCreateUserAccounts()
        {
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;
            _testRunnerCallback
                .Setup(p => p.GenerateExecuteTaskResult())
                .Returns(new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None });

            var message = GenerateTaskDispatchEvent();
            message.Body.TaskPluginType = PluginStrings.Argo;
            var resetEvent = new CountdownEvent(2);

#pragma warning disable CS8603 // Possible null reference return.
            _storageAdminService.Setup(a => a.CreateUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<PolicyRequest[]>()
                )).ReturnsAsync(() => null);
#pragma warning restore CS8603 // Possible null reference return.

            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(_options.Value.Messaging.Topics.TaskDispatchRequest, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.RequeueWithDelay(It.IsAny<MessageBase>()))
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
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.Wait(5000));

            _messageBrokerSubscriberService.Verify(p => p.RequeueWithDelay(It.Is<MessageBase>(m => message.MessageId == m.MessageId)), Times.Once());
            _messageBrokerPublisherService.Verify(p => p.Publish(It.Is<string>(m => m == _options.Value.Messaging.Topics.TaskUpdateRequest), It.IsAny<Message>()), Times.Once());
        }

        [Fact(DisplayName = "Task Manager - TaskDispatchEvent executes runner and accepts task")]
        public async Task TaskManager_TaskDispatchEvent_ExecutesRunner()
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
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
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
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.Wait(5000));

            _testRunnerCallback.Verify(p => p.GenerateExecuteTaskResult(), Times.Once());
            _messageBrokerSubscriberService.Verify(p => p.Acknowledge(It.Is<MessageBase>(m => message.MessageId == m.MessageId)), Times.Once());
            _messageBrokerPublisherService.Verify(p => p.Publish(It.Is<string>(m => m == _options.Value.Messaging.Topics.TaskUpdateRequest), It.IsAny<Message>()), Times.Once());
        }

        [Fact(DisplayName = "Task Manager - TaskCallbackEvent rejects message (no re-queue) on validation failures")]
        public async Task TaskManager_TaskCallbackEvent_ValidationFailure()
        {
            var resetEvent = new ManualResetEvent(false);
            var message = new JsonMessage<TaskCallbackEvent>(
                new TaskCallbackEvent(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                "1");

            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(_options.Value.Messaging.Topics.TaskCallbackRequest, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Set());
            _storageService.Setup(p => p.CreateTemporaryCredentialsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Amazon.SecurityToken.Model.Credentials
                {
                    AccessKeyId = Guid.NewGuid().ToString(),
                    SecretAccessKey = Guid.NewGuid().ToString()
                });

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.WaitOne(5000));

            _messageBrokerSubscriberService.Verify(p => p.Reject(It.Is<MessageBase>(m => m.MessageId == message.MessageId), It.Is<bool>(b => !b)), Times.Once());
        }

        [Fact(DisplayName = "Task Manager - TaskCallbackEvent rejects and re-queues message on no matching execution ID")]
        public async Task TaskManager_TaskCallbackEvent_NoMatchingExecutionId()
        {
            var message = GenerateTaskCallbackEvent();
            var resetEvent = new ManualResetEvent(false);

            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(_options.Value.Messaging.Topics.TaskCallbackRequest, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Set());
            _storageService.Setup(p => p.CreateTemporaryCredentialsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Amazon.SecurityToken.Model.Credentials
                {
                    AccessKeyId = Guid.NewGuid().ToString(),
                    SecretAccessKey = Guid.NewGuid().ToString()
                });

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.WaitOne(5000));

            _messageBrokerSubscriberService.Verify(p => p.Reject(It.Is<MessageBase>(m => m.MessageId == message.MessageId), It.Is<bool>(b => !b)), Times.Once());
        }

        [Fact(DisplayName = "Task Manager - TaskCallbackEvent rejects message (no-requeue) on exception executing runner")]
        public async Task TaskManager_TaskCallbackEvent_ExceptionGettingStatus()
        {
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;

            _testRunnerCallback
                .Setup(p => p.GenerateExecuteTaskResult())
                .Returns(new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None });
            _testRunnerCallback
                .Setup(p => p.GenerateGetStatusResult())
                .Throws(new Exception("error"));

            var resetEvent = new CountdownEvent(2);
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Signal());
            _messageBrokerSubscriberService
                .Setup(p => p.Acknowledge(It.IsAny<MessageBase>()))
                .Callback(() => resetEvent.Signal());
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

            var taskDispatchEventMessage = GenerateTaskDispatchEvent();
            taskDispatchEventMessage.Body.TaskPluginType = PluginStrings.Argo;
            _taskDispatchEventService.Setup(p => p.GetByTaskExecutionIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new API.Models.TaskDispatchEventInfo(taskDispatchEventMessage.Body));

            _ = _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(_options.Value.Messaging.Topics.TaskDispatchRequest, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(taskDispatchEventMessage))).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
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
                        messageReceivedCallback(CreateMessageReceivedEventArgs(taskCallbackEventMessage));
                    }).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
                });

            _messageBrokerPublisherService
                .Setup(p => p.Publish(It.IsAny<string>(), It.IsAny<Message>()))
                .Callback(() => resetEvent.Signal());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.Wait(5000));

            _testRunnerCallback.Verify(p => p.GenerateExecuteTaskResult(), Times.Once());
            _testRunnerCallback.Verify(p => p.GenerateGetStatusResult(), Times.Once());
            _messageBrokerSubscriberService.Verify(p => p.Acknowledge(It.Is<MessageBase>(m => m.MessageId == taskDispatchEventMessage.MessageId)), Times.Once());
            _messageBrokerPublisherService.Verify(p => p.Publish(It.Is<string>(m => m == _options.Value.Messaging.Topics.TaskUpdateRequest), It.IsAny<Message>()), Times.Exactly(2));
            _messageBrokerSubscriberService.Verify(p => p.Reject(It.Is<MessageBase>(m => m.MessageId == taskCallbackEventMessage.MessageId), It.Is<bool>(b => !b)), Times.Once());
        }

        [Fact(DisplayName = "Task Manager - TaskCallbackEvent completes workflow")]
        public async Task TaskManager_TaskCallbackEvent_CompletesWorkflow()
        {
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;
            _testRunnerCallback
                .Setup(p => p.GenerateExecuteTaskResult())
                .Returns(new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None });
            _testRunnerCallback
                .Setup(p => p.GenerateGetStatusResult())
                .Returns(new ExecutionStatus { Status = TaskExecutionStatus.Succeeded, FailureReason = FailureReason.None });

            _testMetadataRepositoryCallback
                .Setup(p => p.GenerateRetrieveMetadataResult())
                .Returns(new Dictionary<string, object>()
                {
                    { "key", "value" }
                });

            var resetEvent = new CountdownEvent(2);

            var taskDispatchEventMessage = GenerateTaskDispatchEvent();
            taskDispatchEventMessage.Body.IntermediateStorage = new Messaging.Common.Storage()
            {
                Bucket = "testBucket",
                Endpoint = "testEndpoind",
                Name = "test",
                RelativeRootPath = "/test/path"
            };

            taskDispatchEventMessage.Body.TaskPluginType = PluginStrings.Argo;
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
                        }).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
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
                            messageReceivedCallback(CreateMessageReceivedEventArgs(taskCallbackEventMessage));
                        }).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
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
            _storageAdminService.Setup(a => a.CreateUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<PolicyRequest[]>()
                )).ReturnsAsync(new Amazon.SecurityToken.Model.Credentials()
                {
                    AccessKeyId = "validaccesskeyid",
                    SecretAccessKey = "b",
                });

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.Wait(5000));

            _testRunnerCallback.Verify(p => p.GenerateExecuteTaskResult(), Times.Once());
            _testRunnerCallback.Verify(p => p.GenerateGetStatusResult(), Times.Once());
            _messageBrokerSubscriberService.Verify(p => p.Acknowledge(It.Is<MessageBase>(m => m.MessageId == taskDispatchEventMessage.MessageId)), Times.Once());
            _messageBrokerSubscriberService.Verify(p => p.Acknowledge(It.Is<MessageBase>(m => m.MessageId == taskCallbackEventMessage.MessageId)), Times.Once());
            _messageBrokerPublisherService.Verify(p => p.Publish(It.Is<string>(m => m == _options.Value.Messaging.Topics.TaskUpdateRequest), It.IsAny<Message>()), Times.Exactly(2));
        }

        [Fact(DisplayName = "Task Manager - none argo TaskCallbackEvent completes workflow")]
        public async Task TaskManager_NonArgoTaskCallbackEvent_CompletesWorkflow()
        {
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;
            _testRunnerCallback
                .Setup(p => p.GenerateExecuteTaskResult())
                .Returns(new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None });
            _testRunnerCallback
                .Setup(p => p.GenerateGetStatusResult())
                .Returns(new ExecutionStatus { Status = TaskExecutionStatus.Succeeded, FailureReason = FailureReason.None });

            var resetEvent = new CountdownEvent(2);

            var taskDispatchEventMessage = GenerateTaskDispatchEvent();

            taskDispatchEventMessage.Body.TaskPluginType = NOT_ARGO;
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
                    }).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
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
                        messageReceivedCallback(CreateMessageReceivedEventArgs(taskCallbackEventMessage));
                    }).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
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
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.Wait(5000));

            _testRunnerCallback.Verify(p => p.GenerateExecuteTaskResult(), Times.Once());
            _testRunnerCallback.Verify(p => p.GenerateGetStatusResult(), Times.Once());
            _messageBrokerSubscriberService.Verify(p => p.Acknowledge(It.Is<MessageBase>(m => m.MessageId == taskDispatchEventMessage.MessageId)), Times.Once());
            _messageBrokerSubscriberService.Verify(p => p.Acknowledge(It.Is<MessageBase>(m => m.MessageId == taskCallbackEventMessage.MessageId)), Times.Once());
            _messageBrokerPublisherService.Verify(p => p.Publish(It.Is<string>(m => m == _options.Value.Messaging.Topics.TaskUpdateRequest), It.IsAny<Message>()), Times.Exactly(2));
        }

        [Fact(DisplayName = "Task Manager - TaskCallbackEvent metadata fails and fails workflow")]
        public async Task TaskManager_TaskCallbackEventMetadataFails_FailsWorkflow()
        {
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;
            _testRunnerCallback
                .Setup(p => p.GenerateExecuteTaskResult())
                .Returns(new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None });
            _testRunnerCallback
                .Setup(p => p.GenerateGetStatusResult())
                .Returns(new ExecutionStatus { Status = TaskExecutionStatus.Succeeded, FailureReason = FailureReason.None });

            _testMetadataRepositoryCallback
                .Setup(p => p.GenerateRetrieveMetadataResult())
                .Returns(new Dictionary<string, object>());

            var resetEvent = new CountdownEvent(2);

            var taskDispatchEventMessage = GenerateTaskDispatchEvent();

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
                    }).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
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
                        messageReceivedCallback(CreateMessageReceivedEventArgs(taskCallbackEventMessage));
                    }).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Acknowledge(It.IsAny<MessageBase>()))
                .Callback(() => resetEvent.Signal());
            _messageBrokerPublisherService
                .Setup(p => p.Publish(It.IsAny<string>(), It.IsAny<Message>()))
                .Callback(() => resetEvent.Signal());
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Signal());
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

            _testMetadataRepositoryCallback.Setup(p => p.GenerateRetrieveMetadataResult()).Throws(new Exception());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.Wait(5000));

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
    }
}
