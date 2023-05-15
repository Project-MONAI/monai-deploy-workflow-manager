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
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkflowManager.Logging;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Database
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
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var mongoDatabase = client.GetDatabase(databaseSettings.Value.DatabaseName, null);
            _taskExecutionStatsCollection = mongoDatabase.GetCollection<ExecutionStats>("ExecutionStats", null);
            EnsureIndex(_taskExecutionStatsCollection).GetAwaiter().GetResult();
        }

        private static async Task EnsureIndex(IMongoCollection<ExecutionStats> TaskExecutionStatsCollection)
        {
            Guard.Against.Null(TaskExecutionStatsCollection, "TaskExecutionStatsCollection");

            var asyncCursor = (await TaskExecutionStatsCollection.Indexes.ListAsync());
            var bsonDocuments = (await asyncCursor.ToListAsync());
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

                await TaskExecutionStatsCollection.Indexes.CreateOneAsync(model);
            }
        }

        public async Task CreateAsync(TaskExecution TaskExecutionInfo, string workflowId, string correlationId)
        {
            Guard.Against.Null(TaskExecutionInfo, "taskDispatchEventInfo");

            try
            {
                var insertMe = new ExecutionStats(TaskExecutionInfo, workflowId, correlationId);

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
            Guard.Against.Null(taskUpdateEvent, "taskUpdateEvent");
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

                    , new UpdateOptions { IsUpsert = true }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.DatabaseException(nameof(CreateAsync), e);
            }
        }

        public async Task UpdateExecutionStatsAsync(TaskCancellationEvent taskCanceledEvent, string workflowId, string correlationId)
        {
            Guard.Against.Null(taskCanceledEvent, "taskCanceledEvent");

            try
            {
                var updateMe = new ExecutionStats(taskCanceledEvent, workflowId, correlationId);

                var duration = updateMe.CompletedAtUTC == default ? 0 : (updateMe.CompletedAtUTC - updateMe.StartedUTC).TotalMilliseconds / 1000;
                await _taskExecutionStatsCollection.UpdateOneAsync(o =>
                        o.ExecutionId == updateMe.ExecutionId,
                    Builders<ExecutionStats>.Update
                        .Set(w => w.Status, updateMe.Status)
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

        public async Task<IEnumerable<ExecutionStats>> GetStatsAsync(DateTime startTime, DateTime endTime, int PageSize = 10, int PageNumber = 1, string workflowId = "", string taskId = "")
        {
            startTime = startTime.ToUniversalTime();

            var workflowNull = string.IsNullOrWhiteSpace(workflowId);
            var taskIdNull = string.IsNullOrWhiteSpace(taskId);

            var result = await _taskExecutionStatsCollection.Find(T =>
             T.StartedUTC >= startTime &&
             T.StartedUTC <= endTime.ToUniversalTime() &&
             (workflowNull || T.WorkflowId == workflowId) &&
             (taskIdNull || T.TaskId == taskId)
             //&&
             //(
             //    T.Status == TaskExecutionStatus.Succeeded.ToString()
             //    || T.Status == TaskExecutionStatus.Failed.ToString()
             //    || T.Status == TaskExecutionStatus.PartialFail.ToString()
             // )
             )
                .Limit(PageSize)
                .Skip((PageNumber - 1) * PageSize)
                .ToListAsync();
            return result;
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
                    var start = DateTime.Now;
                    var end = new DateTime();
                    foreach (var statKey in statKeys)
                    {
                        if (statKey.Contains("StartTime") && DateTime.TryParse(taskUpdateEvent.ExecutionStats[statKey], out var startTime))
                        {
                            start = (startTime < start ? startTime : start);
                        }
                        else if (DateTime.TryParse(taskUpdateEvent.ExecutionStats[statKey], out var endTime))
                        {
                            end = (endTime > end ? endTime : start);
                        }
                    }
                    taskExecutionStats.ExecutionTimeSeconds = (end - start).TotalMilliseconds / 1000;
                }
            }
            return taskExecutionStats;
        }

        public async Task<long> GetStatsStatusCountAsync(DateTime start, DateTime endTime, string status = "", string workflowId = "", string taskId = "")
        {
            var statusNull = string.IsNullOrWhiteSpace(status);
            var workflowNull = string.IsNullOrWhiteSpace(workflowId);
            var taskIdNull = string.IsNullOrWhiteSpace(taskId);

            return await _taskExecutionStatsCollection.CountDocumentsAsync(T =>
            T.StartedUTC >= start.ToUniversalTime() &&
            T.StartedUTC <= endTime.ToUniversalTime() &&
            (workflowNull || T.WorkflowId == workflowId) &&
            (taskIdNull || T.TaskId == taskId) &&
            (statusNull || T.Status == status));
        }

        public async Task<long> GetStatsStatusSucceededCountAsync(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "")
        {
            var workflowNull = string.IsNullOrWhiteSpace(workflowId);
            var taskIdNull = string.IsNullOrWhiteSpace(taskId);

            return await _taskExecutionStatsCollection.CountDocumentsAsync(T =>
            T.StartedUTC >= startTime.ToUniversalTime() &&
            T.StartedUTC <= endTime.ToUniversalTime() &&
            (workflowNull || T.WorkflowId == workflowId) &&
            (taskIdNull || T.TaskId == taskId) &&
            T.Status == TaskExecutionStatus.Succeeded.ToString());
        }

        public async Task<long> GetStatsStatusFailedCountAsync(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "")
        {
            var workflowNull = string.IsNullOrWhiteSpace(workflowId);
            var taskIdNull = string.IsNullOrWhiteSpace(taskId);

            return await _taskExecutionStatsCollection.CountDocumentsAsync(T =>
            T.StartedUTC >= startTime.ToUniversalTime() &&
            T.StartedUTC <= endTime.ToUniversalTime() &&
            (workflowNull || T.WorkflowId == workflowId) &&
            (taskIdNull || T.TaskId == taskId) &&
            (
                T.Status == TaskExecutionStatus.Failed.ToString() ||
                T.Status == TaskExecutionStatus.PartialFail.ToString()
            ));
        }

        public async Task<(double avgTotalExecution, double avgArgoExecution)> GetAverageStats(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "")
        {
            var workflowNull = string.IsNullOrWhiteSpace(workflowId);
            var taskIdNull = string.IsNullOrWhiteSpace(taskId);

            var test = await _taskExecutionStatsCollection.Aggregate()
                .Match(T =>
                T.StartedUTC >= startTime.ToUniversalTime() &&
                T.StartedUTC <= endTime.ToUniversalTime() &&
                (workflowNull || T.WorkflowId == workflowId) &&
                (taskIdNull || T.TaskId == taskId) &&
                T.Status == TaskExecutionStatus.Succeeded.ToString())
                .Group(g => new { g.Version }, r => new
                {
                    avgTotalExecution = r.Average(x => (x.DurationSeconds)),
                    avgArgoExecution = r.Average(x => (x.ExecutionTimeSeconds))
                }).ToListAsync();

            var firstResult = test.FirstOrDefault() ?? new { avgTotalExecution = 0.0, avgArgoExecution = 0.0 };
            return (firstResult.avgTotalExecution, firstResult.avgArgoExecution);
        }
    }
}
