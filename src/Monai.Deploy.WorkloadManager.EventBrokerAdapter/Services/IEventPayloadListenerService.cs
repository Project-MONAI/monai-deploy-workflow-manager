using Monai.Deploy.MessageBroker.Common;

namespace Monai.Deploy.WorkloadManager.PayloadListener.Services
{
    public interface IEventPayloadListenerService
    {
        Task RecievePayload(MessageReceivedEventArgs message);
    }
}
