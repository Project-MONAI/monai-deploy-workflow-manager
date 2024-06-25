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
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Options;
using Monai.Deploy.WorkflowManager.Common.Logging;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Common.Database
{
    public class TaskExecutionStatsRepository : ITaskExecutionStatsRepository
    {
        private readonly IMongoCollection<ExecutionStats> _taskExecutionStatsCollection;
        private readonly ILogger<TaskExecutionStatsRepository> _logger;

        public TaskExecutionStatsRepository(
            IMongoClient client,
            IOptions<ExecutionStatsDatabaseSettings> databaseSettings,
            ILogger<TaskExecutionStatsRepository> logger)
        {
            _ = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var mongoDatabase = client.GetDatabase(databaseSettings.Value.DatabaseName, null);
            _taskExecutionStatsCollection = mongoDatabase.GetCollection<ExecutionStats>("ExecutionStats", null);
            EnsureIndex(_taskExecutionStatsCollection).GetAwaiter().GetResult();
        }

        private static async Task EnsureIndex(IMongoCollection<ExecutionStats> taskExecutionStatsCollection)
        {
            ArgumentNullException.ThrowIfNull(taskExecutionStatsCollection, "TaskExecutionStatsCollection");

            var asyncCursor = await taskExecutionStatsCollection.Indexes.ListAsync();
            var bsonDocuments = await asyncCursor.ToListAsync();
            var indexes = bsonDocuments.Select(_ => _.GetElement("name").Value.ToString()).ToList();

            // If index not present create it else skip.
            if (!indexes.Any(i => i is not null && i.Equals("ExecutionStatsIndex")))
            {
                // Create Index here

                var options = new CreateIndexOptions()
                {
                    Name = "ExecutionStatsIndex"
                };
                var model = new CreateIndexModel<ExecutionStats>(
                    Builders<ExecutionStats>.IndexKeys.Ascending(s => s.StartedUTC),
                    options
                    );

                await taskExecutionStatsCollection.Indexes.CreateOneAsync(model);
            }
        }

        public async Task CreateAsync(TaskExecution taskExecutionInfo, string workflowId, string correlationId)
        {
            ArgumentNullException.ThrowIfNull(taskExecutionInfo, "taskDispatchEventInfo");

            try
            {
                var insertMe = new ExecutionStats(taskExecutionInfo, workflowId, correlationId);

                await _taskExecutionStatsCollection.ReplaceOneAsync(doc =>
                     doc.ExecutionId == insertMe.ExecutionId,
                    insertMe,
                    new ReplaceOptions { IsUpsert = true }
                        ).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.DatabaseException(nameof(CreateAsync), e);
            }
        }

        public async Task UpdateExecutionStatsAsync(TaskExecution taskUpdateEvent, string workflowId, TaskExecutionStatus? status = null)
        {
            ArgumentNullException.ThrowIfNull(taskUpdateEvent, "taskUpdateEvent");
            var currentStatus = status ?? taskUpdateEvent.Status;

            try
            {
                var updateMe = ExposeExecutionStats(new ExecutionStats(taskUpdateEvent, workflowId, ""), taskUpdateEvent);

                var duration = updateMe.CompletedAtUTC == default ? 0 : (updateMe.CompletedAtUTC - updateMe.StartedUTC).TotalMilliseconds / 1000;
                await _taskExecutionStatsCollection.UpdateOneAsync(o =>
                        o.ExecutionId == updateMe.ExecutionId,
                    Builders<ExecutionStats>.Update
                        .Set(w => w.Status, currentStatus.ToString())
                        .Set(w => w.LastUpdatedUTC, DateTime.UtcNow)
                        .Set(w => w.CompletedAtUTC, updateMe.CompletedAtUTC)
                        .Set(w => w.ExecutionTimeSeconds, updateMe.ExecutionTimeSeconds)
                        .Set(w => w.DurationSeconds, duration)
                        .Set(w => w.Reason, taskUpdateEvent.Reason)

                    , new UpdateOptions { IsUpsert = true }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.DatabaseException(nameof(CreateAsync), e);
            }
        }

        public async Task UpdateExecutionStatsAsync(TaskCancellationEvent taskCanceledEvent, string workflowId, string correlationId)
        {
            ArgumentNullException.ThrowIfNull(taskCanceledEvent, "taskCanceledEvent");

            try
            {
                var updateMe = new ExecutionStats(taskCanceledEvent, workflowId, correlationId);

                var duration = updateMe.CompletedAtUTC == default ? 0 : (updateMe.CompletedAtUTC - updateMe.StartedUTC).TotalMilliseconds / 1000;
                await _taskExecutionStatsCollection.UpdateOneAsync(o =>
                        o.ExecutionId == updateMe.ExecutionId,
                    Builders<ExecutionStats>.Update
                        .Set(w => w.Status, updateMe.Status)
                        .Set(w => w.Reason, taskCanceledEvent.Reason)
                        .Set(w => w.LastUpdatedUTC, DateTime.UtcNow)
                        .Set(w => w.CompletedAtUTC, updateMe.CompletedAtUTC)
                        .Set(w => w.DurationSeconds, duration)

                    , new UpdateOptions { IsUpsert = true }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.DatabaseException(nameof(CreateAsync), e);
            }
        }

        public async Task<IEnumerable<ExecutionStats>> GetAllStatsAsync(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "")
        {
            return await GetStatsAsync(startTime, endTime, null, null, workflowId, taskId);
        }

        public async Task<IEnumerable<ExecutionStats>> GetStatsAsync(DateTime startTime, DateTime endTime, int? pageSize = 10, int? pageNumber = 1, string workflowId = "", string taskId = "")
        {
            CreateFilter(startTime, endTime, workflowId, taskId, out var builder, out var filter);

            filter &= builder.Where(GetExecutedTasksFilter());

            var result = _taskExecutionStatsCollection.Find(filter);
            if (pageSize is not null)
            {
                result = result.Limit(pageSize).Skip((pageNumber - 1) * pageSize);
            }

            return await result.ToListAsync();
        }

        private static ExecutionStats ExposeExecutionStats(ExecutionStats taskExecutionStats, TaskExecution taskUpdateEvent)
        {
            if (taskUpdateEvent.ExecutionStats is not null)
            {
                if (taskUpdateEvent.ExecutionStats.ContainsKey("finishedAt") &&
                    DateTime.TryParse(taskUpdateEvent.ExecutionStats["finishedAt"], out var finished))
                {
                    taskExecutionStats.CompletedAtUTC = finished;
                    taskExecutionStats.DurationSeconds = (taskExecutionStats.CompletedAtUTC - taskExecutionStats.StartedUTC).TotalMilliseconds / 1000;
                }

                var statKeys = taskUpdateEvent.ExecutionStats.Keys.Where(v => v.StartsWith("podStartTime") || v.StartsWith("podFinishTime"));
                if (statKeys.Any())
                {
                    CalculatePodExecutionTime(taskExecutionStats, taskUpdateEvent, statKeys);
                }
            }
            return taskExecutionStats;
        }

        /// <summary>
        /// Calculates and sets ExecutionStats ExecutionTimeSeconds
        /// </summary>
        /// <param name="taskExecutionStats"></param>
        /// <param name="taskUpdateEvent"></param>
        /// <param name="statKeys"></param>
        private static void CalculatePodExecutionTime(ExecutionStats taskExecutionStats, TaskExecution taskUpdateEvent, IEnumerable<string> statKeys)
        {
            var start = DateTime.Now;
            var end = new DateTime();
            foreach (var statKey in statKeys)
            {
                if (statKey.Contains("StartTime") && DateTime.TryParse(taskUpdateEvent.ExecutionStats[statKey], out var startTime))
                {
                    start = startTime < start ? startTime : start;
                }
                else if (DateTime.TryParse(taskUpdateEvent.ExecutionStats[statKey], out var endTime))
                {
                    end = endTime > end ? endTime : start;
                }
            }
            taskExecutionStats.ExecutionTimeSeconds = (end - start).TotalMilliseconds / 1000;
        }

        public async Task<long> GetStatsStatusCountAsync(DateTime startTime, DateTime endTime, string status = "", string workflowId = "", string taskId = "")
        {
            Expression<Func<ExecutionStats, bool>>? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                statusFilter = t => t.Status == status;
            }
            return await GetStatsCountAsync(startTime, endTime, statusFilter, workflowId, taskId);
        }

        public async Task<long> GetStatsCountAsync(DateTime startTime, DateTime endTime, Expression<Func<ExecutionStats, bool>>? statusFilter = null, string workflowId = "", string taskId = "")
        {
            CreateFilter(startTime, endTime, workflowId, taskId, out var builder, out var filter);

            if (statusFilter is not null)
            {
                filter &= builder.Where(statusFilter);
            }

            return await _taskExecutionStatsCollection.CountDocumentsAsync(filter);
        }

        private static void CreateFilter(DateTime startTime, DateTime endTime, string workflowId, string taskId, out FilterDefinitionBuilder<ExecutionStats> builder, out FilterDefinition<ExecutionStats> filter)
        {
            var workflowNull = string.IsNullOrWhiteSpace(workflowId);
            var taskIdNull = string.IsNullOrWhiteSpace(taskId);

            builder = Builders<ExecutionStats>.Filter;
            filter = builder.Empty;
            filter &= builder.Where(t => t.StartedUTC >= startTime.ToUniversalTime());
            filter &= builder.Where(t => t.StartedUTC <= endTime.ToUniversalTime());
            filter &= builder.Where(t => workflowNull || t.WorkflowId == workflowId);
            filter &= builder.Where(t => taskIdNull || t.TaskId == taskId);
        }

        /// <summary>
        /// Gets filter for tasks that have ran to completion.
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<ExecutionStats, bool>> GetExecutedTasksFilter()
        {
            var dispatched = TaskExecutionStatus.Dispatched.ToString();
            var created = TaskExecutionStatus.Created.ToString();
            //var accepted = TaskExecutionStatus.Accepted.ToString();

            return t => t.Status != dispatched && t.Status != created;//&& t.Status != accepted;
        }


        public async Task<long> GetStatsTotalCompleteExecutionsCountAsync(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "")
        {
            var dispatched = TaskExecutionStatus.Dispatched.ToString();
            var created = TaskExecutionStatus.Created.ToString();
            var accepted = TaskExecutionStatus.Accepted.ToString();
            Expression<Func<ExecutionStats, bool>> statusFilter = t => t.Status != dispatched && t.Status != created && t.Status != accepted;

            return await GetStatsCountAsync(startTime, endTime, GetExecutedTasksFilter(), workflowId, taskId);
        }

        public async Task<long> GetStatsStatusSucceededCountAsync(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "")
        {
            Expression<Func<ExecutionStats, bool>> statusFilter = t => t.Status == TaskExecutionStatus.Succeeded.ToString();
            return await GetStatsCountAsync(startTime, endTime, statusFilter, workflowId, taskId);
        }

        public async Task<long> GetStatsStatusFailedCountAsync(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "")
        {
            Expression<Func<ExecutionStats, bool>> statusFilter = t => t.Status == TaskExecutionStatus.Failed.ToString() || t.Status == TaskExecutionStatus.PartialFail.ToString();
            return await GetStatsCountAsync(startTime, endTime, statusFilter, workflowId, taskId);
        }

        public async Task<(double avgTotalExecution, double avgArgoExecution)> GetAverageStats(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "")
        {
            CreateFilter(startTime, endTime, workflowId, taskId, out var builder, out var filter);
            filter &= builder.Where(t => t.Status == TaskExecutionStatus.Succeeded.ToString());

            var test = await _taskExecutionStatsCollection.Aggregate()
                .Match(filter)
                .Group(g => new { g.Version }, r => new
                {
                    avgTotalExecution = r.Average(x => x.DurationSeconds),
                    avgArgoExecution = r.Average(x => x.ExecutionTimeSeconds)
                }).ToListAsync();

            var firstResult = test.FirstOrDefault() ?? new { avgTotalExecution = 0.0, avgArgoExecution = 0.0 };
            return (firstResult.avgTotalExecution, firstResult.avgArgoExecution);
        }
    }
}
