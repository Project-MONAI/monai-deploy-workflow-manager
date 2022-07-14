// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Interfaces
{
    public interface IPayloadService
    {
        /// <summary>
        /// Creates a payload and appends patient details.
        /// </summary>
        /// <param name="eventPayload">request event payload to insert.</param>
        Task<bool> CreateAsync(WorkflowRequestEvent eventPayload);

        /// <summary>
        /// Gets a payload by id.
        /// </summary>
        /// <param name="payloadId">payload id to retrieve.</param>
        Task<Payload> GetByIdAsync(string payloadId);

        /// <summary>
        /// Gets a list of payloads.
        /// </summary>
        Task<IList<Payload>> GetAllAsync(int? skip = null, int? limit = null);

        /// <summary>
        /// Gets Count of objects
        /// </summary>
        /// <returns>the count of objects</returns>
        Task<long> CountAsync();
    }
}
