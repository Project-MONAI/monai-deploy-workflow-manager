using Monai.Deploy.Messaging.Common;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Services
{
    public interface IEventPayloadRecieverService
    {
        /// <summary>
        /// Recieves a workflow message payload and validates it,
        /// either passing it on to the workflow executor or handling the message accordingly.
        /// </summary>
        /// <param name="message">The workflow message event.</param>
        Task RecieveWorkflowPayload(MessageReceivedEventArgs message);
    }
}
