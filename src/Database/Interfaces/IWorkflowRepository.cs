// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using System.Threading.Tasks;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Database.Interfaces
{
    public interface IWorkflowRepository
    {
        /// <summary>
        /// Get Retrieves a worklow based on an Id.
        /// </summary>
        /// <param name="workflowId">The workflow Id.</param>
        Task<WorkflowRevision> GetByWorkflowIdAsync(string workflowId);

        /// <summary>
        /// Get Retrieves a list of worklows based on a list of Ids.
        /// </summary>
        /// <param name="workflowIds">The workflow Ids.</param>
        Task<IList<WorkflowRevision>> GetByWorkflowsIdsAsync(IEnumerable<string> workflowIds);

        /// <summary>
        /// Get Retrieves a worklow based on an aeTitle.
        /// </summary>
        /// <param name="aeTitle">An aeTitle to retrieve.</param>
        Task<WorkflowRevision> GetByAeTitleAsync(string aeTitle);

        /// <summary>
        /// Get Retrieves a list of worklows based on an aeTitle.
        /// </summary>
        /// <param name="aeTitle">An aeTitle to retrieve workflows for.</param>
        Task<IList<WorkflowRevision>> GetWorkflowsByAeTitleAsync(string aeTitle);

        /// <summary>
        /// Creates a workflow object.
        /// </summary>
        /// <param name="workflow">Workflow object to create.</param>
        Task<string> CreateAsync(Workflow workflow);
    }
}
