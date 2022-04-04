using Monai.Deploy.WorkflowManager.Contracts.Rest;

namespace Monai.Deploy.WorkflowManager.Common.Services
{
    public interface IMonaiService
    {
        ServiceStatus Status { get; set; }
        string ServiceName { get; }
    }
}
