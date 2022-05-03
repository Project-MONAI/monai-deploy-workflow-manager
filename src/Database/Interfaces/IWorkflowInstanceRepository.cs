// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using System.Threading.Tasks;

namespace Monai.Deploy.WorkflowManager.Database.Interfaces
{
    public interface IWorkflowInstanceRepository
    {
        /// <summary>
        /// Creates a workflow instance in the database.
        /// </summary>
        /// <param name="workflows">A list of workflows to create.</param>
        Task<bool> CreateAsync(IList<WorkflowInstance> workflows);
    }
}
