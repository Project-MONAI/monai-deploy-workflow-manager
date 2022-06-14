using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Interfaces
{
    public interface IWorkflowInstanceService
    {
        /// <summary>
        /// Gets a list of workflow instances from the workflow instance repository.
        /// </summary>
        Task<IList<WorkflowInstance>> GetListAsync();

        /// <summary>
        /// Gets a workflow instance from the workflow instance repository by Id.
        /// </summary>
        Task<WorkflowInstance> GetByIdAsync(string id);
    }
}
