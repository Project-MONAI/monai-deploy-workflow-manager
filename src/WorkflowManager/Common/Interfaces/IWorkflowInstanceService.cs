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

using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces
{
    public interface IWorkflowInstanceService : IPaginatedApi<WorkflowInstance>
    {
        /// <summary>
        /// Gets a workflow instance from the workflow instance repository by Id.
        /// </summary>
        public Task<WorkflowInstance> GetByIdAsync(string id);

        /// <summary>
        /// Acknowledges a task error and acknowledges a workflow if all tasks are acknowledged.
        /// </summary>
        /// <param name="workflowInstanceId">The Workflow Instance Id.</param>
        /// <param name="executionId">The Task Execution Id.</param>
        /// <returns>An updated workflow.</returns>
        public Task<WorkflowInstance> AcknowledgeTaskError(string workflowInstanceId, string executionId);

        /// <summary>
        /// Used for filtering status also.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <param name="status"></param>
        /// <param name="payloadId"></param>
        /// <returns></returns>
        public Task<IList<WorkflowInstance>> GetAllAsync(int? skip = null, int? limit = null, Status? status = null, string? payloadId = null);

        /// <summary>
        /// Used to get a filtered count.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="payloadId"></param>
        /// <returns></returns>
        public Task<long> FilteredCountAsync(Status? status = null, string? payloadId = null);

        /// <summary>
        /// Get all failed workflow instance's.
        /// </summary>
        Task<IList<WorkflowInstance>> GetAllFailedAsync();

        Task UpdateExportCompleteMetadataAsync(string workflowInstanceId, string executionId, Dictionary<string, FileExportStatus> fileStatuses);
    }
}
