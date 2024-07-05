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

        // <summary>
        // Retrieves a workflow based on a name.
        // </summary>
        /// <param name="name">The workflow name</param>
        Task<WorkflowRevision> GetByWorkflowNameAsync(string name);

        /// <summary>
        /// Retrieves a workflow based on an aeTitle.
        /// </summary>
        /// <param name="aeTitle">An aeTitle to retrieve.</param>
        Task<WorkflowRevision> GetByAeTitleAsync(string aeTitle);

        Task<IEnumerable<WorkflowRevision>> GetAllByAeTitleAsync(string aeTitle, int? skip, int? limit);

        /// <summary>
        /// Retrieves a count of workflows based on an aeTitle.
        /// </summary>
        /// <param name="aeTitle"></param>
        /// <returns></returns>
        Task<long> GetCountByAeTitleAsync(string aeTitle);

        /// <summary>
        /// Retrieves a list of workflows based on an aeTitle.
        /// </summary>
        /// <param name="aeTitle">An aeTitle to retrieve workflows for.</param>
        Task<IList<WorkflowRevision>> GetWorkflowsByAeTitleAsync(List<string> aeTitles);

        /// <summary>
        /// Retrieves a list of workflows based..<br/>
        /// if clinical workflow has AET no data origin. => WorkflowRequestEvents received with CalledAET with that AET this workflow (regardless of what the CallingAET is)<br/>
        /// if clinical workflow has AET and data_orgins => only WorkflowRequestEvents with CalledAET with that AET  and CallingAET trigger this workflow.<br/>
        /// </summary>
        /// <example>
        /// If clinical workflow (workflow revision) exists with AET “MONAI” but no data_origins set
        /// Any inbound WorkflowRequestEvents with CalledAET = “MONAI” trigger this workflow (regardless of what the CallingAET is)
        /// 
        /// If clinical workflow (workflow revision) exists with AET “MONAI” and data_origins set as “PACS”
        /// Only inbound WorkflowRequestEvents with CalledAET = “MONAI” and CallingAET = “PACS” trigger this workflow
        /// </example>
        /// <param name="calledAeTitle"></param>
        /// <param name="sallingAeTitle"></param>
        /// <returns></returns>
        Task<IList<WorkflowRevision>> GetWorkflowsForWorkflowRequestAsync(string calledAeTitle, string callingAeTitle);

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

        Task<long> CountAsync(FilterDefinition<WorkflowRevision> filter);

        Task<IList<WorkflowRevision>> GetAllAsync(int? skip, int? limit);

        /// <summary>
        /// Updates a workflow object and creates a new revision.
        /// </summary>
        /// <param name="workflow">Workflow object to create.</param>
        /// <param name="existingWorkflow">Existing Workflow object to update.</param>
        Task<string> UpdateAsync(Workflow workflow, WorkflowRevision existingWorkflow);
    }
}
