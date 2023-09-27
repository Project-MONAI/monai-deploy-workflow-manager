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

using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces
{
    public interface IWorkflowService : IPaginatedApi<WorkflowRevision>
    {
        /// <summary>
        /// Gets a workflow from the workflow repository.
        /// </summary>
        /// <param name="id">Id used to retrieve a Workflow.</param>
        Task<WorkflowRevision> GetAsync(string id);

        /// <summary>
        /// Gets a workflow from the workflow repository using the name.
        /// </summary>
        /// <param name="name">The name used to retrieve a worklfow.</param>
        Task<WorkflowRevision> GetByNameAsync(string name);

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
        Task<string?> UpdateAsync(Workflow workflow, string id, bool isUpdateToWorkflowName = false);

        /// <summary>
        /// Soft deletes a given workflow and all revisions
        /// </summary>
        /// <param name="workflow">Workflow to delete.</param>
        Task<DateTime> DeleteWorkflowAsync(WorkflowRevision workflow);

        /// <summary>
        /// get all workflows with AeTitle
        /// </summary>
        /// <param name="aeTitle">the title to get</param>
        /// <param name="skip">skip x num of records</param>
        /// <param name="limit">limit to x number</param>
        /// <returns></returns>
        Task<IEnumerable<WorkflowRevision>> GetByAeTitleAsync(string aeTitle, int? skip = null, int? limit = null);

        /// <summary>
        /// returns the number of workflows with this aetitle
        /// </summary>
        /// <param name="aeTitle">the title to count</param>
        /// <returns></returns>
        Task<long> GetCountByAeTitleAsync(string aeTitle);
    }
}
