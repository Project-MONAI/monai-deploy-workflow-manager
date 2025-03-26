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
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Common.Database.Interfaces
{
    public interface IWorkflowInstanceRepository
    {
        /// <summary>
        /// Gets a list of workflow instances.
        /// </summary>
        Task<IList<WorkflowInstance>> GetListAsync();

        /// <summary>
        /// Creates a workflow instance in the database.
        /// </summary>
        /// <param name="workflowInstances">A list of workflowInstances to create.</param>
        Task<bool> CreateAsync(IList<WorkflowInstance> workflowInstances);

        /// <summary>
        /// Gets a workflow instance for a given workflowInstanceId.
        /// </summary>
        /// <param name="workflowInstanceId">A Workflow Instance Id to retrieve.</param>
        Task<WorkflowInstance> GetByWorkflowInstanceIdAsync(string workflowInstanceId);

        /// <summary>
        /// Gets count of Workflow Instances.
        /// </summary>
        /// <returns></returns>
        Task<long> CountAsync(FilterDefinition<WorkflowInstance> filter);

        /// <summary>
        /// Gets the count of workflow instances with a filter.
        /// </summary>
        /// <returns></returns>
        Task<long> FilteredCountAsync(Status? status = null, string? payloadId = null);

        /// <summary>
        /// Gets a list of workflow instances for a given set of workflowIds.
        /// </summary>
        /// <param name="workflowIds">A list of workflowIds to retrieve.</param>
        Task<IList<WorkflowInstance>> GetByWorkflowsIdsAsync(List<string> workflowIds);

        /// <summary>
        /// Gets a list of workflow instances for a given set of payloadIds.
        /// </summary>
        /// <param name="workflowIds">A list of workflowIds to retrieve.</param>
        Task<IList<WorkflowInstance>> GetByPayloadIdsAsync(List<string> workflowIds);

        /// <summary>
        /// Gets All Workflow Instance.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IList<WorkflowInstance>> GetAllAsync(int? skip, int? limit, Status? status, string? payloadId);

        /// <summary>
        /// Updates the Task Status for a given task within a workflow instance.
        /// </summary>
        /// <param name="workflowInstanceId">Workflow Instance to update.</param>
        /// <param name="taskId">TaskId to update.</param>
        /// <param name="status">Status to set.</param>
        Task<bool> UpdateTaskStatusAsync(string workflowInstanceId, string taskId, TaskExecutionStatus status);

        /// <summary>
        /// Updates the Task output artifacts for a given task within a workflow instance.
        /// </summary>
        /// <param name="workflowInstanceId">Workflow Instance to update.</param>
        /// <param name="taskId">TaskId to update.</param>
        /// <param name="outputArtifacts">Output artifacts to set.</param>
        Task<bool> UpdateTaskOutputArtifactsAsync(string workflowInstanceId, string taskId, Dictionary<string, string> outputArtifacts);

        /// <summary>
        /// Gets a task execution for a given workflow instance id and task id.
        /// </summary>
        /// <param name="workflowInstanceId">A Workflow Instance Id to retrieve a task from.</param>
        /// <param name="taskId">A Task Id to retrieve.</param>
        Task<TaskExecution?> GetTaskByIdAsync(string workflowInstanceId, string taskId);

        /// <summary>
        /// Updates the Task list for a given workflow instance.
        /// </summary>
        /// <param name="workflowInstanceId">Workflow Instance to update.</param>
        /// <param name="tasks">List of takss to update.</param>
        Task<bool> UpdateTasksAsync(string workflowInstanceId, List<TaskExecution> tasks);

        /// <summary>
        /// Updates the Task for a given task id within a specified workflow instance.
        /// </summary>
        /// <param name="workflowInstanceId">Workflow Instance to update.</param>
        /// <param name="taskId">A task to update.</param>
        /// <param name="task">Task to set</param>
        Task<bool> UpdateTaskAsync(string workflowInstanceId, string taskId, TaskExecution task);

        /// <summary>
        /// Updates the Status for a given workflow instance.
        /// </summary>
        /// <param name="workflowInstanceId">Workflow Instance to update.</param>
        /// <param name="status">Status to set.</param>
        Task<bool> UpdateWorkflowInstanceStatusAsync(string workflowInstanceId, Status status);

        /// <summary>
        /// Get all failed workflow instance's.
        /// </summary>
        Task<IList<WorkflowInstance>> GetAllFailedAsync();

        /// <summary>
        /// Acknowledges a workflowinstance error.
        /// </summary>
        /// <param name="workflowInstanceId">The Workflow Instance Id.</param>
        /// <returns>An updated workflow.</returns>
        Task<WorkflowInstance> AcknowledgeWorkflowInstanceErrors(string workflowInstanceId);

        /// <summary>
        /// Acknowledges a task error.
        /// </summary>
        /// <param name="workflowInstanceId">The Workflow Instance Id.</param>
        /// <param name="executionId">The Execution Id.</param>
        /// <returns>An updated workflow.</returns>
        Task<WorkflowInstance> AcknowledgeTaskError(string workflowInstanceId, string executionId);

        /// <summary>
        /// Updates the result metadata for an export complete task.
        /// </summary>
        /// <param name="workflowInstanceId">The Workflow Instance Id.</param>
        /// <param name="executionId">The Execution Id.</param>
        /// <param name="fileStatuses">The file statuses to set.</param>
        /// <returns>a Success value.</returns>
        Task<bool> UpdateExportCompleteMetadataAsync(string workflowInstanceId, string executionId, Dictionary<string, object> fileStatuses);
    }
}
