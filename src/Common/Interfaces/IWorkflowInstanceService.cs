using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Interfaces
{
    public interface IWorkflowInstanceService
    {
        /// <summary>
        /// Gets a list of workflow instances from the workflow instance repository.
        /// </summary>
        Task<IList<WorkflowInstance>> GetListAsync();
    }
}
