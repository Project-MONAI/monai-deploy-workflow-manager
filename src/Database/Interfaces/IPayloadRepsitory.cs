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

using System.Collections.Generic;
using System.Threading.Tasks;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Database.Interfaces
{
    public interface IPayloadRepsitory
    {
        /// <summary>
        /// Creates a payload in the database.
        /// </summary>
        /// <param name="payload">A payload to create.</param>
        Task<bool> CreateAsync(Payload payload);

        /// <summary>
        /// Retrieves a list of payloads in the database.
        /// </summary>
        Task<IList<Payload>> GetAllAsync(int? skip = null, int? limit = null, string patientId = "", string patientName = "");

        /// <summary>
        /// Retrieves a payload by id in the database.
        /// </summary>
        /// <param name="payloadId">A payloadId to retrieve.</param>
        Task<Payload> GetByIdAsync(string payloadId);

        /// <summary>
        /// Gets count of objects
        /// </summary>
        /// <returns>Count of objects.</returns>
        Task<long> CountAsync();

        /// Updates a payload in the database.
        /// </summary>
        /// <param name="payloadId"></param>
        /// <param name="workflowInstances"></param>
        /// <returns></returns>
        Task<bool> UpdateAssociatedWorkflowInstancesAsync(string payloadId, IEnumerable<string> workflowInstances);
    }
}
