// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    public interface ITaskRunner
    {
        /// <summary>
        /// Executes the task runner plug-in.
        /// </summary>
        /// <returns><code ref="System.Threading.Task">Task</code></returns>
        Task ExecuteTask();

        /// <summary>
        /// Gets the status of a task.
        /// </summary>
        /// <returns><see cref="System.Threading.Tasks.Task{ExecutionStatus}"/></returns>
        Task<ExecutionStatus> GetStatus(string taskId);
    }
}
