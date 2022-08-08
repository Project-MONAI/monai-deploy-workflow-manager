/*
 * Copyright 2021-2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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

        public async Task<IList<WorkflowInstance>> GetAllAsync(int? skip = null, int? limit = null, Status? status = null)
            => await _workflowInstanceRepository.GetAllAsync(skip, limit, status);

        public async Task<IList<WorkflowInstance>> GetAllAsync(int? skip = null, int? limit = null)
            => await _workflowInstanceRepository.GetAllAsync(skip, limit, null);
    }
}
