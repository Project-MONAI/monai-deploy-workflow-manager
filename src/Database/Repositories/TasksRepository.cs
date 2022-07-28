
// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Database.Repositories
{
    public class TasksRepository : RepositoryBase, ITasksRepository
    {
        private readonly IMongoCollection<WorkflowInstance> _workflowInstanceCollection;
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly ILogger<TasksRepository> _logger;
        private string _tasksCollection;

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
            _workflowInstanceCollection = mongoDatabase.GetCollection<WorkflowInstance>(bookStoreDatabaseSettings.Value.WorkflowInstanceCollectionName);
            _collection = mongoDatabase.GetCollection<BsonDocument>(bookStoreDatabaseSettings.Value.WorkflowInstanceCollectionName);

            var task = Task.Run(() => EnsureIndex(_workflowInstanceCollection));
            task.Wait();
        }

        private async Task EnsureIndex(IMongoCollection<WorkflowInstance> workflowInstanceCollection)
        {
            var asyncCursor = (await workflowInstanceCollection.Indexes.ListAsync());
            var bsonDocuments = (await asyncCursor.ToListAsync());
            var indexes = bsonDocuments.Select(_ => _.GetElement("name").Value.ToString()).ToList();
            // If index not present create it else skip.
            if (!indexes.Any(i => i.Equals("TasksIndex")))
            {
                // Create Index here

                var options = new CreateIndexOptions()
                {
                    Name = "TasksIndex"
                };
                var model = new CreateIndexModel<WorkflowInstance>(
                    Builders<WorkflowInstance>.IndexKeys.Ascending(s => s.Tasks),
                    options
                    );

                _tasksCollection = await workflowInstanceCollection.Indexes.CreateOneAsync(model);
            }
        }

        public async Task<long> CountAsync() => await CountAsync(_workflowInstanceCollection, null);

        public async Task<IList<TaskExecution>> GetAllAsync(int? skip, int? limit)
        {
            var builder = Builders<WorkflowInstance>.Filter;

            var filter = builder.Eq("Tasks.Status", TaskExecutionStatus.Accepted);
            filter &= builder.Ne("Tasks.Status", TaskExecutionStatus.Dispatched);

            var result = await _workflowInstanceCollection.Aggregate()
                .Match(filter)
                .Unwind<WorkflowInstance, WorkflowInstanceTasksUnwindResult>(wf => wf.Tasks)
                .Skip(skip ?? 0)
                .Limit(limit ?? 500)
                .ToListAsync();

            return result.Select(r => r.Tasks).ToList();
        }

        public async Task<TaskExecution?> GetTaskAsync(string workflowInstanceId, string taskId, string executionId)
        {
            var builder = Builders<WorkflowInstance>.Filter;

            var filter = builder.Eq(wf => wf.WorkflowId, workflowInstanceId);

            var result = await _workflowInstanceCollection
                .Find(filter)
                .FirstOrDefaultAsync();

            return result?.Tasks.FirstOrDefault(t => t.TaskId == taskId && t.ExecutionId == executionId);
        }
    }
}
