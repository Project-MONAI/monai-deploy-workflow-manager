// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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

        public async Task<bool> CreateAsync(TaskDispatchEventInfo taskDispatchEvent)
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

        public async Task<TaskDispatchEventInfo?> GetByTaskExecutionIdAsync(string taskExecutionId)
        {
            Guard.Against.NullOrWhiteSpace(taskExecutionId, nameof(taskExecutionId));
            return await _taskDispatchEventRepository.GetByTaskExecutionIdAsync(taskExecutionId).ConfigureAwait(false);
        }

        public async Task<bool> RemoveAsync(string taskExecutionId)
        {
            try
            {
                Guard.Against.NullOrWhiteSpace(taskExecutionId, nameof(taskExecutionId));
                return await _taskDispatchEventRepository.RemoveAsync(taskExecutionId).ConfigureAwait(false);
            }
            finally
            {
                _logger.TaskDispatchEventDeleted(taskExecutionId);
            }
        }
    }
}
