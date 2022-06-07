// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using System.Threading.Tasks;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Database.Interfaces
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
        /// Gets a list of workflow instances for a given set of workflowIds.
        /// </summary>
        /// <param name="workflowIds">A list of workflowIds to retrieve.</param>
        Task<IList<WorkflowInstance>> GetByWorkflowsIdsAsync(List<string> workflowIds);

        /// <summary>
        /// Updates the Task Status for a given task within a workflow instance.
        /// </summary>
        /// <param name="workflowInstanceId">Workflow Instance to update.</param>
        /// <param name="taskId">TaskId to update.</param>
        /// <param name="status">Status to set.</param>
        Task<bool> UpdateTaskStatusAsync(string workflowInstanceId, string taskId, TaskExecutionStatus status);

        /// <summary>
        /// Updates the Task list for a given workflow instance.
        /// </summary>
        /// <param name="workflowInstanceId">Workflow Instance to update.</param>
        /// <param name="tasks">List of takss to update.</param>
        Task<bool> UpdateTasksAsync(string workflowInstanceId, List<TaskExecution> tasks);

        /// <summary>
        /// Updates the Status for a given workflow instance.
        /// </summary>
        /// <param name="workflowInstanceId">Workflow Instance to update.</param>
        /// <param name="status">Status to set.</param>
        Task<bool> UpdateWorkflowInstanceStatusAsync(string workflowInstanceId, Status status);
    }
}
