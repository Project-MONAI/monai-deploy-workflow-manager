// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Services
{
    public class WorkflowInstanceService : IWorkflowInstanceService
    {
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;

        public WorkflowInstanceService(IWorkflowInstanceRepository workflowInstanceRepository)
        {
            _workflowInstanceRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
        }

        public async Task<IList<WorkflowInstance>> GetListAsync() => await _workflowInstanceRepository.GetListAsync();

        public async Task<WorkflowInstance> GetByIdAsync(string id)
        {
            Guard.Against.NullOrWhiteSpace(id);

            return await _workflowInstanceRepository.GetByWorkflowInstanceIdAsync(id);
        }
    }
}
