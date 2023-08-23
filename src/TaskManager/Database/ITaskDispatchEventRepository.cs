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

using Monai.Deploy.WorkflowManager.TaskManager.API.Models;

namespace Monai.Deploy.WorkflowManager.TaskManager.Database
{
    public interface ITaskDispatchEventRepository
    {
        /// <summary>
        /// Creates a task dispatch event in the database.
        /// </summary>
        /// <param name="taskDispatchEvent">A TaskDispatchEvent to create.</param>
        /// <returns>Returns the created TaskDispatchEventInfo.</returns>
        Task<TaskDispatchEventInfo?> CreateAsync(TaskDispatchEventInfo taskDispatchEventInfo);

        /// <summary>
        /// Updates user accounts of a task dispatch event in the database.
        /// </summary>
        /// <param name="taskDispatchEvent">A TaskDispatchEvent to update.</param>
        /// <returns>Returns the created TaskDispatchEventInfo.</returns>
        Task<TaskDispatchEventInfo?> UpdateUserAccountsAsync(TaskDispatchEventInfo taskDispatchEventInfo);

        /// <summary>
        /// Retrieves a task dispatch event by the task execution ID.
        /// </summary>
        /// <param name="taskExecutionId">Task execution ID associated with the event</param>
        Task<TaskDispatchEventInfo?> GetByTaskExecutionIdAsync(string taskExecutionId);

        /// <summary>
        /// Deletes a task dispatch event from the database.
        /// </summary>
        /// <param name="taskExecutionId">Task execution ID associated with the event</param>
        /// <returns></returns>
        Task<bool> RemoveAsync(string taskExecutionId);

        /// <summary>
        /// Updates the plugin args of a task dispatch event in the database.
        /// </summary>
        /// <param name="taskDispatchEventInfo">A TaskDispatchEvent to update.</param>
        /// <param name="pluginArgs">The plugin arguments to update.</param>
        /// <returns>The updated TaskDispatchEventInfo.</returns>
        Task<TaskDispatchEventInfo?> UpdateTaskPluginArgsAsync(TaskDispatchEventInfo taskDispatchEventInfo, Dictionary<string, string> pluginArgs);
    }
}
