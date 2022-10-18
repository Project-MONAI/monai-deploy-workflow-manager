using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Interfaces
{
    internal interface IWorkflowValidator
    {
        void Reset();

        Task<(List<string> Errors, List<string> SuccessfulPaths)> ValidateWorkflow(Workflow workflow);
    }
}
