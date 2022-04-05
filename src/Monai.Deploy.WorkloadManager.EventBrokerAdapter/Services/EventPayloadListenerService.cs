using Monai.Deploy.MessageBroker.Common;
using Monai.Deploy.MessageBroker.Messages;
using Monai.Deploy.WorkloadManager.PayloadListener.Validators;

namespace Monai.Deploy.WorkloadManager.PayloadListener.Services
{
    public class EventPayloadListenerService : IEventPayloadListenerService
    {
        public EventPayloadListenerService(IEventPayloadValidator payloadValidator)
        {
            PayloadValidator = payloadValidator;
        }

        IEventPayloadValidator PayloadValidator { get; }

        public async Task RecievePayload(MessageReceivedEventArgs message)
        {
            var payload = message.Message.ConvertTo<WorkflowRequestMessage>();

            var validation = PayloadValidator.ValidateWorkflowRequest(payload);


        }
    }
}
