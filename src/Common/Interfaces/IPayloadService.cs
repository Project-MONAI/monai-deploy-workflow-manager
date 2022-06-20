using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Interfaces
{
    public interface IPayloadService
    {
        Task<bool> CreateAsync(WorkflowRequestEvent eventPayload);

        Task<Payload> GeyByIdAsync(string payloadId);

        Task<IList<Payload>> GetAllAsync();
    }
}
