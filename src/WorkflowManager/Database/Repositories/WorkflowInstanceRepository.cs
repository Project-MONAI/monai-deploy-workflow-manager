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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Database.Options;
using Monai.Deploy.WorkflowManager.Common.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Monai.Deploy.WorkflowManager.Common.Database.Repositories
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
            _workflowInstanceCollection = mongoDatabase.GetCollection<WorkflowInstance>("WorkflowInstances");
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
                _logger.DbGetWorkflowInstancesError(e);
                return new List<WorkflowInstance>();
            }
        }

        public async Task<IList<WorkflowInstance>> GetByPayloadIdsAsync(List<string> workflowIds)
        {
            Guard.Against.NullOrEmpty(workflowIds, nameof(workflowIds));

            try
            {
                var filterDef = new FilterDefinitionBuilder<WorkflowInstance>();

                var filter = filterDef.In(x => x.PayloadId, workflowIds);
                var workflowIstances = await _workflowInstanceCollection.Find(filter).ToListAsync();

                return workflowIstances ?? new List<WorkflowInstance>();
            }
            catch (Exception e)
            {
                _logger.DbGetWorkflowInstancesError(e);
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
                _logger.DbCreateWorkflowInstancesError(e);

                return false;
            }
        }

        public async Task<bool> UpdateTaskAsync(string workflowInstanceId, string taskId, TaskExecution task)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(taskId, nameof(taskId));
            ArgumentNullException.ThrowIfNull(task, nameof(task));

            try
            {
                await _workflowInstanceCollection.FindOneAndUpdateAsync(
                    i => i.Id == workflowInstanceId && i.Tasks.Any(t => t.TaskId == taskId),
                    Builders<WorkflowInstance>.Update.Set(w => w.Tasks.FirstMatchingElement(), task));

                return true;
            }
            catch (Exception e)
            {
                _logger.DbUpdateWorkflowInstancesError(workflowInstanceId, e);

                return false;
            }
        }

        public async Task<bool> UpdateTaskStatusAsync(string workflowInstanceId, string taskId, TaskExecutionStatus status)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(taskId, nameof(taskId));
            ArgumentNullException.ThrowIfNull(status, nameof(status));

            try
            {
                var update = Builders<WorkflowInstance>.Update
                    .Set(w => w.Tasks.FirstMatchingElement().Status, status);

                if (status is TaskExecutionStatus.Succeeded
                    || status is TaskExecutionStatus.Failed
                    || status is TaskExecutionStatus.PartialFail
                    || status is TaskExecutionStatus.Canceled)
                {
                    update = Builders<WorkflowInstance>.Update
                    .Set(w => w.Tasks.FirstMatchingElement().Status, status)
                    .Set(w => w.Tasks.FirstMatchingElement().TaskEndTime, DateTime.UtcNow);
                }

                await _workflowInstanceCollection.FindOneAndUpdateAsync(
                    i => i.Id == workflowInstanceId && i.Tasks.Any(t => t.TaskId == taskId), update);

                return true;
            }
            catch (Exception e)
            {
                _logger.DbUpdateTaskStatusError(taskId, status, e);

                return false;
            }
        }

        public async Task<bool> UpdateTaskOutputArtifactsAsync(string workflowInstanceId, string taskId, Dictionary<string, string> outputArtifacts)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(taskId, nameof(taskId));
            ArgumentNullException.ThrowIfNull(outputArtifacts, nameof(outputArtifacts));

            try
            {
                await _workflowInstanceCollection.FindOneAndUpdateAsync(
                    i => i.Id == workflowInstanceId && i.Tasks.Any(t => t.TaskId == taskId),
                    Builders<WorkflowInstance>.Update.Set(w => w.Tasks.FirstMatchingElement().OutputArtifacts, outputArtifacts));

                return true;
            }
            catch (Exception e)
            {
                _logger.DbUpdateTaskOutputArtifactError(taskId, e);

                return false;
            }
        }

        public async Task<bool> UpdateWorkflowInstanceStatusAsync(string workflowInstanceId, Status status)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            ArgumentNullException.ThrowIfNull(status, nameof(status));

            try
            {
                await _workflowInstanceCollection.FindOneAndUpdateAsync(
                    i => i.Id == workflowInstanceId,
                    Builders<WorkflowInstance>.Update.Set(w => w.Status, status));

                return true;
            }
            catch (Exception e)
            {
                _logger.DbUpdateWorkflowInstanceStatusError(workflowInstanceId, status, e);
                return false;
            }
        }

        public async Task<WorkflowInstance> AcknowledgeWorkflowInstanceErrors(string workflowInstanceId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            var acknowledgedTimeStamp = DateTime.UtcNow;

            await _workflowInstanceCollection.FindOneAndUpdateAsync(
                i => i.Id == workflowInstanceId,
                Builders<WorkflowInstance>.Update.Set(w => w.AcknowledgedWorkflowErrors, acknowledgedTimeStamp));

            return await GetByWorkflowInstanceIdAsync(workflowInstanceId);
        }

        public async Task<WorkflowInstance> AcknowledgeTaskError(string workflowInstanceId, string executionId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(executionId, nameof(executionId));

            var acknowledgedTimeStamp = DateTime.UtcNow;

            var result = await _workflowInstanceCollection.UpdateOneAsync(
                i => i.Id == workflowInstanceId && i.Tasks.Any(t => t.ExecutionId == executionId && (t.Status == TaskExecutionStatus.Failed || t.Status == TaskExecutionStatus.PartialFail)),
                Builders<WorkflowInstance>.Update.Set(w => w.Tasks.FirstMatchingElement().AcknowledgedTaskErrors, acknowledgedTimeStamp));

            return await GetByWorkflowInstanceIdAsync(workflowInstanceId);
        }

        public async Task<TaskExecution?> GetTaskByIdAsync(string workflowInstanceId, string taskId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(taskId, nameof(taskId));

            try
            {
                var workflowInstance = await _workflowInstanceCollection
                                            .Find(x => x.Id == workflowInstanceId)
                                            .FirstOrDefaultAsync();

                return workflowInstance?.Tasks?.FirstOrDefault(t => t.TaskId == taskId);
            }
            catch (Exception e)
            {
                _logger.DbGetTaskByIdError(taskId, e);

                return null;
            }
        }

        public async Task<bool> UpdateExportCompleteMetadataAsync(string workflowInstanceId, string executionId, Dictionary<string, object> fileStatuses)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            ArgumentNullException.ThrowIfNullOrEmpty(executionId, nameof(executionId));

            try
            {
                await _workflowInstanceCollection.UpdateOneAsync(
                    i => i.Id == workflowInstanceId && i.Tasks.Any(t => t.ExecutionId == executionId),
                    Builders<WorkflowInstance>.Update.Set(w => w.Tasks.FirstMatchingElement().ResultMetadata, fileStatuses));

                return true;
            }
            catch (Exception e)
            {
                _logger.DbUpdateTasksError(workflowInstanceId, e);
                return false;
            }
        }

        public async Task<bool> UpdateTasksAsync(string workflowInstanceId, List<TaskExecution> tasks)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
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
                _logger.DbUpdateTasksError(workflowInstanceId, e);
                return false;
            }
        }

        public async Task<WorkflowInstance> GetByWorkflowInstanceIdAsync(string workflowInstanceId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            var workflow = await _workflowInstanceCollection
                            .Find(x => x.Id == workflowInstanceId)
                            .FirstOrDefaultAsync();

            return workflow;
        }

        public async Task<long> CountAsync(FilterDefinition<WorkflowInstance> filter)
        {
            return await CountAsync(_workflowInstanceCollection, filter);
        }

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

        public async Task<IList<WorkflowInstance>> GetAllFailedAsync()
        {
            return await GetAllAsync(_workflowInstanceCollection,
                                  wfInstance => (wfInstance.Status == Status.Failed ||
                                      wfInstance.Tasks.Any(task => task.Status.Equals(TaskExecutionStatus.PartialFail)))
                                      && wfInstance.AcknowledgedWorkflowErrors == null,
                                  Builders<WorkflowInstance>.Sort.Descending(x => x.Id));
        }
    }
}
