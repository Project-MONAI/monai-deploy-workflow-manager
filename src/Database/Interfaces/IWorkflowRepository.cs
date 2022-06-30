// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Database.Interfaces
{
    public interface IWorkflowRepository
    {
        /// <summary>
        /// Gets a list of the latest workflow revisions.
        /// </summary>
        List<WorkflowRevision> GetWorkflowsList();

        /// <summary>
        /// Retrieves a workflow based on an Id.
        /// </summary>
        /// <param name="workflowId">The workflow Id.</param>
        Task<WorkflowRevision> GetByWorkflowIdAsync(string workflowId);

        /// <summary>
        /// Retrieves a list of workflows based on a list of Ids.
        /// </summary>
        /// <param name="workflowIds">The workflow Ids.</param>
        Task<IList<WorkflowRevision>> GetByWorkflowsIdsAsync(IEnumerable<string> workflowIds);

        /// <summary>
        /// Retrieves a workflow based on an aeTitle.
        /// </summary>
        /// <param name="aeTitle">An aeTitle to retrieve.</param>
        Task<WorkflowRevision> GetByAeTitleAsync(string aeTitle);

        /// <summary>
        /// Retrieves a list of workflows based on an aeTitle.
        /// </summary>
        /// <param name="aeTitle">An aeTitle to retrieve workflows for.</param>
        Task<IList<WorkflowRevision>> GetWorkflowsByAeTitleAsync(string aeTitle);

        /// <summary>
        /// Creates a workflow object.
        /// </summary>
        /// <param name="workflow">Workflow object to create.</param>
        Task<string> CreateAsync(Workflow workflow);

        /// <summary>
        /// Soft deletes all workflow revisions of given workflow.
        /// </summary>
        /// <param name="workflow"></param>
        /// <returns></returns>
        Task<DateTime> SoftDeleteWorkflow(WorkflowRevision workflow);

        /// <summary>
        /// Updates a workflow object and creates a new revision.
        /// </summary>
        /// <param name="workflow">Workflow object to create.</param>
        /// <param name="existingWorkflow">Existing Workflow object to update.</param>
        Task<string> UpdateAsync(Workflow workflow, WorkflowRevision existingWorkflow);
    }
}
