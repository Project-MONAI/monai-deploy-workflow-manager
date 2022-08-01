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

using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Interfaces
{
    public interface IPayloadService : IPaginatedApi<Payload>
    {
        /// <summary>
        /// Creates a payload and appends patient details.
        /// </summary>
        /// <param name="eventPayload">request event payload to insert.</param>
        Task<Payload?> CreateAsync(WorkflowRequestEvent eventPayload);

        /// <summary>
        /// Gets a payload by id.
        /// </summary>
        /// <param name="payloadId">payload id to retrieve.</param>
        Task<Payload> GetByIdAsync(string payloadId);

        /// <summary>
        /// Gets a list of payloads.
        /// </summary>
        Task<IList<Payload>> GetAllAsync(int? skip = null,
                                         int? limit = null,
                                         string? patientId = "",
                                         string? patientName = "");

        /// <summary>
        /// Updates a payload
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        Task<bool> UpdateWorkflowInstanceIdsAsync(string payloadId, IEnumerable<string> workflowInstances);
    }
}
