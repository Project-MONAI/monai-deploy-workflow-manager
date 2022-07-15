using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Interfaces
{
    public interface IWorkflowInstanceService : IPaginatedApi<WorkflowInstance>
    {
        /// <summary>
        /// Gets a workflow instance from the workflow instance repository by Id.
        /// </summary>
        Task<WorkflowInstance> GetByIdAsync(string id);

        /// <summary>
        /// Used for filtering status also.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Task<IList<WorkflowInstance>> GetAllAsync(int? skip = null, int? limit = null, Status? status = null);
    }
}
