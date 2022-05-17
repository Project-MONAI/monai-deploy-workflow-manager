// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    public interface ITaskPlugin : IDisposable
    {
        /// <summary>
        /// Executes the task runner plug-in.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="Task{ExecutionStatus}"/>.</returns>
        Task<ExecutionStatus> ExecuteTask(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the status of a task.
        /// </summary>
        /// <param name="identity">The identity for locating the task.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="Task{ExecutionStatus}"/></returns>
        Task<ExecutionStatus> GetStatus(string identity, CancellationToken cancellationToken = default);
    }
}
