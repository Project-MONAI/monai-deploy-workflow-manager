using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Interfaces
{
    public interface IWorkflowInstanceService : IPaginatedApi<WorkflowInstance>
    {
        /// <summary>
        /// Gets a workflow instance from the workflow instance repository by Id.
        /// </summary>
        Task<WorkflowInstance> GetByIdAsync(string id);
    }
}
