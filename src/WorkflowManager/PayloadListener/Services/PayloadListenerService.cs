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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Logging;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Monai.Deploy.WorkflowManager.Logging;

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
        public string ArtifactRecievedRoutingKey { get; set; }
        public string ExportHL7CompleteRoutingKey { get; set; }
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
            ArtifactRecievedRoutingKey = configuration.Value.Messaging.Topics.ArtifactRecieved;
            ExportHL7CompleteRoutingKey = configuration.Value.Messaging.Topics.ExportHL7Complete;

            Concurrency = 2;

            _eventPayloadListenerService = eventPayloadListenerService ?? throw new ArgumentNullException(nameof(eventPayloadListenerService));

            _messageSubscriber = _scope.ServiceProvider.GetRequiredService<IMessageBrokerSubscriberService>();
            _messageSubscriber.OnConnectionError += (sender, args) =>
            {
                _logger.MessagingServiceErrorRecover(args.ErrorMessage);
                SetupPolling();
            };
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
            _messageSubscriber.SubscribeAsync(WorkflowRequestRoutingKey, WorkflowRequestRoutingKey, OnWorkflowRequestReceivedCallbackAsync);
            _logger.EventSubscription(ServiceName, WorkflowRequestRoutingKey);

            _messageSubscriber.SubscribeAsync(TaskStatusUpdateRoutingKey, TaskStatusUpdateRoutingKey, OnTaskUpdateStatusReceivedCallback);
            _logger.EventSubscription(ServiceName, TaskStatusUpdateRoutingKey);

            _messageSubscriber.SubscribeAsync(ExportCompleteRoutingKey, ExportCompleteRoutingKey, OnExportCompleteReceivedCallback);
            _logger.EventSubscription(ServiceName, ExportCompleteRoutingKey);

            _messageSubscriber.SubscribeAsync(ArtifactRecievedRoutingKey, ArtifactRecievedRoutingKey, OnArtifactReceivedtReceivedCallbackAsync);
            _logger.EventSubscription(ServiceName, ArtifactRecievedRoutingKey);

            _messageSubscriber.SubscribeAsync(ExportHL7CompleteRoutingKey, ExportHL7CompleteRoutingKey, OnExportHL7CompleteReceivedCallback);
            _logger.EventSubscription(ServiceName, ExportHL7CompleteRoutingKey);
        }

        private async Task OnWorkflowRequestReceivedCallbackAsync(MessageReceivedEventArgs eventArgs)
        {

            using var loggerScope = _logger.BeginScope(new Common.Miscellaneous.LoggingDataDictionary<string, object>
            {
                ["correlationId"] = eventArgs.Message.CorrelationId,
                ["source"] = eventArgs.Message.ApplicationId,
                ["messageId"] = eventArgs.Message.MessageId,
                ["messageDescription"] = eventArgs.Message.MessageDescription,
            });

            _logger.WorkflowRequestReceived();
            await _eventPayloadListenerService.ReceiveWorkflowPayload(eventArgs);
        }

        private async Task OnTaskUpdateStatusReceivedCallback(MessageReceivedEventArgs eventArgs)
        {
            using var loggerScope = _logger.BeginScope(new Common.Miscellaneous.LoggingDataDictionary<string, object>
            {
                ["correlationId"] = eventArgs.Message.CorrelationId,
                ["source"] = eventArgs.Message.ApplicationId,
                ["messageId"] = eventArgs.Message.MessageId,
                ["messageDescription"] = eventArgs.Message.MessageDescription,
            });

            _logger.TaskUpdateReceived();
            await _eventPayloadListenerService.TaskUpdatePayload(eventArgs);
        }

        private async Task OnExportCompleteReceivedCallback(MessageReceivedEventArgs eventArgs)
        {
            using var loggerScope = _logger.BeginScope(new Common.Miscellaneous.LoggingDataDictionary<string, object>
            {
                ["correlationId"] = eventArgs.Message.CorrelationId,
                ["source"] = eventArgs.Message.ApplicationId,
                ["messageId"] = eventArgs.Message.MessageId,
                ["messageDescription"] = eventArgs.Message.MessageDescription,
            });

            _logger.ExportCompleteReceived();
            await _eventPayloadListenerService.ExportCompletePayload(eventArgs);

        }

        private async Task OnExportHL7CompleteReceivedCallback(MessageReceivedEventArgs eventArgs)
        {
            using var loggerScope = _logger.BeginScope(new Common.Miscellaneous.LoggingDataDictionary<string, object>
            {
                ["correlationId"] = eventArgs.Message.CorrelationId,
                ["source"] = eventArgs.Message.ApplicationId,
                ["messageId"] = eventArgs.Message.MessageId,
                ["messageDescription"] = eventArgs.Message.MessageDescription,
            });

            _logger.ExportCompleteReceived();
            await _eventPayloadListenerService.ExportCompletePayload(eventArgs);

        }

        private async Task OnArtifactReceivedtReceivedCallbackAsync(MessageReceivedEventArgs eventArgs)
        {

            using var loggerScope = _logger.BeginScope(new Common.Miscellaneous.LoggingDataDictionary<string, object>
            {
                ["correlationId"] = eventArgs.Message.CorrelationId,
                ["source"] = eventArgs.Message.ApplicationId,
                ["messageId"] = eventArgs.Message.MessageId,
                ["messageDescription"] = eventArgs.Message.MessageDescription,
            });

            _logger.ArtifactReceivedReceived();
            await _eventPayloadListenerService.ArtifactReceivePayload(eventArgs);
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
