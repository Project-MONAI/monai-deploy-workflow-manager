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
        Task<long> CountAsync();
        Task<IList<WorkflowRevision>> GetAllAsync(int? skip, int? limit);

        /// <summary>
        /// Updates a workflow object and creates a new revision.
        /// </summary>
        /// <param name="workflow">Workflow object to create.</param>
        /// <param name="existingWorkflow">Existing Workflow object to update.</param>
        Task<string> UpdateAsync(Workflow workflow, WorkflowRevision existingWorkflow);
    }
}
