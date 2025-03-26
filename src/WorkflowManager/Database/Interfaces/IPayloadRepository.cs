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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Common.Database.Interfaces
{
    public interface IPayloadRepository
    {
        /// <summary>
        /// Creates a payload in the database.
        /// </summary>
        /// <param name="payload">A payload to create.</param>
        Task<bool> CreateAsync(Payload payload);

        /// <summary>
        /// Retrieves a list of payloads in the database.
        /// </summary>
        Task<IList<Payload>> GetAllAsync(int? skip = null, int? limit = null, string? patientId = null, string? patientName = null, string? accessionId = null);

        /// <summary>
        /// Retrieves a payload by id in the database.
        /// </summary>
        /// <param name="payloadId">A payloadId to retrieve.</param>
        Task<Payload> GetByIdAsync(string payloadId);

        /// <summary>
        /// Gets count of objects
        /// </summary>
        /// <returns>Count of objects.</returns>
        Task<long> CountAsync(FilterDefinition<Payload> filter);

        /// <summary>
        /// Updates a payload in the database.
        /// </summary>
        /// <param name="payload">The payload to update.</param>
        /// <returns>The updated payload.</returns>
        Task<bool> UpdateAsyncWorkflowIds(Payload payload);

        /// <summary>
        /// Gets all the payloads that might need deleted
        /// </summary>
        /// <param name="now">the current datetime</param>
        /// <returns></returns>
        Task<IList<Payload>> GetPayloadsToDelete(DateTime now);

        /// <summary>
        /// Marks a bunch of payloads as a new deleted state
        /// </summary>
        /// <param name="Ids">a list of payloadIds to mark in new status</param>
        /// <param name="status">the status to mark as</param>
        /// <returns></returns>
        Task MarkDeletedState(IList<string> Ids, PayloadDeleted status);
    }
}
