// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Rest;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Logging;

namespace Monai.Deploy.WorkflowManager.TaskManager
{
    public class TaskManager : IHostedService, IDisposable, IMonaiService
    {
        private const string TaskManagerApplicationId = "4c9072a1-35f5-4d85-847d-dafca22244a8";

        private readonly ILogger<TaskManager> _logger;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IStorageAdminService _storageAdminService;
        private readonly IServiceScope _scope;
        private readonly IDictionary<string, TaskRunnerInstance> _activeExecutions;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IStorageService _storageService;
        private CancellationToken _cancellationToken;
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

            _storageAdminService = _scope.ServiceProvider.GetRequiredService<IStorageAdminService>() ?? throw new ServiceNotFoundException(nameof(IStorageAdminService));
            _storageService = _scope.ServiceProvider.GetRequiredService<IStorageService>() ?? throw new ServiceNotFoundException(nameof(IStorageService));
            _messageBrokerPublisherService = null;
            _messageBrokerSubscriberService = null;
            _activeJobs = 0;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _messageBrokerPublisherService = _scope.ServiceProvider.GetRequiredService<IMessageBrokerPublisherService>() ?? throw new ServiceNotFoundException(nameof(IMessageBrokerPublisherService));
            _messageBrokerSubscriberService = _scope.ServiceProvider.GetRequiredService<IMessageBrokerSubscriberService>() ?? throw new ServiceNotFoundException(nameof(IMessageBrokerSubscriberService));

            _messageBrokerSubscriberService.SubscribeAsync(_options.Value.Messaging.Topics.TaskDispatchRequest, string.Empty, TaskDispatchEventReceivedCallback);
            _messageBrokerSubscriberService.SubscribeAsync(_options.Value.Messaging.Topics.TaskCallbackRequest, string.Empty, TaskCallbackEventReceivedCallback);

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

            JsonMessage<TaskDispatchEvent>? message = null;
            try
            {
                message = args.Message.ConvertToJsonMessage<TaskDispatchEvent>();
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
                await HandleMessageException(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                return;
            }

            if (!_activeExecutions.TryGetValue(message.Body.ExecutionId, out var runner))
            {
                _logger.NoActiveExecutorWithTheId(message.Body.ExecutionId);
                await HandleMessageException(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, Strings.NoMatchingExecutorId, false).ConfigureAwait(false);
                return;
            }

            var metadataAssembly = string.Empty;
            JsonMessage<TaskUpdateEvent>? updateMessage = null;

            try
            {
                if (_options.Value.TaskManager.MetadataAssemblyMappings.TryGetValue(runner.Event.TaskPluginType, out var metadataMappingValue))
                {
                    metadataAssembly = metadataMappingValue;
                }
                var executionStatus = await runner.Runner.GetStatus(message.Body.Identity, _cancellationTokenSource.Token).ConfigureAwait(false);
                updateMessage = GenerateUpdateEventMessage(message, message.Body.ExecutionId, message.Body.WorkflowInstanceId, message.Body.TaskId, executionStatus);
                updateMessage.Body.Metadata.Add(Strings.JobIdentity, message.Body.Identity);
            }
            catch (Exception ex)
            {
                _logger.ErrorExecutingTask(ex);
                await HandleMessageException(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);

                return;
            }

            if (!string.IsNullOrWhiteSpace(metadataAssembly))
            {
                IMetadataRepository? metadataRepository = null;

                try
                {
                    metadataRepository = typeof(IMetadataRepository).CreateInstance<IMetadataRepository>(serviceProvider: _scope.ServiceProvider, typeString: metadataAssembly, _serviceScopeFactory, runner.Event, message.Body);
                }
                catch (Exception ex)
                {
                    _logger.UnsupportedRunner(metadataAssembly, ex);
                    await HandleMessageException(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                    metadataRepository?.Dispose();

                    return;
                }

                try
                {
                    var metadata = await metadataRepository.RetrieveMetadata().ConfigureAwait(false);

                    foreach (var item in metadata)
                        updateMessage.Body.Metadata.Add(item.Key, item.Value);
                }
                catch (Exception ex)
                {
                    updateMessage.Body.Status = TaskExecutionStatus.Failed;
                    updateMessage.Body.Reason = FailureReason.PluginError;

                    _logger.MetadataRetrievalFailed(ex);
                }
            }

            AcknowledgeMessage(message);
            await SendUpdateEvent(updateMessage).ConfigureAwait(false);

            Interlocked.Decrement(ref _activeJobs);
            _activeExecutions.Remove(message.Body.ExecutionId);
        }

        private async Task HandleDispatchTask(JsonMessage<TaskDispatchEvent> message)
        {
            Guard.Against.NullService(_messageBrokerSubscriberService, nameof(IMessageBrokerSubscriberService));
            Guard.Against.Null(message, nameof(message));

            var pluginAssembly = string.Empty;
            try
            {
                message.Body.Validate();
                pluginAssembly = _options.Value.TaskManager.PluginAssemblyMappings[message.Body.TaskPluginType];
            }
            catch (MessageValidationException ex)
            {
                _logger.InvalidMessageReceived(message.MessageId, message.CorrelationId, ex);
                await HandleMessageException(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                return;
            }

            if (!TryReserveResourceForExecution())
            {
                _logger.NoResourceAvailableForExecution();
                _messageBrokerSubscriberService!.Reject(message, true);
                return;
            }

            try
            {
                if (string.Equals(message.Body.TaskPluginType,
                                  PluginStrings.Argo,
                                  StringComparison.InvariantCultureIgnoreCase))
                {
                    await AddCredentialsToPlugin(message).ConfigureAwait(false);
                }
                else
                {
                    await Task.WhenAll(
                        PopulateTemporaryStorageCredentials(message.Body.Inputs.ToArray()),
                        PopulateTemporaryStorageCredentials(message.Body.IntermediateStorage),
                        PopulateTemporaryStorageCredentials(message.Body.Outputs.ToArray())
                    ).ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                _logger.GenerateTemporaryCredentialsException(ex);
                await HandleMessageException(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, true).ConfigureAwait(false);
                return;
            }

            ITaskPlugin? taskRunner = null;
            try
            {
                taskRunner = typeof(ITaskPlugin).CreateInstance<ITaskPlugin>(serviceProvider: _scope.ServiceProvider, typeString: pluginAssembly, _serviceScopeFactory, message.Body);
            }
            catch (Exception ex)
            {
                _logger.UnsupportedRunner(pluginAssembly, ex);
                await HandleMessageException(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                taskRunner?.Dispose();
                return;
            }

            try
            {
                var executionStatus = await taskRunner.ExecuteTask(_cancellationTokenSource.Token).ConfigureAwait(false);
                _activeExecutions.Add(message.Body.ExecutionId, new TaskRunnerInstance(taskRunner, message.Body));
                var updateMessage = GenerateUpdateEventMessage(message, message.Body.ExecutionId, message.Body.WorkflowInstanceId, message.Body.TaskId, executionStatus);
                await SendUpdateEvent(updateMessage).ConfigureAwait(false);
                AcknowledgeMessage(message);
            }
            catch (Exception ex)
            {
                _logger.ErrorExecutingTask(ex);
                await HandleMessageException(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
            }
        }

        private async Task AddCredentialsToPlugin(JsonMessage<TaskDispatchEvent> message)
        {
            var storages = new List<Messaging.Common.Storage>();
            storages.Add(message.Body.IntermediateStorage);
            storages.AddRange(message.Body.Outputs);
            storages.AddRange(message.Body.Inputs);

            var storageBuckets = storages.Select(storage => storage.Bucket)
                .Distinct()
                .ToArray();

            var creds = await _storageAdminService.CreateUserAsync(
                $"TM{Guid.NewGuid()}",
                 AccessPermissions.Read,
                 storageBuckets);

            if (creds is null)
            {
                _logger.LogError("Credentials not generated");
                return;
            }

            foreach (var storage in storages)
            {
                storage.Credentials = new Credentials
                {
                    AccessKey = creds.AccessKeyId,
                    AccessToken = creds.SecretAccessKey
                };
            }
        }

        private async Task PopulateTemporaryStorageCredentials(params Messaging.Common.Storage[] storages)
        {
            Guard.Against.Null(storages, nameof(storages));

            foreach (var storage in storages)
            {
                var credeentials = await _storageService.CreateTemporaryCredentialsAsync(storage.Bucket, storage.RelativeRootPath, _options.Value.TaskManager.TemporaryStorageCredentialDurationSeconds, _cancellationToken).ConfigureAwait(false);
                storage.Credentials = new Credentials
                {
                    AccessKey = credeentials.AccessKeyId,
                    AccessToken = credeentials.SecretAccessKey,
                    SessionToken = credeentials.SessionToken,
                };
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

        private static JsonMessage<TaskUpdateEvent> GenerateUpdateEventMessage<T>(JsonMessage<T> message, string executionId, string WorkflowInstanceId, string taskId, ExecutionStatus executionStatus, List<Messaging.Common.Storage> outputs = null)
        {
            Guard.Against.Null(message, nameof(message));
            Guard.Against.Null(executionStatus, nameof(executionStatus));

            return new JsonMessage<TaskUpdateEvent>(new TaskUpdateEvent
            {
                CorrelationId = message.CorrelationId,
                ExecutionId = executionId,
                Reason = executionStatus.FailureReason,
                Status = executionStatus.Status,
                WorkflowInstanceId = WorkflowInstanceId,
                TaskId = taskId,
                Message = executionStatus.Errors,
                Outputs = outputs ?? new List<Messaging.Common.Storage>(),
            }, TaskManagerApplicationId, message.CorrelationId);
        }

        //TODO: gh-100 implement retry logic
        private async Task SendUpdateEvent(JsonMessage<TaskUpdateEvent> message)
        {
            Guard.Against.NullService(_messageBrokerPublisherService, nameof(IMessageBrokerPublisherService));
            Guard.Against.Null(message, nameof(message));

            try
            {
                _logger.SendingTaskUpdateMessage(_options.Value.Messaging.Topics.TaskUpdateRequest, message.Body.Reason);
                await _messageBrokerPublisherService!.Publish(_options.Value.Messaging.Topics.TaskUpdateRequest, message.ToMessage()).ConfigureAwait(false);
                _logger.TaskUpdateMessageSent(_options.Value.Messaging.Topics.TaskUpdateRequest);
            }
            catch (Exception ex)
            {
                _logger.ErrorSendingMessage(_options.Value.Messaging.Topics.TaskUpdateRequest, ex);
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

        private async Task HandleMessageException(MessageBase message, string WorkflowInstanceId, string taskId, string executionId, string errors, bool requeue)
        {
            Guard.Against.NullService(_messageBrokerSubscriberService, nameof(IMessageBrokerSubscriberService));

            if (message is null)
            {
                return;
            }

            using var loggerScope = _logger.BeginScope($"Workflow ID={WorkflowInstanceId}, Execution ID={executionId}, Task ID={taskId}, Correlation ID={message.CorrelationId}");

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
                Status = TaskExecutionStatus.Failed,
                WorkflowInstanceId = WorkflowInstanceId,
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
