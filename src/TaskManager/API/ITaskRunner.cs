// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    public interface ITaskRunner
    {
        /// <summary>
        /// Executes the task runner plug-in.
        /// </summary>
        /// <returns><see cref="Task{ExecutionStatus}"/>.</returns>
        Task<ExecutionStatus> ExecuteTask();

        /// <summary>
        /// Gets the status of a task.
        /// </summary>
        /// <param name="identity">The identity for locating the task.</param>
        /// <returns><see cref="Task{ExecutionStatus}"/></returns>
        Task<ExecutionStatus> GetStatus(string identity);
    }
}
