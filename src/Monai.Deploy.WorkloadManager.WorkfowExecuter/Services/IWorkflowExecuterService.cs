
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Models;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Services
{
    public interface IWorkflowExecuterService
    {
        Task<bool> ProcessPayload(PayloadReceived message);
    }
}
