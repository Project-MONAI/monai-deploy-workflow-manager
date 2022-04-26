// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Common;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Rest;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Logging;
using TaskStatus = Monai.Deploy.Messaging.Events.TaskStatus;

namespace Monai.Deploy.WorkflowManager.TaskManager
{
    public class TaskManager : IHostedService, IDisposable, IMonaiService
    {
        //TODO: change application ID, task topic
        private const string TaskManagerApplicationId = "23f81094-13fb-4964-ad6e-4cd434623ee9";

        internal const string TaskUpdateEvent = "md.tasks.update";
        internal const string TaskDispatchEvent = "md.tasks.dispatch";
        internal const string TaskCallbackEvent = "md.tasks.callback";

        private readonly ILogger<TaskManager> _logger;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IServiceScope _scope;
        private readonly IDictionary<string, TaskRunnerInstance> _activeExecutions;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private IMessageBrokerPublisherService? _messageBrokerPublisherService;
        private IMessageBrokerSubscriberService? _messageBrokerSubscriberService;

        private long _activeJobs;
        private bool _disposedValue;

        public ServiceStatus Status { get; set; } = ServiceStatus.Unknown;

        public string ServiceName => "MONAI Deploy Task Manager";

        public TaskManager(
            ILogger<TaskManager> logger,
            IOptions<WorkflowManagerOptions> options,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _scope = _serviceScopeFactory.CreateScope();

            _activeExecutions = new Dictionary<string, TaskRunnerInstance>();
            _cancellationTokenSource = new CancellationTokenSource();

            _messageBrokerPublisherService = null;
            _messageBrokerSubscriberService = null;
            _activeJobs = 0;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _messageBrokerPublisherService = _scope.ServiceProvider.GetRequiredService<IMessageBrokerPublisherService>() ?? throw new ServiceNotFoundException(nameof(IMessageBrokerPublisherService));
            _messageBrokerSubscriberService = _scope.ServiceProvider.GetRequiredService<IMessageBrokerSubscriberService>() ?? throw new ServiceNotFoundException(nameof(IMessageBrokerSubscriberService));

            _messageBrokerSubscriberService.SubscribeAsync(TaskDispatchEvent, string.Empty, TaskDispatchEventReceivedCallback);
            _messageBrokerSubscriberService.SubscribeAsync(TaskCallbackEvent, string.Empty, TaskCallbackEventReceivedCallback);

            Status = ServiceStatus.Running;
            _logger.ServiceStarted(ServiceName);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.ServiceStopping(ServiceName);

            _messageBrokerSubscriberService?.Dispose();
            _messageBrokerPublisherService?.Dispose();

            _cancellationTokenSource.Cancel();
            Status = ServiceStatus.Stopped;

            return Task.CompletedTask;
        }

        private async Task TaskCallbackEventReceivedCallback(MessageReceivedEventArgs args)
        {
            Guard.Against.Null(args, nameof(args));
            using var loggingScope = _logger.BeginScope($"Message Type={args.Message.MessageDescription}, ID={args.Message.MessageId}. Correlation ID={args.Message.CorrelationId}");
            var message = args.Message.ConvertToJsonMessage<TaskCallbackEvent>();
            try
            {
                await HandleTaskCallback(message).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                _logger.ServiceCancelledWithException(ServiceName, ex);
            }
            catch (Exception ex)
            {
                _logger.ErrorProcessingMessage(message?.MessageId, message?.CorrelationId, ex);
            }
        }

        private async Task TaskDispatchEventReceivedCallback(MessageReceivedEventArgs args)
        {
            Guard.Against.Null(args, nameof(args));

            using var loggingScope = _logger.BeginScope($"Message Type={args.Message.MessageDescription}, ID={args.Message.MessageId}. Correlation ID={args.Message.CorrelationId}");
            var message = args.Message.ConvertToJsonMessage<TaskDispatchEvent>();
            try
            {
                await HandleDispatchTask(message).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                _logger.ServiceCancelledWithException(ServiceName, ex);
            }
            catch (Exception ex)
            {
                _logger.ErrorProcessingMessage(message?.MessageId, message?.CorrelationId, ex);
            }
        }

        private async Task HandleTaskCallback(JsonMessage<TaskCallbackEvent> message)
        {
            Guard.Against.Null(message, nameof(message));

            try
            {
                message.Body.Validate();
            }
            catch (MessageValidationException ex)
            {
                _logger.InvalidMessageReceived(message.MessageId, message.CorrelationId, ex);
                await HandleMessageException(message, message.Body.WorkflowId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                return;
            }

            if (!_activeExecutions.TryGetValue(message.Body.ExecutionId, out var runner))
            {
                _logger.NoActiveExecutorWithTheId(message.Body.ExecutionId);
                await HandleMessageException(message, message.Body.WorkflowId, message.Body.TaskId, message.Body.ExecutionId, Strings.NoMatchingExecutorId, false).ConfigureAwait(false);
                return;
            }

            try
            {
                var executionStatus = await runner.Runner.GetStatus(message.Body.Identity, _cancellationTokenSource.Token).ConfigureAwait(false);
                AcknowledgeMessage(message);
                var updateMessage = GenerateUpdateEventMessage(message, message.Body.ExecutionId, message.Body.WorkflowId, message.Body.TaskId, executionStatus);
                updateMessage.Body.Metadata.Add(Strings.JobIdentity, message.Body.Identity);
                await SendUpdateEvent(updateMessage).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.ErrorExecutingTask(ex);
                await HandleMessageException(message, message.Body.WorkflowId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
            }

            Interlocked.Decrement(ref _activeJobs);
            _activeExecutions.Remove(message.Body.ExecutionId);
        }

        private async Task HandleDispatchTask(JsonMessage<TaskDispatchEvent> message)
        {
            Guard.Against.NullService(_messageBrokerSubscriberService, nameof(IMessageBrokerSubscriberService));
            Guard.Against.Null(message, nameof(message));

            try
            {
                message.Body.Validate();
            }
            catch (MessageValidationException ex)
            {
                _logger.InvalidMessageReceived(message.MessageId, message.CorrelationId, ex);
                await HandleMessageException(message, message.Body.WorkflowId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                return;
            }

            if (!TryReserveResourceForExecution())
            {
                _logger.NoResourceAvailableForExecution();
                _messageBrokerSubscriberService!.Reject(message, true);
                return;
            }

            ITaskRunner? taskRunner = null;
            try
            {
                taskRunner = typeof(ITaskRunner).CreateInstance<ITaskRunner>(serviceProvider: _scope.ServiceProvider, typeString: message.Body.TaskAssemblyName, _serviceScopeFactory, message.Body);
            }
            catch (Exception ex)
            {
                _logger.UnsupportedRunner(message.Body.TaskAssemblyName, ex);
                await HandleMessageException(message, message.Body.WorkflowId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                taskRunner?.Dispose();
                return;
            }

            try
            {
                var executionStatus = await taskRunner.ExecuteTask(_cancellationTokenSource.Token).ConfigureAwait(false);
                _activeExecutions.Add(message.Body.ExecutionId, new TaskRunnerInstance(taskRunner, message.Body));
                AcknowledgeMessage(message);
                var updateMessage = GenerateUpdateEventMessage(message, message.Body.ExecutionId, message.Body.WorkflowId, message.Body.TaskId, executionStatus);
                await SendUpdateEvent(updateMessage).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.ErrorExecutingTask(ex);
                await HandleMessageException(message, message.Body.WorkflowId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
            }
        }

        private void AcknowledgeMessage<T>(JsonMessage<T> message)
        {
            Guard.Against.NullService(_messageBrokerSubscriberService, nameof(IMessageBrokerSubscriberService));
            Guard.Against.Null(message, nameof(message));

            try
            {
                _logger.SendingAckMessage(message.MessageDescription);
                _messageBrokerSubscriberService!.Acknowledge(message);
                _logger.AckMessageSent(message.MessageDescription);
            }
            catch (Exception ex)
            {
                _logger.ErrorSendingMessage(message.MessageDescription, ex);
            }
        }

        private static JsonMessage<TaskUpdateEvent> GenerateUpdateEventMessage<T>(JsonMessage<T> message, string executionId, string workflowId, string taskId, ExecutionStatus executionStatus)
        {
            Guard.Against.Null(message, nameof(message));
            Guard.Against.Null(executionStatus, nameof(executionStatus));

            return new JsonMessage<TaskUpdateEvent>(new TaskUpdateEvent
            {
                CorrelationId = message.CorrelationId,
                ExecutionId = executionId,
                Reason = executionStatus.FailureReason,
                Status = executionStatus.Status,
                WorkflowId = workflowId,
                TaskId = taskId,
                Message = executionStatus.Errors,
            }, TaskManagerApplicationId, message.CorrelationId);
        }

        //TODO: perform retries
        private async Task SendUpdateEvent(JsonMessage<TaskUpdateEvent> message)
        {
            Guard.Against.NullService(_messageBrokerPublisherService, nameof(IMessageBrokerPublisherService));
            Guard.Against.Null(message, nameof(message));

            try
            {
                _logger.SendingTaskUpdateMessage(TaskUpdateEvent, message.Body.Reason);
                await _messageBrokerPublisherService!.Publish(TaskUpdateEvent, message.ToMessage()).ConfigureAwait(false);
                _logger.TaskUpdateMessageSent(TaskUpdateEvent);
            }
            catch (Exception ex)
            {
                _logger.ErrorSendingMessage(TaskUpdateEvent, ex);
            }
        }

        private bool TryReserveResourceForExecution()
        {
            var activeJobs = Interlocked.Read(ref _activeJobs);
            if (activeJobs >= _options.Value.TaskManager.MaximumNumberOfConcurrentJobs)
            {
                return false;
            }

            var expectedActiveJobs = ++activeJobs;
            return Interlocked.CompareExchange(ref _activeJobs, expectedActiveJobs, activeJobs) != activeJobs;
        }

        //TODO: change application ID, task topic
        private async Task HandleMessageException(MessageBase message, string workflowId, string taskId, string executionId, string errors, bool requeue)
        {
            Guard.Against.NullService(_messageBrokerSubscriberService, nameof(IMessageBrokerSubscriberService));

            if (message is null)
            {
                return;
            }

            using var loggerScope = _logger.BeginScope($"Workflow ID={workflowId}, Execution ID={executionId}, Task ID={taskId}, Correlation ID={message.CorrelationId}");

            try
            {
                _logger.SendingRejectMessageNoRequeue(message.MessageDescription);
                _messageBrokerSubscriberService!.Reject(message, requeue);
                _logger.RejectMessageNoRequeueSent(message.MessageDescription);
            }
            catch (Exception ex)
            {
                _logger.ErrorSendingMessage(message.MessageDescription, ex);
            }

            var updateMessage = new JsonMessage<TaskUpdateEvent>(new TaskUpdateEvent
            {
                CorrelationId = message.CorrelationId,
                ExecutionId = executionId,
                Reason = FailureReason.PluginError,
                Status = TaskStatus.Failed,
                WorkflowId = workflowId,
                TaskId = taskId,
                Message = errors,
            }, TaskManagerApplicationId, message.CorrelationId);

            await SendUpdateEvent(updateMessage).ConfigureAwait(false);
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
