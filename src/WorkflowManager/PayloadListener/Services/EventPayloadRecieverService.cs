/*
 * Copyright 2021-2022 MONAI Consortium
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

using Monai.Deploy.Messaging.API;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using Monai.Deploy.WorkflowManager.PayloadListener.Validators;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Services;
using Monai.Deploy.WorkflowManager.Common.Interfaces;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Services
{
    public class EventPayloadReceiverService : IEventPayloadReceiverService
    {
        public EventPayloadReceiverService(
            ILogger<EventPayloadReceiverService> logger,
            IEventPayloadValidator payloadValidator,
            IMessageBrokerSubscriberService messageBrokerSubscriberService,
            IWorkflowExecuterService workflowExecuterService,
            IPayloadService payloadService)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            PayloadValidator = payloadValidator ?? throw new ArgumentNullException(nameof(payloadValidator));
            _messageSubscriber = messageBrokerSubscriberService ?? throw new ArgumentNullException(nameof(messageBrokerSubscriberService));
            WorkflowExecuterService = workflowExecuterService ?? throw new ArgumentNullException(nameof(workflowExecuterService));
            PayloadService = payloadService ?? throw new ArgumentNullException(nameof(payloadService));
        }

        private IEventPayloadValidator PayloadValidator { get; }

        private IWorkflowExecuterService WorkflowExecuterService { get; }

        private IPayloadService PayloadService { get; }

        private ILogger<EventPayloadReceiverService> Logger { get; }

        private readonly IMessageBrokerSubscriberService _messageSubscriber;

        public async Task ReceiveWorkflowPayload(MessageReceivedEventArgs message)
        {
            try
            {
                var requestEvent = message.Message.ConvertTo<WorkflowRequestEvent>();

                var validation = PayloadValidator.ValidateWorkflowRequest(requestEvent);

                if (!validation)
                {
                    Logger.EventRejectedNoQueue(message.Message.MessageId);
                    _messageSubscriber.Reject(message.Message, false);

                    return;
                }

                var payload = await PayloadService.CreateAsync(requestEvent);
                if (payload is null)
                {
                    Logger.EventRejectedRequeue(message.Message.MessageId);
                    await _messageSubscriber.RequeueWithDelay(message.Message);

                    return;
                }

                if (!await WorkflowExecuterService.ProcessPayload(requestEvent, payload))
                {
                    Logger.EventRejectedRequeue(message.Message.MessageId);
                    await _messageSubscriber.RequeueWithDelay(message.Message);

                    return;
                }

                _messageSubscriber.Acknowledge(message.Message);
            }
            catch (Exception e)
            {
                Logger.Exception("Failed to serialize WorkflowRequestMessage", e);
                Logger.EventRejectedRequeue(message.Message.MessageId);

                await _messageSubscriber.RequeueWithDelay(message.Message);
            }
        }

        public async Task TaskUpdatePayload(MessageReceivedEventArgs message)
        {
            try
            {
                var payload = message.Message.ConvertTo<TaskUpdateEvent>();

                if (!PayloadValidator.ValidateTaskUpdate(payload))
                {
                    Logger.EventRejectedNoQueue(message.Message.MessageId);
                    _messageSubscriber.Reject(message.Message, false);

                    return;
                }

                var processTaskUpdateResult = await WorkflowExecuterService.ProcessTaskUpdate(payload);
                if (!processTaskUpdateResult && payload.Reason != FailureReason.TimedOut)
                {
                    Logger.EventRejectedRequeue(message.Message.MessageId);

                    await _messageSubscriber.RequeueWithDelay(message.Message);

                    return;
                }

                _messageSubscriber.Acknowledge(message.Message);
            }
            catch (Exception e)
            {
                Logger.Exception($"Failed to serialize {nameof(TaskUpdateEvent)}", e);
                Logger.EventRejectedRequeue(message.Message.MessageId);

                await _messageSubscriber.RequeueWithDelay(message.Message);
            }
        }

        public async Task ExportCompletePayload(MessageReceivedEventArgs message)
        {
            try
            {
                var payload = message.Message.ConvertTo<ExportCompleteEvent>();

                if (!PayloadValidator.ValidateExportComplete(payload))
                {
                    Logger.EventRejectedNoQueue(message.Message.MessageId);
                    _messageSubscriber.Reject(message.Message, false);

                    return;
                }

                if (!await WorkflowExecuterService.ProcessExportComplete(payload, message.Message.CorrelationId))
                {
                    Logger.EventRejectedRequeue(message.Message.MessageId);

                    await _messageSubscriber.RequeueWithDelay(message.Message);

                    return;
                }

                _messageSubscriber.Acknowledge(message.Message);
            }
            catch (Exception e)
            {
                Logger.Exception($"Failed to serialize {nameof(ExportCompleteEvent)}", e);
                Logger.EventRejectedRequeue(message.Message.MessageId);

                await _messageSubscriber.RequeueWithDelay(message.Message);
            }
        }
    }
}
