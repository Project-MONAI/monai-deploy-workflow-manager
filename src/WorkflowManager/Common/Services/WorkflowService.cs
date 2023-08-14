/*
 * Copyright 2022 MONAI Consortium
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
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Logging;

namespace Monai.Deploy.WorkflowManager.Common.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ILogger<WorkflowService> _logger;

        public WorkflowService(IWorkflowRepository workflowRepository, ILogger<WorkflowService> logger)
        {
            _workflowRepository = workflowRepository ?? throw new ArgumentNullException(nameof(workflowRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<WorkflowRevision> GetAsync(string id)
        {
            Guard.Against.NullOrWhiteSpace(id, nameof(id));

            var workflow = await _workflowRepository.GetByWorkflowIdAsync(id);

            return workflow;
        }

        public async Task<WorkflowRevision> GetByNameAsync(string name)
        {
            Guard.Against.NullOrWhiteSpace(name, nameof(name));

            return await _workflowRepository.GetByWorkflowNameAsync(name);
        }

        public async Task<string> CreateAsync(Workflow workflow)
        {
            Guard.Against.Null(workflow, nameof(workflow));

            var id = await _workflowRepository.CreateAsync(workflow);
            _logger.WorkflowCreated(id, workflow.Name);
            return id;
        }

        public async Task<string?> UpdateAsync(Workflow workflow, string id, bool isUpdateToWorkflowName = false)
        {
            Guard.Against.Null(workflow, nameof(workflow));
            Guard.Against.NullOrWhiteSpace(id, nameof(id));

            var existingWorkflow = await _workflowRepository.GetByWorkflowIdAsync(id);

            if (existingWorkflow is null)
            {
                return null;
            }

            var result = await _workflowRepository.UpdateAsync(workflow, existingWorkflow);
            _logger.WorkflowUpdated(id, workflow.Name);
            return result;
        }

        public Task<DateTime> DeleteWorkflowAsync(WorkflowRevision workflow)
        {
            Guard.Against.Null(workflow, nameof(workflow));
            var result = _workflowRepository.SoftDeleteWorkflow(workflow);
            _logger.WorkflowDeleted(workflow.WorkflowId, workflow.Id, workflow.Workflow?.Name);
            return result;
        }

        public async Task<long> CountAsync() => await _workflowRepository.CountAsync();

        public async Task<IList<WorkflowRevision>> GetAllAsync(int? skip = null, int? limit = null)
            => await _workflowRepository.GetAllAsync(skip, limit);


        public async Task<IEnumerable<WorkflowRevision>> GetByAeTitleAsync(string aeTitle, int? skip = null, int? limit = null)
        => await _workflowRepository.GetAllByAeTitleAsync(aeTitle, skip, limit);

        public async Task<long> GetCountByAeTitleAsync(string aeTitle)
         => await _workflowRepository.GetCountByAeTitleAsync(aeTitle);

    }
}
