// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Services
{
    public class TasksService : ITasksService
    {
        private readonly ITasksRepository _tasksRepository;

        public TasksService(ITasksRepository workflowInstanceRepository)
        {
            _tasksRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
        }

        public async Task<long> CountAsync() => await _tasksRepository.CountAsync();

        /// <summary>
        /// Gets all running  tasks
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<IList<TaskExecution>> GetAllAsync(int? skip = null, int? limit = null)
            => await _tasksRepository.GetAllAsync(skip, limit);

        /// <summary>
        /// Gets task by WorkflowInstanceId, TaskId, ExecutionId.
        /// </summary>
        /// <param name="workflowInstanceId">Instance Id.</param>
        /// <param name="taskId">Task Id.</param>
        /// <param name="executionId">Execution Id.</param>
        /// <returns></returns>
        public async Task<TaskExecution> GetTaskAsync(string workflowInstanceId, string taskId, string executionId)
            => await _tasksRepository.GetTaskAsync(workflowInstanceId, taskId, executionId);
    }
}
