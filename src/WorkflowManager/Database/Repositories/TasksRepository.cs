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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Database.Options;
using Monai.Deploy.WorkflowManager.Common.Logging;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Common.Database.Repositories
{
    public class TasksRepository : RepositoryBase, ITasksRepository
    {
        private readonly IMongoCollection<WorkflowInstance> _workflowInstanceCollection;
        private readonly ILogger<TasksRepository> _logger;

        public TasksRepository(
            IMongoClient client,
            IOptions<WorkloadManagerDatabaseSettings> bookStoreDatabaseSettings,
            ILogger<TasksRepository> logger)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var mongoDatabase = client.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
            _workflowInstanceCollection = mongoDatabase.GetCollection<WorkflowInstance>("WorkflowInstances");

            var task = Task.Run(() => EnsureIndex(_workflowInstanceCollection));
            task.Wait();
        }

        private static async Task EnsureIndex(IMongoCollection<WorkflowInstance> workflowInstanceCollection)
        {
            var asyncCursor = (await workflowInstanceCollection.Indexes.ListAsync());
            var bsonDocuments = (await asyncCursor.ToListAsync());
            var indexes = bsonDocuments.Select(_ => _.GetElement("name").Value.ToString()).ToList();

            // If index not present create it else skip.
            if (!indexes.Any(i => i is not null && i.Equals("TasksIndex")))
            {
                // Create Index here

                var options = new CreateIndexOptions()
                {
                    Name = "TasksIndex"
                };
                var model = new CreateIndexModel<WorkflowInstance>(
                    Builders<WorkflowInstance>.IndexKeys.Ascending($"{nameof(WorkflowInstance.Tasks)}.{nameof(Task.Status)}"),
                    options
                    );

                await workflowInstanceCollection.Indexes.CreateOneAsync(model);
            }
        }

        public async Task<(IList<TaskExecution> Tasks, long Count)> GetAllAsync(int? skip, int? limit)
        {
            try
            {
                var filter = new FilterDefinitionBuilder<WorkflowInstance>().Where(x =>
                x.Status != Status.Succeeded && x.Status != Status.Failed);

                var result = await _workflowInstanceCollection
                    .Find(filter)
                    .ToListAsync();

                if (result is null || result.Count == 0)
                {
                    return (new List<TaskExecution>(), 0);
                }

                var tasks = result.SelectMany(r => r.Tasks.Where(t =>
                t.Status != TaskExecutionStatus.Succeeded &&
                t.Status != TaskExecutionStatus.Failed &&
                t.Status != TaskExecutionStatus.Canceled));

                return (tasks.Skip(skip ?? 0).Take(limit ?? 500).ToList(), tasks.Count());
            }
            catch (Exception e)
            {
                _logger.DbGetAllTasksError(e);
                return (new List<TaskExecution>(), 0);
            }
        }

        public async Task<TaskExecution?> GetTaskAsync(string workflowInstanceId, string taskId, string executionId)
        {
            try
            {
                var builder = Builders<WorkflowInstance>.Filter;

                var filter = builder.Eq(wf => wf.Id, workflowInstanceId);

                var result = await _workflowInstanceCollection
                    .Find(filter)
                    .FirstOrDefaultAsync();

                return result?.Tasks.FirstOrDefault(t => t.TaskId == taskId && t.ExecutionId == executionId);
            }
            catch (Exception e)
            {
                _logger.DbGetTasksError(taskId, e);
                return default;
            }
        }
    }
}
