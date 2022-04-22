using System.Collections.Generic;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using System.Threading.Tasks;

namespace Monai.Deploy.WorkflowManager.Database.Interfaces
{
    public interface IWorkflowInstanceRepository
    {

        Task<bool> CreateAsync(IEnumerable<WorkflowInstance> workflows);
    }
}
