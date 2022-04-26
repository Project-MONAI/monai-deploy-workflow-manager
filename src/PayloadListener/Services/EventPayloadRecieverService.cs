// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using Monai.Deploy.WorkflowManager.PayloadListener.Validators;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Services;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Services
{
    public class EventPayloadRecieverService : IEventPayloadRecieverService
    {
        public EventPayloadRecieverService(
            ILogger<EventPayloadRecieverService> logger,
            IEventPayloadValidator payloadValidator,
            IMessageBrokerSubscriberService messageBrokerSubscriberService,
            IWorkflowExecuterService workflowExecuterService)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            PayloadValidator = payloadValidator ?? throw new ArgumentNullException(nameof(payloadValidator));
            _messageSubscriber = messageBrokerSubscriberService ?? throw new ArgumentNullException(nameof(messageBrokerSubscriberService));
            WorkflowExecuterService = workflowExecuterService ?? throw new ArgumentNullException(nameof(workflowExecuterService));
        }

        private IEventPayloadValidator PayloadValidator { get; }

        private IWorkflowExecuterService WorkflowExecuterService { get; }

        private ILogger<EventPayloadRecieverService> Logger { get; }

        private readonly IMessageBrokerSubscriberService _messageSubscriber;

        public async Task RecieveWorkflowPayload(MessageReceivedEventArgs message)
        {
            try
            {
                var payload = message.Message.ConvertTo<WorkflowRequestEvent>();

                var validation = PayloadValidator.ValidateWorkflowRequest(payload);

                if (!validation)
                {
                    Logger.EventRejectedNoQueue(message.Message.MessageId);
                    _messageSubscriber.Reject(message.Message, false);

                    return;
                }

                WorkflowExecuterService.ProcessPayload(payload);

                _messageSubscriber.Acknowledge(message.Message);
            }
            catch (Exception e)
            {
                Logger.Exception("Failed to serialze WorkflowRequestMessage", e);
                Logger.EventRejectedRequeue(message.Message.MessageId);

                //
                _messageSubscriber.Reject(message.Message, true);
            }
        }
    }
}
