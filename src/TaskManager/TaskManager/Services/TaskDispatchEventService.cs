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

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Monai.Deploy.WorkflowManager.TaskManager.Database;
using Monai.Deploy.WorkflowManager.TaskManager.Logging;

namespace Monai.Deploy.WorkflowManager.TaskManager.Services
{
    public class TaskDispatchEventService : ITaskDispatchEventService
    {
        private readonly ITaskDispatchEventRepository _taskDispatchEventRepository;

        private readonly ILogger<TaskDispatchEventService> _logger;

        public TaskDispatchEventService(ITaskDispatchEventRepository taskDispatchEventRepository, ILogger<TaskDispatchEventService> logger)
        {
            _taskDispatchEventRepository = taskDispatchEventRepository ?? throw new ArgumentNullException(nameof(taskDispatchEventRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TaskDispatchEventInfo?> CreateAsync(TaskDispatchEventInfo taskDispatchEvent)
        {
            Guard.Against.Null(taskDispatchEvent, nameof(taskDispatchEvent));

            try
            {
                return await _taskDispatchEventRepository.CreateAsync(taskDispatchEvent).ConfigureAwait(false);
            }
            finally
            {
                _logger.TaskDispatchEventSaved(taskDispatchEvent.Event.ExecutionId);
            }
        }

        public async Task<TaskDispatchEventInfo?> UpdateUserAccountsAsync(TaskDispatchEventInfo taskDispatchEvent)
        {
            Guard.Against.Null(taskDispatchEvent, nameof(taskDispatchEvent));

            try
            {
                return await _taskDispatchEventRepository.UpdateUserAccountsAsync(taskDispatchEvent).ConfigureAwait(false);
            }
            finally
            {
                _logger.TaskDispatchEventSaved(taskDispatchEvent.Event.ExecutionId);
            }
        }

        public async Task<TaskDispatchEventInfo?> GetByTaskExecutionIdAsync(string taskExecutionId)
        {
            Guard.Against.NullOrWhiteSpace(taskExecutionId, nameof(taskExecutionId));
            return await _taskDispatchEventRepository.GetByTaskExecutionIdAsync(taskExecutionId).ConfigureAwait(false);
        }

        public async Task<TaskDispatchEventInfo> UpdateTaskPluginArgsAsync(TaskDispatchEventInfo taskDispatchEvent, Dictionary<string, string> pluginArgs)
        {
            Guard.Against.Null(taskDispatchEvent, nameof(taskDispatchEvent));
            Guard.Against.Null(pluginArgs);

            try
            {
                return await _taskDispatchEventRepository.UpdateTaskPluginArgsAsync(taskDispatchEvent, pluginArgs);
            }
            finally
            {
                _logger.TaskDispatchEventSaved(taskDispatchEvent.Event.ExecutionId);
            }
        }
    }
}
