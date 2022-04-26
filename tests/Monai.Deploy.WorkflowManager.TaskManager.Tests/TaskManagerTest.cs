// SPDX-FileCopyrightText: � 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Rest;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.TaskManager.Tests
{
    public interface ITestRunnerCallback
    {
        ExecutionStatus GenerateExecuteTaskResult();

        ExecutionStatus GenerateGetStatusResult();
    }

    internal sealed class TestRunner : RunnerBase
    {
        private readonly ITestRunnerCallback _testRunnerCallback;

        public TestRunner(
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
            return Task.FromResult(_testRunnerCallback.GenerateExecuteTaskResult())!;
        }

        public override Task<ExecutionStatus> GetStatus(string identity, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_testRunnerCallback.GenerateGetStatusResult())!;
        }
    }

    public class TaskManagerTest
    {
        private readonly Mock<ILogger<TaskManager>> _logger;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
        private readonly Mock<IServiceScope> _serviceScope;
        private readonly Mock<IMessageBrokerPublisherService> _messageBrokerPublisherService;
        private readonly Mock<IMessageBrokerSubscriberService> _messageBrokerSubscriberService;
        private readonly Mock<ITestRunnerCallback> _testRunnerCallback;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TaskManagerTest()
        {
            _logger = new Mock<ILogger<TaskManager>>();
            _options = Options.Create(new WorkflowManagerOptions());
            _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            _serviceScope = new Mock<IServiceScope>();
            _messageBrokerPublisherService = new Mock<IMessageBrokerPublisherService>();
            _messageBrokerSubscriberService = new Mock<IMessageBrokerSubscriberService>();
            _testRunnerCallback = new Mock<ITestRunnerCallback>();
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

            _serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            _logger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        }

        [Fact(DisplayName = "Task Manager starts & stops")]
        public async Task TaskManager_StartStop()
        {
            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(ServiceStatus.Running, service.Status);

            await service.StopAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
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
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(TaskManager.TaskDispatchEvent, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(false);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Set());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
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
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(TaskManager.TaskDispatchEvent, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(false);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Set());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.WaitOne(5000));

            _messageBrokerSubscriberService.Verify(p => p.Reject(It.Is<MessageBase>(m => message.MessageId == m.MessageId), It.Is<bool>(b => b)), Times.Once());
        }

        [Fact(DisplayName = "Task Manager - TaskDispatchEvent rejects message (no-requeue) with unsupported runner")]
        public async Task TaskManager_TaskDispatchEvent_UnsupportedRunner()
        {
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;
            var message = GenerateTaskDispatchEvent();
            var resetEvent = new ManualResetEvent(false);

            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(TaskManager.TaskDispatchEvent, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(false);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Set());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
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
            message.Body.TaskAssemblyName = typeof(TestRunner).AssemblyQualifiedName!;
            var resetEvent = new ManualResetEvent(false);

            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(TaskManager.TaskDispatchEvent, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(false);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Set());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.WaitOne(5000));

            _testRunnerCallback.Verify(p => p.GenerateExecuteTaskResult(), Times.Once());
            _messageBrokerSubscriberService.Verify(p => p.Reject(It.Is<MessageBase>(m => message.MessageId == m.MessageId), It.Is<bool>(b => !b)), Times.Once());
        }

        [Fact(DisplayName = "Task Manager - TaskDispatchEvent executes runner and accepts task")]
        public async Task TaskManager_TaskDispatchEvent_ExecutesRunner()
        {
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;
            _testRunnerCallback
                .Setup(p => p.GenerateExecuteTaskResult())
                .Returns(new ExecutionStatus { Status = Messaging.Events.TaskStatus.Accepted, FailureReason = FailureReason.None });

            var message = GenerateTaskDispatchEvent();
            message.Body.TaskAssemblyName = typeof(TestRunner).AssemblyQualifiedName!;
            var resetEvent = new CountdownEvent(2);

            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(TaskManager.TaskDispatchEvent, StringComparison.OrdinalIgnoreCase)),
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

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.Wait(5000));

            _testRunnerCallback.Verify(p => p.GenerateExecuteTaskResult(), Times.Once());
            _messageBrokerSubscriberService.Verify(p => p.Acknowledge(It.Is<MessageBase>(m => message.MessageId == m.MessageId)), Times.Once());
            _messageBrokerPublisherService.Verify(p => p.Publish(It.Is<string>(m => m == TaskManager.TaskUpdateEvent), It.IsAny<Message>()), Times.Once());
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
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(TaskManager.TaskCallbackEvent, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(false);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Set());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
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
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(TaskManager.TaskCallbackEvent, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(message))).ConfigureAwait(false);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Reject(It.IsAny<MessageBase>(), It.IsAny<bool>()))
                .Callback(() => resetEvent.Set());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
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
                .Returns(new ExecutionStatus { Status = Messaging.Events.TaskStatus.Accepted, FailureReason = FailureReason.None });
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

            var taskDispatchEventMessage = GenerateTaskDispatchEvent();
            taskDispatchEventMessage.Body.TaskAssemblyName = typeof(TestRunner).AssemblyQualifiedName!;

            _ = _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(TaskManager.TaskDispatchEvent, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    await Task.Run(() => messageReceivedCallback(CreateMessageReceivedEventArgs(taskDispatchEventMessage))).ConfigureAwait(false);
                });

            var TaskCallbackEventMessage = GenerateTaskCallbackEvent(taskDispatchEventMessage);
            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(TaskManager.TaskCallbackEvent, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    Assert.True(resetEvent.Wait(5000));
                    resetEvent.Reset(2);
                    await Task.Run(() =>
                    {
                        messageReceivedCallback(CreateMessageReceivedEventArgs(TaskCallbackEventMessage));
                    }).ConfigureAwait(false);
                });

            _messageBrokerPublisherService
                .Setup(p => p.Publish(It.IsAny<string>(), It.IsAny<Message>()))
                .Callback(() => resetEvent.Signal());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.Wait(5000));

            _testRunnerCallback.Verify(p => p.GenerateExecuteTaskResult(), Times.Once());
            _testRunnerCallback.Verify(p => p.GenerateGetStatusResult(), Times.Once());
            _messageBrokerSubscriberService.Verify(p => p.Acknowledge(It.Is<MessageBase>(m => m.MessageId == taskDispatchEventMessage.MessageId)), Times.Once());
            _messageBrokerPublisherService.Verify(p => p.Publish(It.Is<string>(m => m == TaskManager.TaskUpdateEvent), It.IsAny<Message>()), Times.Exactly(2));
            _messageBrokerSubscriberService.Verify(p => p.Reject(It.Is<MessageBase>(m => m.MessageId == TaskCallbackEventMessage.MessageId), It.Is<bool>(b => !b)), Times.Once());
        }

        [Fact(DisplayName = "Task Manager - TaskCallbackEvent completes workflow")]
        public async Task TaskManager_TaskCallbackEvent_CompletesWorkflow()
        {
            _options.Value.TaskManager.MaximumNumberOfConcurrentJobs = 1;
            _testRunnerCallback
                .Setup(p => p.GenerateExecuteTaskResult())
                .Returns(new ExecutionStatus { Status = Messaging.Events.TaskStatus.Accepted, FailureReason = FailureReason.None });
            _testRunnerCallback
                .Setup(p => p.GenerateGetStatusResult())
                .Returns(new ExecutionStatus { Status = Messaging.Events.TaskStatus.Succeeded, FailureReason = FailureReason.None });

            var resetEvent = new CountdownEvent(2);

            var taskDispatchEventMessage = GenerateTaskDispatchEvent();
            taskDispatchEventMessage.Body.TaskAssemblyName = typeof(TestRunner).AssemblyQualifiedName!;
            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(TaskManager.TaskDispatchEvent, StringComparison.OrdinalIgnoreCase)),
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

            var TaskCallbackEventMessage = GenerateTaskCallbackEvent(taskDispatchEventMessage);
            _messageBrokerSubscriberService.Setup(
                p => p.SubscribeAsync(It.Is<string>(p => p.Equals(TaskManager.TaskCallbackEvent, StringComparison.OrdinalIgnoreCase)),
                                 It.IsAny<string>(),
                                 It.IsAny<Func<MessageReceivedEventArgs, Task>>(),
                                 It.IsAny<ushort>()))
                .Callback<string, string, Func<MessageReceivedEventArgs, Task>, ushort>(async (topic, queue, messageReceivedCallback, prefetchCount) =>
                {
                    Assert.True(resetEvent.Wait(5000));
                    resetEvent.Reset(2);
                    await Task.Run(() =>
                    {
                        messageReceivedCallback(CreateMessageReceivedEventArgs(TaskCallbackEventMessage));
                    }).ConfigureAwait(false);
                });
            _messageBrokerSubscriberService
                .Setup(p => p.Acknowledge(It.IsAny<MessageBase>()))
                .Callback(() => resetEvent.Signal());
            _messageBrokerPublisherService
                .Setup(p => p.Publish(It.IsAny<string>(), It.IsAny<Message>()))
                .Callback(() => resetEvent.Signal());

            var service = new TaskManager(_logger.Object, _options, _serviceScopeFactory.Object);
            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            Assert.Equal(ServiceStatus.Running, service.Status);

            Assert.True(resetEvent.Wait(5000));

            _testRunnerCallback.Verify(p => p.GenerateExecuteTaskResult(), Times.Once());
            _testRunnerCallback.Verify(p => p.GenerateGetStatusResult(), Times.Once());
            _messageBrokerSubscriberService.Verify(p => p.Acknowledge(It.Is<MessageBase>(m => m.MessageId == taskDispatchEventMessage.MessageId)), Times.Once());
            _messageBrokerSubscriberService.Verify(p => p.Acknowledge(It.Is<MessageBase>(m => m.MessageId == TaskCallbackEventMessage.MessageId)), Times.Once());
            _messageBrokerPublisherService.Verify(p => p.Publish(It.Is<string>(m => m == TaskManager.TaskUpdateEvent), It.IsAny<Message>()), Times.Exactly(2));
        }

        private static JsonMessage<TaskCallbackEvent> GenerateTaskCallbackEvent(JsonMessage<TaskDispatchEvent>? taskDispatchEventMessage = null)
        {
            return new JsonMessage<TaskCallbackEvent>(
                            new TaskCallbackEvent
                            {
                                CorrelationId = taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.CorrelationId,
                                ExecutionId = taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.Body.ExecutionId,
                                WorkflowId = taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.Body.WorkflowId,
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
                                ExecutionId = Guid.NewGuid().ToString(),
                                TaskAssemblyName = Guid.NewGuid().ToString(),
                                WorkflowId = Guid.NewGuid().ToString(),
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
            return message;
        }

        private static MessageReceivedEventArgs CreateMessageReceivedEventArgs<T>(JsonMessage<T> message)
        {
            return new MessageReceivedEventArgs(message.ToMessage(), CancellationToken.None);
        }
    }
}
