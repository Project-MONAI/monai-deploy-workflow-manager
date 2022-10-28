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
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Exceptions;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Logging;

namespace Monai.Deploy.WorkflowManager.Common.Services
{
    public class WorkflowInstanceService : IWorkflowInstanceService, IPaginatedApi<WorkflowInstance>
    {
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        private readonly ILogger<WorkflowInstanceService> _logger;

        public WorkflowInstanceService(IWorkflowInstanceRepository workflowInstanceRepository, ILogger<WorkflowInstanceService> logger)
        {
            _workflowInstanceRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<WorkflowInstance> GetByIdAsync(string id)
        {
            Guard.Against.NullOrWhiteSpace(id);

            return await _workflowInstanceRepository.GetByWorkflowInstanceIdAsync(id);
        }

        public async Task<WorkflowInstance> AcknowledgeTaskError(string workflowInstanceId, string executionId)
        {
            Guard.Against.NullOrWhiteSpace(workflowInstanceId);
            Guard.Against.NullOrWhiteSpace(executionId);

            var workflowInstance = await _workflowInstanceRepository.GetByWorkflowInstanceIdAsync(workflowInstanceId);

            if (workflowInstance is null || workflowInstance.Tasks.FirstOrDefault(t => t.ExecutionId == executionId) is null)
            {
                throw new MonaiNotFoundException($"WorkflowInstance or task execution not found for workflowInstanceId: {workflowInstanceId}, executionId: {executionId}");
            }

            if (workflowInstance.Status != Status.Failed || workflowInstance.Tasks.First(t => t.ExecutionId == executionId).Status != TaskExecutionStatus.Failed)
            {
                throw new MonaiBadRequestException($"WorkflowInstance status or task execution status is not failed for workflowInstanceId: {workflowInstanceId}, executionId: {executionId}");
            }

            var updatedInstance = await _workflowInstanceRepository.AcknowledgeTaskError(workflowInstanceId, executionId);
            _logger.AckowledgedTaskError();
            var failedTasks = updatedInstance.Tasks.Where(t => t.Status == TaskExecutionStatus.Failed);

            if (failedTasks.Any() && failedTasks.All(t => t.AcknowledgedTaskErrors != null))
            {
                var results = await _workflowInstanceRepository.AcknowledgeWorkflowInstanceErrors(workflowInstanceId);
                _logger.AckowledgedWorkflowInstanceErrors();
                return results;
            }

            return updatedInstance;
        }

        public async Task<long> CountAsync() => await _workflowInstanceRepository.CountAsync();

        public async Task<IList<WorkflowInstance>> GetAllAsync(int? skip = null, int? limit = null, Status? status = null, string? payloadId = null)
            => await _workflowInstanceRepository.GetAllAsync(skip, limit, status, payloadId);

        public async Task<IList<WorkflowInstance>> GetAllAsync(int? skip = null, int? limit = null)
            => await _workflowInstanceRepository.GetAllAsync(skip, limit, null, null);

        public async Task<long> FilteredCountAsync(Status? status = null, string? payloadId = null)
            => await _workflowInstanceRepository.FilteredCountAsync(status, payloadId);

        public async Task<IList<WorkflowInstance>> GetAllFailedAsync(DateTime dateTime)
            => await _workflowInstanceRepository.GetAllFailedAsync(dateTime);
    }
}
