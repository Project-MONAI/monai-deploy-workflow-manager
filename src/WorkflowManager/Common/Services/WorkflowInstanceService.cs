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

using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Exceptions;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Logging;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services
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
            ArgumentNullException.ThrowIfNullOrWhiteSpace(id, nameof(id));

            return await _workflowInstanceRepository.GetByWorkflowInstanceIdAsync(id);
        }

        public async Task<WorkflowInstance> AcknowledgeTaskError(string workflowInstanceId, string executionId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(executionId, nameof(executionId));

            var workflowInstance = await _workflowInstanceRepository.GetByWorkflowInstanceIdAsync(workflowInstanceId);

            if (workflowInstance is null || workflowInstance.Tasks.FirstOrDefault(t => t.ExecutionId == executionId) is null)
            {
                throw new MonaiNotFoundException($"WorkflowInstance or task execution not found for workflowInstanceId: {workflowInstanceId}, executionId: {executionId}");
            }

            var task = workflowInstance.Tasks.First(t => t.ExecutionId == executionId);

            if ((task.Status != TaskExecutionStatus.Failed && task.Status != TaskExecutionStatus.PartialFail)
                || (workflowInstance.Status != Status.Failed && workflowInstance.Status != Status.Succeeded))
            {
                throw new MonaiBadRequestException($"WorkflowInstance status or task execution status is not failed for workflowInstanceId: {workflowInstanceId}, executionId: {executionId}");
            }

            var updatedInstance = await _workflowInstanceRepository.AcknowledgeTaskError(workflowInstanceId, executionId);
            _logger.AckowledgedTaskError();
            var failedTasks = updatedInstance.Tasks.Where(t => t.Status == TaskExecutionStatus.Failed || t.Status == TaskExecutionStatus.PartialFail);

            if (failedTasks.Any() && failedTasks.All(t => t.AcknowledgedTaskErrors != null))
            {
                var results = await _workflowInstanceRepository.AcknowledgeWorkflowInstanceErrors(workflowInstanceId);
                _logger.AckowledgedWorkflowInstanceErrors();
                return results;
            }

            return updatedInstance;
        }

        public async Task UpdateExportCompleteMetadataAsync(string workflowInstanceId, string executionId, Dictionary<string, FileExportStatus> fileStatuses)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(executionId, nameof(executionId));

            var resultMetadata = fileStatuses.ToDictionary(f => f.Key, f => f.Value.ToString() as object);

            await _workflowInstanceRepository.UpdateExportCompleteMetadataAsync(workflowInstanceId, executionId, resultMetadata);
        }

        public async Task<long> CountAsync(FilterDefinition<WorkflowInstance>? filter)
        {
            filter = filter ?? Builders<WorkflowInstance>.Filter.Empty;
            return await _workflowInstanceRepository.CountAsync(filter);
        }

        public async Task<IList<WorkflowInstance>> GetAllAsync(int? skip = null, int? limit = null, Status? status = null, string? payloadId = null)
            => await _workflowInstanceRepository.GetAllAsync(skip, limit, status, payloadId);

        public async Task<IList<WorkflowInstance>> GetAllAsync(int? skip = null, int? limit = null)
            => await _workflowInstanceRepository.GetAllAsync(skip, limit, null, null);

        public async Task<long> FilteredCountAsync(Status? status = null, string? payloadId = null)
            => await _workflowInstanceRepository.FilteredCountAsync(status, payloadId);

        public async Task<IList<WorkflowInstance>> GetAllFailedAsync()
            => await _workflowInstanceRepository.GetAllFailedAsync();
    }
}
