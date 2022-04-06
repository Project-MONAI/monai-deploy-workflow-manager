using Monai.Deploy.WorkloadManager.Contracts.Rest;

namespace Monai.Deploy.WorkloadManager.Common.Services
{
    public interface IMonaiService
    {
        ServiceStatus Status { get; set; }
        string ServiceName { get; }
    }
}
