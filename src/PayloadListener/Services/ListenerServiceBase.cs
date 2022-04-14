﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Rest;
using Monai.Deploy.WorkflowManager.Logging.Logging;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Services
{
    public abstract class ListenerServiceBase : IHostedService, IMonaiService, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IServiceScope _scope;

        private readonly IEventPayloadRecieverService _eventPayloadListenerService;

        private readonly IMessageBrokerSubscriberService _messageSubscriber;
        private bool _disposedValue;

        public abstract string WorkflowRequestRoutingKey { get; }
        protected abstract int Concurrency { get; }
        public ServiceStatus Status { get; set; } = ServiceStatus.Unknown;
        public abstract string ServiceName { get; }

        protected ListenerServiceBase(
            ILogger logger,
            IOptions<WorkflowManagerOptions> configuration,
            IServiceScopeFactory serviceScopeFactory,
            IEventPayloadRecieverService eventPayloadListenerService)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _scope = _serviceScopeFactory.CreateScope();

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

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
            _messageSubscriber.Subscribe(WorkflowRequestRoutingKey, String.Empty, OnWorkflowRequestRecievedCallback);
            _logger.EventSubscription(ServiceName, WorkflowRequestRoutingKey);
        }

        private void OnWorkflowRequestRecievedCallback(MessageReceivedEventArgs eventArgs)
        {
            Task.Run(async () =>
            {
                await _eventPayloadListenerService.RecieveWorkflowPayload(eventArgs);
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
