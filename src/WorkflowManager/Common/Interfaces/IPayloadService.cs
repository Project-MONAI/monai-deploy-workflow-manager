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
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces
{
    public interface IPayloadService : IPaginatedApi<PayloadDto>
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
        Task<IList<PayloadDto>> GetAllAsync(int? skip = null,
                                         int? limit = null,
                                         string? patientId = null,
                                         string? patientName = null,
                                         string? accessionId = null);
        new Task<IList<PayloadDto>> GetAllAsync(int? skip = null, int? limit = null);

        /// <summary>
        /// Deletes a payload by id.
        /// </summary>
        /// <param name="payloadId">payload id to delete.</param>
        Task<bool> DeletePayloadFromStorageAsync(string payloadId);

        /// <summary>
        /// Gets the expiry date for a payload.
        /// </summary>
        /// <param name="now"></param>
        /// <param name="workflowInstanceId"></param>
        /// <returns>date of expiry or null</returns>
        Task<DateTime?> GetExpiry(DateTime now, string? workflowInstanceId);

        /// <summary>
        /// Updates a payload
        /// </summary>
        /// <param name="payloadId">payload id to update.</param>
        /// <param name="payload">updated payload.</param>
        /// <returns>true if the update is successful, false otherwise.</returns>
        Task<bool> UpdateAsyncWorkflowIds(Payload payload);

        Task<long> CountAsync(FilterDefinition<Payload> filter);
    }
}
