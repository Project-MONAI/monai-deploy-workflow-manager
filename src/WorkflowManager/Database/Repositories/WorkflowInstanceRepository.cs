/*
 * Copyright 2021-2022 MONAI Consortium
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Database.Repositories
{
    public class WorkflowInstanceRepository : RepositoryBase, IWorkflowInstanceRepository
    {
        private readonly IMongoCollection<WorkflowInstance> _workflowInstanceCollection;
        private readonly ILogger<WorkflowInstanceRepository> _logger;

        public WorkflowInstanceRepository(
            IMongoClient client,
            IOptions<WorkloadManagerDatabaseSettings> bookStoreDatabaseSettings,
            ILogger<WorkflowInstanceRepository> logger)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var mongoDatabase = client.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
            _workflowInstanceCollection = mongoDatabase.GetCollection<WorkflowInstance>(bookStoreDatabaseSettings.Value.WorkflowInstanceCollectionName);
        }

        public async Task<IList<WorkflowInstance>> GetListAsync()
        {
            var workflowIstances = await _workflowInstanceCollection.Find(Builders<WorkflowInstance>.Filter.Empty).ToListAsync();

            return workflowIstances ?? new List<WorkflowInstance>();
        }

        public async Task<IList<WorkflowInstance>> GetByWorkflowsIdsAsync(List<string> workflowIds)
        {
            Guard.Against.NullOrEmpty(workflowIds, nameof(workflowIds));

            try
            {
                var filterDef = new FilterDefinitionBuilder<WorkflowInstance>();

                var filter = filterDef.In(x => x.WorkflowId, workflowIds);
                var workflowIstances = await _workflowInstanceCollection.Find(filter).ToListAsync();

                return workflowIstances ?? new List<WorkflowInstance>();
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(GetByWorkflowsIdsAsync), e);

                return new List<WorkflowInstance>();
            }
        }

        public async Task<bool> CreateAsync(IList<WorkflowInstance> workflowInstances)
        {
            Guard.Against.NullOrEmpty(workflowInstances, nameof(workflowInstances));

            try
            {
                await _workflowInstanceCollection.InsertManyAsync(workflowInstances);

                return true;
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(CreateAsync), e);

                return false;
            }
        }

        public async Task<bool> UpdateTaskAsync(string workflowInstanceId, string taskId, TaskExecution task)
        {
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrWhiteSpace(taskId, nameof(taskId));
            Guard.Against.Null(task, nameof(task));

            try
            {
                await _workflowInstanceCollection.FindOneAndUpdateAsync(
                    i => i.Id == workflowInstanceId && i.Tasks.Any(t => t.TaskId == taskId),
                    Builders<WorkflowInstance>.Update.Set(w => w.Tasks[-1], task));

                return true;
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(UpdateTaskAsync), e);

                return false;
            }
        }

        public async Task<bool> UpdateTaskStatusAsync(string workflowInstanceId, string taskId, TaskExecutionStatus status)
        {
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrWhiteSpace(taskId, nameof(taskId));
            Guard.Against.Null(status, nameof(status));

            try
            {
                await _workflowInstanceCollection.FindOneAndUpdateAsync(
                    i => i.Id == workflowInstanceId && i.Tasks.Any(t => t.TaskId == taskId),
                    Builders<WorkflowInstance>.Update.Set(w => w.Tasks[-1].Status, status));

                return true;
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(UpdateTaskStatusAsync), e);

                return false;
            }
        }

        public async Task<bool> UpdateTaskOutputArtifactsAsync(string workflowInstanceId, string taskId, Dictionary<string, string> outputArtifacts)
        {
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrWhiteSpace(taskId, nameof(taskId));
            Guard.Against.Null(outputArtifacts, nameof(outputArtifacts));

            try
            {
                await _workflowInstanceCollection.FindOneAndUpdateAsync(
                    i => i.Id == workflowInstanceId && i.Tasks.Any(t => t.TaskId == taskId),
                    Builders<WorkflowInstance>.Update.Set(w => w.Tasks[-1].OutputArtifacts, outputArtifacts));

                return true;
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(UpdateTaskOutputArtifactsAsync), e);

                return false;
            }
        }

        public async Task<bool> UpdateWorkflowInstanceStatusAsync(string workflowInstanceId, Status status)
        {
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.Null(status, nameof(status));

            try
            {
                await _workflowInstanceCollection.FindOneAndUpdateAsync(
                    i => i.Id == workflowInstanceId,
                    Builders<WorkflowInstance>.Update.Set(w => w.Status, status));

                return true;
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(UpdateWorkflowInstanceStatusAsync), e);
                return false;
            }
        }

        public async Task<WorkflowInstance> AcknowledgeWorkflowInstanceErrors(string workflowInstanceId)
        {
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            var acknowledgedTimeStamp = DateTime.UtcNow;

            await _workflowInstanceCollection.FindOneAndUpdateAsync(
                i => i.Id == workflowInstanceId && i.AcknowledgedWorkflowErrors == null,
                Builders<WorkflowInstance>.Update.Set(w => w.AcknowledgedWorkflowErrors, acknowledgedTimeStamp));

            return await GetByWorkflowInstanceIdAsync(workflowInstanceId);
        }

        public async Task<WorkflowInstance> AcknowledgeTaskError(string workflowInstanceId, string executionId)
        {
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrWhiteSpace(executionId, nameof(executionId));

            var acknowledgedTimeStamp = DateTime.UtcNow;

            var workflowInstance = await _workflowInstanceCollection.FindOneAndUpdateAsync(
                i => i.Id == workflowInstanceId && i.Tasks.Any(t => t.ExecutionId == executionId && t.Status == TaskExecutionStatus.Failed && t.AcknowledgedTaskErrors == null),
                Builders<WorkflowInstance>.Update.Set(w => w.Tasks[-1].AcknowledgedTaskErrors, acknowledgedTimeStamp));

            return await GetByWorkflowInstanceIdAsync(workflowInstanceId);
        }

        public async Task<TaskExecution?> GetTaskByIdAsync(string workflowInstanceId, string taskId)
        {
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrWhiteSpace(taskId, nameof(taskId));

            try
            {
                var workflowInstance = await _workflowInstanceCollection
                                            .Find(x => x.Id == workflowInstanceId)
                                            .FirstOrDefaultAsync();

                return workflowInstance?.Tasks?.FirstOrDefault(t => t.TaskId == taskId);
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(GetTaskByIdAsync), e);

                return null;
            }
        }

        public async Task<bool> UpdateTasksAsync(string workflowInstanceId, List<TaskExecution> tasks)
        {
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrEmpty(tasks, nameof(tasks));

            try
            {
                await _workflowInstanceCollection.FindOneAndUpdateAsync(
                    i => i.Id == workflowInstanceId,
                    Builders<WorkflowInstance>.Update.Set(w => w.Tasks, tasks));

                return true;
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(UpdateTasksAsync), e);
                return false;
            }
        }

        public async Task<WorkflowInstance> GetByWorkflowInstanceIdAsync(string workflowInstanceId)
        {
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            var workflow = await _workflowInstanceCollection
                            .Find(x => x.Id == workflowInstanceId)
                            .FirstOrDefaultAsync();

            return workflow;
        }

        public async Task<long> CountAsync() => await CountAsync(_workflowInstanceCollection, null);

        public async Task<long> FilteredCountAsync(Status? status = null, string? payloadId = null)
        {
            var builder = Builders<WorkflowInstance>.Filter;
            var filter = builder.Empty;
            if (status is not null)
            {
                filter &= builder.Eq(w => w.Status, status);
            }
            if (string.IsNullOrEmpty(payloadId) is false)
            {
                filter &= builder.Eq(w => w.PayloadId, payloadId);
            }

            return await CountAsync(_workflowInstanceCollection, filter);
        }

        public async Task<IList<WorkflowInstance>> GetAllAsync(int? skip = null, int? limit = null, Status? status = null, string? payloadId = null)
        {
            var builder = Builders<WorkflowInstance>.Filter;
            var filter = builder.Empty;
            if (status is not null)
            {
                filter &= builder.Eq(w => w.Status, status);
            }
            if (string.IsNullOrEmpty(payloadId) is false)
            {
                filter &= builder.Eq(w => w.PayloadId, payloadId);
            }

            return await GetAllAsync(_workflowInstanceCollection,
                                          filter,
                                          Builders<WorkflowInstance>.Sort.Descending(x => x.StartTime),
                                          skip,
                                          limit);
        }

        public Task<IList<WorkflowInstance>> GetAllAsync(int? skip, int? limit)
            => GetAllAsync(_workflowInstanceCollection,
                                null,
                                Builders<WorkflowInstance>.Sort.Descending(x => x.StartTime),
                                skip,
                                limit);

        public async Task<IList<WorkflowInstance>> GetAllFailedAsync(DateTime startDate)
        {
            return await GetAllAsync(_workflowInstanceCollection,
                                  wfInstance => wfInstance.Status == Status.Failed
                                      && wfInstance.AcknowledgedWorkflowErrors.HasValue
                                      && wfInstance.AcknowledgedWorkflowErrors.Value > startDate,
                                  Builders<WorkflowInstance>.Sort.Descending(x => x.Id));
        }
    }
}
