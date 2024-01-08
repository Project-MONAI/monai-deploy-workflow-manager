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
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Monai.Deploy.WorkflowManager.TaskManager.Database.Options;
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
            _taskDispatchEventCollection = mongoDatabase.GetCollection<TaskDispatchEventInfo>("TaskDispatchEvents");
        }

        public async Task<TaskDispatchEventInfo?> CreateAsync(TaskDispatchEventInfo taskDispatchEventInfo)
        {
            ArgumentNullException.ThrowIfNull(taskDispatchEventInfo, nameof(taskDispatchEventInfo));

            try
            {
                await _taskDispatchEventCollection.InsertOneAsync(taskDispatchEventInfo).ConfigureAwait(false);
                return await GetByTaskExecutionIdAsync(taskDispatchEventInfo.Event.ExecutionId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.DatabaseException(nameof(CreateAsync), e);
                return default;
            }
        }

        public async Task<TaskDispatchEventInfo?> UpdateUserAccountsAsync(TaskDispatchEventInfo taskDispatchEventInfo)
        {
            ArgumentNullException.ThrowIfNull(taskDispatchEventInfo, nameof(taskDispatchEventInfo));

            try
            {
                await _taskDispatchEventCollection.FindOneAndUpdateAsync(i => i.Id == taskDispatchEventInfo.Id, Builders<TaskDispatchEventInfo>.Update.Set(p => p.UserAccounts, taskDispatchEventInfo.UserAccounts)).ConfigureAwait(false);
                return await GetByTaskExecutionIdAsync(taskDispatchEventInfo.Event.ExecutionId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.DatabaseException(nameof(UpdateUserAccountsAsync), e);
                return default;
            }
        }

        public async Task<TaskDispatchEventInfo?> GetByTaskExecutionIdAsync(string taskExecutionId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(taskExecutionId, nameof(taskExecutionId));

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
            ArgumentNullException.ThrowIfNullOrWhiteSpace(taskExecutionId, nameof(taskExecutionId));

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

        public async Task<TaskDispatchEventInfo?> UpdateTaskPluginArgsAsync(TaskDispatchEventInfo taskDispatchEventInfo, Dictionary<string, string> pluginArgs)
        {
            ArgumentNullException.ThrowIfNull(taskDispatchEventInfo, nameof(taskDispatchEventInfo));
            ArgumentNullException.ThrowIfNull(pluginArgs, nameof(pluginArgs));

            try
            {
                await _taskDispatchEventCollection.FindOneAndUpdateAsync(i => i.Id == taskDispatchEventInfo.Id, Builders<TaskDispatchEventInfo>.Update.Set(p => p.Event.TaskPluginArguments, pluginArgs)).ConfigureAwait(false);
                return await GetByTaskExecutionIdAsync(taskDispatchEventInfo.Event.ExecutionId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.DatabaseException(nameof(UpdateTaskPluginArgsAsync), e);
                return default;
            }
        }
    }
}
