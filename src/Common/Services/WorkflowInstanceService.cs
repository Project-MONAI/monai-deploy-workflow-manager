// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Services
{
    public class WorkflowInstanceService : IWorkflowInstanceService, IPaginatedApi<WorkflowInstance>
    {
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;

        public WorkflowInstanceService(IWorkflowInstanceRepository workflowInstanceRepository)
        {
            _workflowInstanceRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
        }

        public async Task<WorkflowInstance> GetByIdAsync(string id)
        {
            Guard.Against.NullOrWhiteSpace(id);

            return await _workflowInstanceRepository.GetByWorkflowInstanceIdAsync(id);
        }

        public async Task<long> CountAsync() => await _workflowInstanceRepository.CountAsync();

        public async Task<IList<WorkflowInstance>> GetAllAsync(int? skip = null, int? limit = null) => await _workflowInstanceRepository.GetAllAsync(skip, limit);
    }
}
