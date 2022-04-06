using Monai.Deploy.Messaging.Common;

namespace Monai.Deploy.WorkloadManager.PayloadListener.Services
{
    public interface IEventPayloadListenerService
    {
        Task RecieveWorkflowPayload(MessageReceivedEventArgs message);
    }
}
