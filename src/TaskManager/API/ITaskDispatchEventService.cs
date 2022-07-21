// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.TaskManager.API.Models;

namespace Monai.Deploy.TaskManager.API
{
    public interface ITaskDispatchEventService
    {
        /// <summary>
        /// Creates a task dispatch event in the database.
        /// </summary>
        /// <param name="taskDispatchEvent">A TaskDispatchEventInfo to create.</param>
        /// <returns>Returns the created TaskDispatchEventInfo.</returns>
        Task<TaskDispatchEventInfo?> CreateAsync(TaskDispatchEventInfo taskDispatchEvent);

        /// <summary>
        /// Retrieves a task dispatch event by the task execution ID.
        /// </summary>
        /// <param name="taskExecutionId">Task execution ID associated with the event</param>
        Task<TaskDispatchEventInfo?> GetByTaskExecutionIdAsync(string taskExecutionId);

        /// <summary>
        /// Deletes a task dispatch event from the database.
        /// </summary>
        /// <param name="taskDispatchEvent">The task dispatch event to be removed.</param>
        /// <returns></returns>
        Task<bool> RemoveAsync(string taskExecutionId);
    }
}
