using System.Collections.Generic;
using System.Threading.Tasks;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Database.Interfaces
{
    public interface IWorkflowRepository
    {
        Task<List<Workflow>> GetListByAeTitleAsync(string aeTitle);

        Task<List<Workflow>> GetByWorkflowsIdsAsync(IEnumerable<string> ids);
    }
}
