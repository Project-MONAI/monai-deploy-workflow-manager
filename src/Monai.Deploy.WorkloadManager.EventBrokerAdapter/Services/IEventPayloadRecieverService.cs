using Monai.Deploy.Messaging.Common;

namespace Monai.Deploy.WorkloadManager.PayloadListener.Services
{
    public interface IEventPayloadRecieverService
    {
        Task RecieveWorkflowPayload(MessageReceivedEventArgs message);
    }
}
