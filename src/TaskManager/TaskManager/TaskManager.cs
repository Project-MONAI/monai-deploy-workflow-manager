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

using System.Reflection;
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
using Monai.Deploy.Storage.S3Policy.Policies;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.API.Extensions;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Monai.Deploy.WorkflowManager.TaskManager.Logging;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1201 // Elements should be documented
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
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IStorageService _storageService;
        private readonly ITaskDispatchEventService _taskDispatchEventService;
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

            _cancellationTokenSource = new CancellationTokenSource();

            _storageAdminService = _scope.ServiceProvider.GetService<IStorageAdminService>() ?? throw new ServiceNotFoundException(nameof(IStorageAdminService));
            _storageService = _scope.ServiceProvider.GetService<IStorageService>() ?? throw new ServiceNotFoundException(nameof(IStorageService));
            _taskDispatchEventService = _scope.ServiceProvider.GetService<ITaskDispatchEventService>() ?? throw new ServiceNotFoundException(nameof(ITaskDispatchEventService));
            _messageBrokerPublisherService = null;
            _messageBrokerSubscriberService = null;
            _activeJobs = 0;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _messageBrokerPublisherService = _scope.ServiceProvider.GetRequiredService<IMessageBrokerPublisherService>() ?? throw new ServiceNotFoundException(nameof(IMessageBrokerPublisherService));
            _messageBrokerSubscriberService = _scope.ServiceProvider.GetRequiredService<IMessageBrokerSubscriberService>() ?? throw new ServiceNotFoundException(nameof(IMessageBrokerSubscriberService));

            _messageBrokerSubscriberService.OnConnectionError += (sender, args) =>
            {
                _logger.MessagingServiceErrorRecover(args.ErrorMessage);
                SubscribeToEvents();
            };
            SubscribeToEvents();

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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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

        private static JsonMessage<TaskUpdateEvent> GenerateUpdateEventMessage<T>(
            JsonMessage<T> message,
            string executionId,
            string workflowInstanceId,
            string taskId,
            ExecutionStatus executionStatus,
            List<Messaging.Common.Storage>? outputs = null)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));
            ArgumentNullException.ThrowIfNull(executionStatus, nameof(executionStatus));

            var body = new TaskUpdateEvent
            {
                CorrelationId = message.CorrelationId,
                ExecutionId = executionId,
                Reason = executionStatus.FailureReason,
                Status = executionStatus.Status,
                ExecutionStats = executionStatus.Stats,
                WorkflowInstanceId = workflowInstanceId,
                TaskId = taskId,
                Message = executionStatus.Errors,
                Outputs = outputs ?? new List<Messaging.Common.Storage>(),
            };
            return new JsonMessage<TaskUpdateEvent>(body, TaskManagerApplicationId, message.CorrelationId);
        }

        private void SubscribeToEvents()
        {
            _messageBrokerSubscriberService.SubscribeAsync(_options.Value.Messaging.Topics.TaskDispatchRequest, _options.Value.Messaging.Topics.TaskDispatchRequest, TaskDispatchEventReceivedCallback);
            _messageBrokerSubscriberService.SubscribeAsync(_options.Value.Messaging.Topics.TaskCallbackRequest, _options.Value.Messaging.Topics.TaskCallbackRequest, TaskCallbackEventReceivedCallback);
            _messageBrokerSubscriberService.SubscribeAsync(_options.Value.Messaging.Topics.TaskCancellationRequest, _options.Value.Messaging.Topics.TaskCancellationRequest, TaskCancelationEventCallback);
        }

        private async Task TaskCallbackEventReceivedCallback(MessageReceivedEventArgs args)
        {
            await TaskCallBackGeneric<TaskCallbackEvent>(args, HandleTaskCallback);
        }

        private async Task TaskDispatchEventReceivedCallback(MessageReceivedEventArgs args)
        {
            await TaskCallBackGeneric<TaskDispatchEvent>(args, HandleDispatchTask);
        }

        private async Task TaskCancelationEventCallback(MessageReceivedEventArgs args)
        {
            // Cancelation just stops running tasks and does Not set any status
            await TaskCallBackGeneric<TaskCancellationEvent>(args, HandleCancellationTask);
        }

        /// <summary>
        /// Generic Version of task callbacks.
        /// </summary>
        /// <typeparam name="T">type.</typeparam>
        /// <param name="args">Message.</param>
        /// <param name="func">Action to run on the message.</param>
        /// <returns>Task.</returns>
        private async Task TaskCallBackGeneric<T>(MessageReceivedEventArgs args, Func<JsonMessage<T>, Task> func)
            where T : EventBase
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            using var loggingScope = _logger.BeginScope(new Common.Miscellaneous.LoggingDataDictionary<string, object>
            {
                ["correlationId"] = args.Message.CorrelationId,
                ["messageId"] = args.Message.MessageId,
                ["messageType"] = args.Message.MessageDescription,
            });

            JsonMessage<T>? message = null;
            try
            {
                message = args.Message.ConvertToJsonMessage<T>();

                // Run action on the message
                await func(message).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                _logger.ServiceCancelledWithException(ServiceName, ex);
                await _messageBrokerSubscriberService!.RequeueWithDelay(args.Message);
            }
            catch (Exception ex)
            {
                _logger.ErrorProcessingMessage(message?.MessageId, message?.CorrelationId, ex);
                await _messageBrokerSubscriberService!.RequeueWithDelay(args.Message);
            }
        }

        private async Task HandleCancellationTask(JsonMessage<TaskCancellationEvent> message)
        {
            _logger.PrecessingTaskCancellationEvent();
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            try
            {
                message.Body.Validate();
            }
            catch (MessageValidationException ex)
            {
                _logger.InvalidMessageReceived(message.MessageId, message.CorrelationId, ex);
                await HandleMessageException(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, false).ConfigureAwait(false);
                return;
            }

            var pluginAssembly = string.Empty;
            ITaskPlugin? taskRunner;
            try
            {
                var taskExecution = await _taskDispatchEventService.GetByTaskExecutionIdAsync(message.Body.ExecutionId).ConfigureAwait(false);

                var taskExecEvent = taskExecution?.Event ?? throw new InvalidOperationException("Task Event data not found.");

                pluginAssembly = string.Empty;
                if (_options.Value.TaskManager.PluginAssemblyMappings.ContainsKey(taskExecution?.Event.TaskPluginType))
                {
                    pluginAssembly = _options.Value.TaskManager.PluginAssemblyMappings[taskExecution?.Event.TaskPluginType];
                }

                taskRunner = typeof(ITaskPlugin).CreateInstance<ITaskPlugin>(serviceProvider: _scope.ServiceProvider, typeString: pluginAssembly, _serviceScopeFactory, taskExecEvent);
            }
            catch (Exception ex)
            {
                _logger.UnsupportedRunner(pluginAssembly, ex);
                await HandleMessageException(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, false).ConfigureAwait(false);
                return;
            }

            try
            {
                await taskRunner.HandleTimeout(message.Body.Identity);
            }
            catch (Exception ex)
            {
                // Ignoring exception here as we've asked for the task to be stopped.
                _logger.ExectionTimingOutTask(pluginAssembly, ex);
            }
            finally
            {
                AcknowledgeMessage(message);
            }
        }

        private async Task HandleTaskCallback(JsonMessage<TaskCallbackEvent> message)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            try
            {
                message.Body.Validate();
            }
            catch (MessageValidationException ex)
            {
                _logger.InvalidMessageReceived(message.MessageId, message.CorrelationId, ex);
                await HandleMessageExceptionTaskUpdate(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                return;
            }

            var taskExecution = await _taskDispatchEventService.GetByTaskExecutionIdAsync(message.Body.ExecutionId).ConfigureAwait(false);
            if (taskExecution is null)
            {
                _logger.NoActiveExecutorWithTheId(message.Body.ExecutionId);
                await HandleMessageExceptionTaskUpdate(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, Strings.NoMatchingExecutorId, false).ConfigureAwait(false);
                return;
            }

            ITaskPlugin? taskRunner = null;

            using var loggingScope = _logger.BeginScope(new Common.Miscellaneous.LoggingDataDictionary<string, object>
            {
                ["durationSoFar"] = (DateTime.UtcNow - taskExecution.Started).TotalMilliseconds,
                ["correlationId"] = taskExecution.Event.CorrelationId,
                ["workflowInstanceId"] = taskExecution.Event.WorkflowInstanceId,
                ["taskId"] = taskExecution.Event.TaskId,
                ["executionId"] = taskExecution.Event.ExecutionId,
            });

            try
            {
                string? pluginAssembly;
                try
                {
                    pluginAssembly = _options.Value.TaskManager.PluginAssemblyMappings[taskExecution.Event.TaskPluginType];
                }
                catch (MessageValidationException ex)
                {
                    _logger.InvalidMessageReceived(message.MessageId, message.CorrelationId, ex);
                    await HandleMessageExceptionTaskUpdate(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                    return;
                }

                try
                {
                    taskRunner = typeof(ITaskPlugin).CreateInstance<ITaskPlugin>(serviceProvider: _scope.ServiceProvider, typeString: pluginAssembly, _serviceScopeFactory, taskExecution.Event);
                }
                catch (Exception ex)
                {
                    _logger.UnsupportedRunner(pluginAssembly, ex);
                    await HandleMessageExceptionTaskUpdate(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                    return;
                }

                JsonMessage<TaskUpdateEvent>? updateMessage;
                try
                {
                    var executionStatus = await taskRunner.GetStatus(message.Body.Identity, message.Body, _cancellationTokenSource.Token).ConfigureAwait(false);
                    updateMessage = GenerateUpdateEventMessage(message, message.Body.ExecutionId, message.Body.WorkflowInstanceId, message.Body.TaskId, executionStatus, taskExecution.Event.Outputs);
                    updateMessage.Body.Metadata.Add(Strings.JobIdentity, message.Body.Identity);
                    foreach (var item in message.Body.Metadata)
                    {
                        updateMessage.Body.Metadata.Add(item.Key, item.Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorExecutingTask(ex);
                    await HandleMessageExceptionTaskUpdate(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                    return;
                }

                try
                {
                    if (_options.Value.TaskManager.MetadataAssemblyMappings.TryGetValue(taskExecution.Event.TaskPluginType, out var metadataMappingValue))
                    {
                        var metadata = await RetrievePluginMetadata(message, taskExecution.Event, metadataMappingValue).ConfigureAwait(false);

                        if (metadata is not null)
                        {
                            foreach (var item in metadata)
                            {
                                updateMessage.Body.Metadata.Add(item.Key, item.Value);
                            }
                        }
                    }
                    else
                    {
                        _logger.MetadataPluginUndefined(taskExecution.Event.TaskPluginType);
                    }
                }
                catch (Exception ex)
                {
                    _logger.MetadataRetrievalFailed(ex);
                    await HandleMessageExceptionTaskUpdate(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                    return;
                }

                AcknowledgeMessage(message);
                await SendUpdateEvent(updateMessage).ConfigureAwait(false);

                Interlocked.Decrement(ref _activeJobs);
                await RemoveUserAccounts(taskExecution).ConfigureAwait(false);
            }
            finally
            {
                taskRunner?.Dispose();
            }
        }

        private async Task RemoveUserAccounts(TaskDispatchEventInfo taskDispatchEventInfo)
        {
            ArgumentNullException.ThrowIfNull(taskDispatchEventInfo, nameof(taskDispatchEventInfo));

            foreach (var user in taskDispatchEventInfo.UserAccounts)
            {
                try
                {
                    await _storageAdminService.RemoveUserAsync(user);
                }
                catch (Exception ex)
                {
                    _logger.ErrorRemovingStorageUserAccount(user, ex);
                }
            }
        }

        private async Task HandleDispatchTask(JsonMessage<TaskDispatchEvent> message)
        {
            Guard.Against.NullService(_messageBrokerSubscriberService, nameof(IMessageBrokerSubscriberService));
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            var pluginAssembly = string.Empty;
            var eventInfo = new API.Models.TaskDispatchEventInfo(message.Body);
            try
            {
                using var messageLoggingScope = _logger.BeginScope(new Common.Miscellaneous.LoggingDataDictionary<string, object>
                {
                    ["workflowInstanceId"] = eventInfo.Event.WorkflowInstanceId,
                    ["taskId"] = eventInfo.Event.TaskId,
                    ["executionId"] = eventInfo.Event.ExecutionId,
                });

                await _taskDispatchEventService.CreateAsync(eventInfo).ConfigureAwait(false);
                message.Body.Validate();
                pluginAssembly = _options.Value.TaskManager.PluginAssemblyMappings[message.Body.TaskPluginType];
            }
            catch (MessageValidationException ex)
            {
                _logger.InvalidMessageReceived(message.MessageId, message.CorrelationId, ex);
                await HandleMessageExceptionTaskUpdate(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                return;
            }

            if (!TryReserveResourceForExecution())
            {
                _logger.NoResourceAvailableForExecution();
                await _messageBrokerSubscriberService!.RequeueWithDelay(message);

                return;
            }

            try
            {
                if (PluginStrings.PlugsRequiresPermanentAccounts.Contains(
                        message.Body.TaskPluginType,
                        StringComparer.InvariantCultureIgnoreCase))
                {
                    eventInfo.AddUserAccount(await AddCredentialsToPlugin(message).ConfigureAwait(false));
                }
                else
                {
                    await Task.WhenAll(
                        PopulateTemporaryStorageCredentials(message.Body.Inputs.ToArray()),
                        PopulateTemporaryStorageCredentials(message.Body.IntermediateStorage),
                        PopulateTemporaryStorageCredentials(message.Body.Outputs.ToArray()))
                    .ConfigureAwait(true);
                }

                await _taskDispatchEventService.UpdateUserAccountsAsync(eventInfo).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.GenerateTemporaryCredentialsException(ex);
                await HandleMessageExceptionTaskUpdate(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, true).ConfigureAwait(false);
                return;
            }

            ITaskPlugin? taskRunner = null;
            try
            {
                taskRunner = typeof(ITaskPlugin).CreateInstance<ITaskPlugin>(serviceProvider: _scope.ServiceProvider, typeString: pluginAssembly, _serviceScopeFactory, message.Body);
            }
            catch (Exception ex)
            {
                var ass = Assembly.Load("Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview");
                var types = ass.GetTypes().ToArray();

                _logger.UnsupportedRunner(pluginAssembly, ex);
                await HandleMessageExceptionTaskUpdate(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
                taskRunner?.Dispose();
                return;
            }

            try
            {
                var executionStatus = await taskRunner.ExecuteTask(_cancellationTokenSource.Token).ConfigureAwait(false);
                var updateMessage = GenerateUpdateEventMessage(message, message.Body.ExecutionId, message.Body.WorkflowInstanceId, message.Body.TaskId, executionStatus);
                await SendUpdateEvent(updateMessage).ConfigureAwait(false);
                AcknowledgeMessage(message);
            }
            catch (Exception ex)
            {
                _logger.ErrorExecutingTask(ex);
                await HandleMessageExceptionTaskUpdate(message, message.Body.WorkflowInstanceId, message.Body.TaskId, message.Body.ExecutionId, ex.Message, false).ConfigureAwait(false);
            }
        }

        private async Task<Dictionary<string, object>?> RetrievePluginMetadata(JsonMessage<TaskCallbackEvent> message, TaskDispatchEvent dispatchEvent, string? metadataAssembly)
        {
            if (string.IsNullOrWhiteSpace(metadataAssembly))
            {
                return null;
            }

            IMetadataRepository? metadataRepository = null;
            try
            {
                metadataRepository = typeof(IMetadataRepository).CreateInstance<IMetadataRepository>(serviceProvider: _scope.ServiceProvider, typeString: metadataAssembly, _serviceScopeFactory, dispatchEvent, message.Body);
            }
            catch (Exception ex)
            {
                _logger.UnsupportedRunner(metadataAssembly, ex);
                metadataRepository?.Dispose();

                throw;
            }

            return await metadataRepository.RetrieveMetadata().ConfigureAwait(false);
        }

        private async Task<string> AddCredentialsToPlugin(JsonMessage<TaskDispatchEvent> message)
        {
            var storages = new List<Messaging.Common.Storage>();
            storages.Add(message.Body.IntermediateStorage);
            storages.AddRange(message.Body.Outputs);
            storages.AddRange(message.Body.Inputs);

            var policyRequests = storages.Select(storage => new PolicyRequest(storage.Bucket, storage.RelativeRootPath)).ToArray();

            var username = $"TM{Guid.NewGuid()}";
            var creds = await _storageAdminService.CreateUserAsync(username, policyRequests);

            if (creds is null)
            {
                throw new TaskManagerException("Credentials not generated");
            }

            foreach (var storage in storages)
            {
                storage.Credentials = new Credentials
                {
                    AccessKey = creds.AccessKeyId,
                    AccessToken = creds.SecretAccessKey,
                };
            }

            return username;
        }

        private async Task PopulateTemporaryStorageCredentials(params Messaging.Common.Storage[] storages)
        {
            ArgumentNullException.ThrowIfNull(storages, nameof(storages));

            foreach (var storage in storages)
            {
                var credentials = await _storageService.CreateTemporaryCredentialsAsync(
                        storage.Bucket,
                        ShortenStoragePath(storage.RelativeRootPath),
                        _options.Value.TaskManager.TemporaryStorageCredentialDurationSeconds,
                        _cancellationToken)
                    .ConfigureAwait(false);

                storage.Credentials = new Credentials
                {
                    AccessKey = credentials.AccessKeyId,
                    AccessToken = credentials.SecretAccessKey,
                    SessionToken = credentials.SessionToken,
                };
            }
        }

        // added because AWS s3 policy creation is by defualt limited to 2048 characters, which
        // can easily be surpassed with long multipart path names.
        private string ShortenStoragePath(string path)
        {
            var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (pathParts.Length <= 3)
            {
                return path;
            }

            var startsWith = path[0] == '/' ? "/" : string.Empty;
            return $"{startsWith}{pathParts[0]}/{pathParts[1]}/{pathParts[2]}";
        }

        private void AcknowledgeMessage<T>(JsonMessage<T> message)
        {
            Guard.Against.NullService(_messageBrokerSubscriberService, nameof(IMessageBrokerSubscriberService));
            ArgumentNullException.ThrowIfNull(message, nameof(message));

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

        // TODO: gh-100 implement retry logic
        private async Task SendUpdateEvent(JsonMessage<TaskUpdateEvent> message)
        {
            Guard.Against.NullService(_messageBrokerPublisherService, nameof(IMessageBrokerPublisherService));
            ArgumentNullException.ThrowIfNull(message, nameof(message));

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

        private async Task HandleMessageExceptionTaskUpdate(MessageBase message, string workflowInstanceId, string taskId, string executionId, string errors, bool requeue)
        {
            Guard.Against.NullService(_messageBrokerSubscriberService, nameof(IMessageBrokerSubscriberService));

            await HandleMessageException(message, workflowInstanceId, taskId, executionId, requeue);

            var updateMessage = new JsonMessage<TaskUpdateEvent>(
                new TaskUpdateEvent
                {
                    CorrelationId = message.CorrelationId,
                    ExecutionId = executionId,
                    Reason = FailureReason.PluginError,
                    Status = TaskExecutionStatus.Failed,
                    WorkflowInstanceId = workflowInstanceId,
                    TaskId = taskId,
                    Message = errors,
                },
                TaskManagerApplicationId,
                message.CorrelationId);

            await SendUpdateEvent(updateMessage).ConfigureAwait(false);
        }

        private async Task HandleMessageException(MessageBase message, string workflowInstanceId, string taskId, string executionId, bool requeue)
        {
            if (message is null)
            {
                return;
            }

            using var loggingScope = _logger.BeginScope(new Common.Miscellaneous.LoggingDataDictionary<string, object>
            {
                ["correlationId"] = message.CorrelationId,
                ["workflowInstanceId"] = workflowInstanceId,
                ["taskId"] = taskId,
                ["executionId"] = executionId,
            });

            try
            {
                _logger.SendingRejectMessageNoRequeue(message.MessageDescription);

                if (requeue)
                {
                    await _messageBrokerSubscriberService!.RequeueWithDelay(message);
                }
                else
                {
                    _messageBrokerSubscriberService!.Reject(message, false);
                }

                _logger.RejectMessageNoRequeueSent(message.MessageDescription);
            }
            catch (Exception ex)
            {
                _logger.ErrorSendingMessage(message.MessageDescription, ex);
            }
        }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore SA1600 // Elements should be documented
    }
}
