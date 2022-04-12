// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
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
using Monai.Deploy.WorkflowManager.TaskManager.Logging;
using Newtonsoft.Json;

namespace TaskManager
{
    public class RunnerCompleteEvent : EventBase
    {
        /// <summary>
        /// Gets or sets the ID representing the instance of the workflow.
        /// </summary>
        [JsonProperty(PropertyName = "workflow_id")]
        [Required]
        public string? WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the ID representing the instance of the Task.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "task_id")]
        public string? TaskId { get; set; }

        /// <summary>
        /// Gets or sets the correlation ID.
        /// </summary>
        [JsonProperty(PropertyName = "correlation_id")]
        [Required]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the Argo job.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        [Required, MaxLength(63)]
        public string? Name { get; set; }
    }
    public class TaskManager : IHostedService, IDisposable, IMonaiService
    {
        private bool _disposedValue;
        private readonly ILogger<TaskManager> _logger;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IServiceScope _scope;
        private readonly IMessageBrokerPublisherService _messageBrokerPublisherService;
        private readonly IMessageBrokerSubscriberService _messageBrokerSubscriberService;
        private readonly BlockingCollection<MessageBase> _mockMessageQueue;
        private long _activeJobs;

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

            _messageBrokerPublisherService = _scope.ServiceProvider.GetRequiredService<IMessageBrokerPublisherService>() ?? throw new ServiceNotFoundException(nameof(IMessageBrokerPublisherService));
            _messageBrokerSubscriberService = _scope.ServiceProvider.GetRequiredService<IMessageBrokerSubscriberService>() ?? throw new ServiceNotFoundException(nameof(IMessageBrokerSubscriberService));

            _mockMessageQueue = new BlockingCollection<MessageBase>();
            _activeJobs = 0;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var task = Task.Run(() =>
            {
                BackgroundProcessing(cancellationToken);
            }, CancellationToken.None);

            Status = ServiceStatus.Running;
            _logger.ServiceStarted(ServiceName);

            if (task.IsCompleted)
                return task;
            return Task.CompletedTask;

        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.ServiceStopping(ServiceName);
            Status = ServiceStatus.Stopped;

            return Task.CompletedTask;
        }

        private void BackgroundProcessing(CancellationToken cancellationToken)
        {
            _logger.ServiceRunning(ServiceName);
            while (!cancellationToken.IsCancellationRequested)
            {
                MessageBase message = null;
                try
                {
                    message = _mockMessageQueue.Take(cancellationToken);
                    using var loggingScope = _logger.BeginScope($"Message ID={0}. Correlation ID={1}", message.MessageId, message.CorrelationId);

                    if (message is JsonMessage<TaskDispatchEvent> taskDispatchMessage)
                    {
                        taskDispatchMessage.Body.Validate();
                        HandleDispatchTask(taskDispatchMessage);
                    }
                    else if (message is JsonMessage<RunnerCompleteEvent> runnerCompleteEvent)
                    {
                        runnerCompleteEvent.Body.Validate();
                        HandleRunnerComplete(runnerCompleteEvent);
                    }
                    else
                    {
                        _logger.UnsupportedEvent(message.MessageDescription);
                        _messageBrokerSubscriberService.Reject(message, true);
                    }

                    if (!HasResourceAvailbleForExecution())
                    {
                        _logger.NoResourceAvailableForExecution();
                        _messageBrokerSubscriberService.Reject(message, true);
                    }

                }
                catch (MessageValidationException ex)
                {
                    _logger.InvalidMessageReceived(message.MessageId, message.CorrelationId, ex);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.ServiceCancelledWithException(ServiceName, ex);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.ServiceDisposed(ServiceName, ex);
                }
                catch (Exception ex)
                {
                    _logger.ErrorProcessingMessage(message?.MessageId, message?.CorrelationId, ex);
                }
            }
            Status = ServiceStatus.Cancelled;
            _logger.ServiceCancelled(ServiceName);
        }

        private void HandleRunnerComplete(JsonMessage<RunnerCompleteEvent> runnerCompleteEvent) => throw new NotImplementedException();
        private void HandleDispatchTask(JsonMessage<TaskDispatchEvent> taskDispatchMessage) => throw new NotImplementedException();

        private bool HasResourceAvailbleForExecution()
        {
            var activeJobs = Interlocked.Read(ref _activeJobs);
            if (activeJobs >= _options.Value.TaskManager.MaximumNumberOfConcurrentJobs)
            {
                return false;
            }

            var expectedActiveJobs = ++activeJobs;
            return Interlocked.CompareExchange(ref _activeJobs, expectedActiveJobs, activeJobs) != activeJobs;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
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
