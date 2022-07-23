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

        public Task<DateTime> DeleteWorkflowAsync(WorkflowRevision workflow)
        {
            Guard.Against.Null(workflow);
            return _workflowRepository.SoftDeleteWorkflow(workflow);
        }

        public async Task<long> CountAsync() => await _workflowRepository.CountAsync();

        public async Task<IList<WorkflowRevision>> GetAllAsync(int? skip = null, int? limit = null)
            => await _workflowRepository.GetAllAsync(skip, limit);
    }
}
