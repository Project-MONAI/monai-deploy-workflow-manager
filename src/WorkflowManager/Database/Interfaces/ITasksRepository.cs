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
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Database.Interfaces
{
    public interface ITasksRepository
    {
        /// <summary>
        /// Gets all running tasks and total number of running tasks.
        /// </summary>
        /// <param name="skip">skip.</param>
        /// <param name="limit">limit.</param>
        /// <returns></returns>
        Task<(IList<TaskExecution> Tasks, long Count)> GetAllAsync(int? skip, int? limit);

        /// <summary>
        /// Gets Task Execution given workflowInstanceId, taskId and executionId.
        /// </summary>
        /// <param name="workflowInstanceId">workflowInstanceId.</param>
        /// <param name="taskId">taskId.</param>
        /// <param name="executionId">executionId<./param>
        /// <returns></returns>
        Task<TaskExecution?> GetTaskAsync(string workflowInstanceId, string taskId, string executionId);
    }
}
