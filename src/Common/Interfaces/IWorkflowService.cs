// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Interfaces
{
    public interface IWorkflowService
    {
        /// <summary>
        /// Gets a list of the latest workflow revisions from the workflow repository.
        /// </summary>
        List<WorkflowRevision> GetList();

        /// <summary>
        /// Gets a workflow from the workflow repository.
        /// </summary>
        /// <param name="id">Id used to retrieve a Workflow.</param>
        Task<WorkflowRevision> GetAsync(string id);

        /// <summary>
        /// Creates a workflow within the workflow repository.
        /// </summary>
        /// <param name="workflow">Workflow to create.</param>
        Task<string> CreateAsync(Workflow workflow);

        /// <summary>
        /// Updates a workflow within the workflow repository.
        /// </summary>
        /// <param name="workflow">Workflow to Update.</param>
        /// <param name="id">Id of the workflow to Update.</param>
        Task<string?> UpdateAsync(Workflow workflow, string id);
    }
}
