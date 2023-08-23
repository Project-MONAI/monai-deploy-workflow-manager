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

using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services
{
    public class TasksService : ITasksService
    {
        private readonly ITasksRepository _tasksRepository;

        public TasksService(ITasksRepository workflowInstanceRepository)
        {
            _tasksRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
        }

        /// <summary>
        /// Gets all running tasks and a total running task count.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<(IList<TaskExecution> Tasks, long Count)> GetAllAsync(int? skip = null, int? limit = null)
            => await _tasksRepository.GetAllAsync(skip, limit);

        /// <summary>
        /// Gets task by WorkflowInstanceId, TaskId, ExecutionId.
        /// </summary>
        /// <param name="workflowInstanceId">Instance Id.</param>
        /// <param name="taskId">Task Id.</param>
        /// <param name="executionId">Execution Id.</param>
        /// <returns></returns>
        public async Task<TaskExecution?> GetTaskAsync(string workflowInstanceId, string taskId, string executionId)
            => await _tasksRepository.GetTaskAsync(workflowInstanceId, taskId, executionId);
    }
}
