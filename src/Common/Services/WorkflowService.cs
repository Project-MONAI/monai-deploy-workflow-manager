// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;

namespace Monai.Deploy.WorkflowManager.Common.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;

        public WorkflowService(IWorkflowRepository workflowRepository)
        {
            _workflowRepository = workflowRepository ?? throw new ArgumentNullException(nameof(workflowRepository));
        }

        public List<WorkflowRevision> GetList() => _workflowRepository.GetWorkflowsList();

        public async Task<WorkflowRevision> GetAsync(string id)
        {
            Guard.Against.NullOrWhiteSpace(id);

            var workflow = await _workflowRepository.GetByWorkflowIdAsync(id);

            return workflow;
        }

        public async Task<string> CreateAsync(Workflow workflow)
        {
            Guard.Against.Null(workflow);

            return await _workflowRepository.CreateAsync(workflow);
        }

        public async Task<string?> UpdateAsync(Workflow workflow, string id)
        {
            Guard.Against.Null(workflow);
            Guard.Against.NullOrWhiteSpace(id);

            var existingWorkflow = await _workflowRepository.GetByWorkflowIdAsync(id);

            if (existingWorkflow is null)
            {
                return null;
            }

            return await _workflowRepository.UpdateAsync(workflow, existingWorkflow);
        }
    }
}
