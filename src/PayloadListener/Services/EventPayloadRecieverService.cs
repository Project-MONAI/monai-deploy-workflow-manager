using Monai.Deploy.Messaging;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using Monai.Deploy.WorkflowManager.PayloadListener.Validators;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Services
{
    public class EventPayloadRecieverService : IEventPayloadRecieverService
    {
        public EventPayloadRecieverService(
            ILogger<EventPayloadRecieverService> logger,
            IEventPayloadValidator payloadValidator,
            IMessageBrokerSubscriberService messageBrokerSubscriberService)
        {
            Logger = logger;
            PayloadValidator = payloadValidator;
            _messageSubscriber = messageBrokerSubscriberService;
        }

        private IEventPayloadValidator PayloadValidator { get; }

        private ILogger<EventPayloadRecieverService> Logger { get; }

        private readonly IMessageBrokerSubscriberService _messageSubscriber;

        public async Task RecieveWorkflowPayload(MessageReceivedEventArgs message)
        {
            try
            {
                var payload = message.Message.ConvertTo<WorkflowRequestMessage>();

                var validation = PayloadValidator.ValidateWorkflowRequest(payload);

                if (!validation)
                {
                    Logger.EventRejectedNoQueue(message.Message.MessageId);
                    _messageSubscriber.Reject(message.Message);

                    return;
                }

                //Workflow executor called here
                _messageSubscriber.Acknowledge(message.Message);
            }
            catch (Exception e)
            {
                Logger.Exception("Failed to serialze WorkflowRequestMessage", e);
                Logger.EventRejectedRequeue(message.Message.MessageId);
                _messageSubscriber.Reject(message.Message);
            }
        }
    }
}
