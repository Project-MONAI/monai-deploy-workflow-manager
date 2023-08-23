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

using Monai.Deploy.Messaging.Events;

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
        Task<ExecutionStatus> GetStatus(string identity, TaskCallbackEvent callbackEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Handle a task timeout when a task never leaves a running state.
        /// </summary>
        /// <returns></returns>
        Task HandleTimeout(string identity);
    }
}
