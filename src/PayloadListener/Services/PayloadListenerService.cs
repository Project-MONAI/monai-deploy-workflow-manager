// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.API;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Rest;
using Monai.Deploy.WorkflowManager.Logging.Logging;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Services
{
    public class PayloadListenerService : IHostedService, IMonaiService, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IServiceScope _scope;

        private readonly IEventPayloadReceiverService _eventPayloadListenerService;

        private readonly IMessageBrokerSubscriberService _messageSubscriber;
        private bool _disposedValue;

        public string WorkflowRequestRoutingKey { get; set; }
        public string TaskStatusUpdateRoutingKey { get; set; }
        public string ExportCompleteRoutingKey { get; set; }
        protected int Concurrency { get; set; }
        public ServiceStatus Status { get; set; } = ServiceStatus.Unknown;
        public string ServiceName => "Payload Listener Service";

        public PayloadListenerService(
            ILogger<PayloadListenerService> logger,
            IOptions<WorkflowManagerOptions> configuration,
            IServiceScopeFactory serviceScopeFactory,
            IEventPayloadReceiverService eventPayloadListenerService)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _scope = _serviceScopeFactory.CreateScope();

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            TaskStatusUpdateRoutingKey = configuration.Value.Messaging.Topics.TaskUpdateRequest;
            WorkflowRequestRoutingKey = configuration.Value.Messaging.Topics.WorkflowRequest;
            ExportCompleteRoutingKey = configuration.Value.Messaging.Topics.ExportComplete;

            Concurrency = 2;

            _eventPayloadListenerService = eventPayloadListenerService ?? throw new ArgumentNullException(nameof(eventPayloadListenerService));

            _messageSubscriber = _scope.ServiceProvider.GetRequiredService<IMessageBrokerSubscriberService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            SetupPolling();

            Status = ServiceStatus.Running;
            _logger.ServiceStarted(ServiceName);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            _logger.ServiceStopping(ServiceName);
            Status = ServiceStatus.Stopped;
            return Task.CompletedTask;
        }

        private void SetupPolling()
        {
            _messageSubscriber.Subscribe(WorkflowRequestRoutingKey, String.Empty, OnWorkflowRequestReceivedCallback);
            _logger.EventSubscription(ServiceName, WorkflowRequestRoutingKey);

            _messageSubscriber.Subscribe(TaskStatusUpdateRoutingKey, String.Empty, OnTaskUpdateStatusReceivedCallback);
            _logger.EventSubscription(ServiceName, TaskStatusUpdateRoutingKey);

            _messageSubscriber.Subscribe(ExportCompleteRoutingKey, String.Empty, OnExportCompleteReceivedCallback);
            _logger.EventSubscription(ServiceName, ExportCompleteRoutingKey);
        }

        private void OnWorkflowRequestReceivedCallback(MessageReceivedEventArgs eventArgs)
        {
            Task.Run(async () =>
            {
                await _eventPayloadListenerService.ReceiveWorkflowPayload(eventArgs);
            }).ConfigureAwait(false);
        }

        private void OnTaskUpdateStatusReceivedCallback(MessageReceivedEventArgs eventArgs)
        {
            Task.Run(async () =>
            {
                await _eventPayloadListenerService.TaskUpdatePayload(eventArgs);
            }).ConfigureAwait(false);
        }

        private void OnExportCompleteReceivedCallback(MessageReceivedEventArgs eventArgs)
        {
            Task.Run(async () =>
            {
                await _eventPayloadListenerService.ExportCompletePayload(eventArgs);
            }).ConfigureAwait(false);
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
