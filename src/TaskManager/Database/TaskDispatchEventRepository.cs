// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Monai.Deploy.WorkflowManager.TaskManager.Logging;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.TaskManager.Database
{
    public class TaskDispatchEventRepository : ITaskDispatchEventRepository
    {
        private readonly IMongoCollection<TaskDispatchEventInfo> _taskDispatchEventCollection;
        private readonly ILogger<TaskDispatchEventRepository> _logger;

        public TaskDispatchEventRepository(
            IMongoClient client,
            IOptions<TaskManagerDatabaseSettings> databaseSettings,
            ILogger<TaskDispatchEventRepository> logger)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var mongoDatabase = client.GetDatabase(databaseSettings.Value.DatabaseName);
            _taskDispatchEventCollection = mongoDatabase.GetCollection<TaskDispatchEventInfo>(databaseSettings.Value.TaskDispatchEventCollectionName);
        }

        public async Task<bool> CreateAsync(TaskDispatchEventInfo taskDispatchEventInfo)
        {
            Guard.Against.Null(taskDispatchEventInfo, nameof(taskDispatchEventInfo));

            try
            {
                await _taskDispatchEventCollection.InsertOneAsync(taskDispatchEventInfo).ConfigureAwait(false);

                return true;
            }
            catch (Exception e)
            {
                _logger.DatabaseException(nameof(CreateAsync), e);

                return false;
            }
        }

        public async Task<TaskDispatchEventInfo?> GetByTaskExecutionIdAsync(string taskExecutionId)
        {
            Guard.Against.NullOrWhiteSpace(taskExecutionId, nameof(taskExecutionId));

            try
            {
                return await _taskDispatchEventCollection
                .Find(x => x.Event.ExecutionId == taskExecutionId)
                .FirstOrDefaultAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.DatabaseException(nameof(GetByTaskExecutionIdAsync), e);
                return default;
            }
        }

        public async Task<bool> RemoveAsync(string taskExecutionId)
        {
            Guard.Against.NullOrWhiteSpace(taskExecutionId, nameof(taskExecutionId));

            try
            {
                await _taskDispatchEventCollection.DeleteOneAsync(
                    Builders<TaskDispatchEventInfo>.Filter.Eq(p => p.Event.ExecutionId, taskExecutionId)).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                _logger.DatabaseException(nameof(RemoveAsync), e);
                return false;
            }
        }
    }
}
