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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.Database.Tests
{
    public class TaskExecutionStatsRepositoryTests
    {
        private readonly Mock<ILogger<TaskExecutionStatsRepository>> _logger;
        private readonly Mock<IMongoClient> _client;
        private readonly IOptions<ExecutionStatsDatabaseSettings> _options;
        private readonly Mock<IMongoDatabase> _dbase;
        private readonly Mock<IMongoCollection<ExecutionStats>> _collection;
        private readonly TaskExecutionStatsRepository _repo;

        public TaskExecutionStatsRepositoryTests()
        {
            _logger = new Mock<ILogger<TaskExecutionStatsRepository>>();
            _client = new Mock<IMongoClient>(MockBehavior.Strict);
            _options = Microsoft.Extensions.Options.Options.Create(new ExecutionStatsDatabaseSettings() { DatabaseName = "dbName" });
            _dbase = new Mock<IMongoDatabase>();
            _collection = new Mock<IMongoCollection<ExecutionStats>>();

            var indexDoc = new BsonDocument(new Dictionary<string, string> { { "name", "ExecutionStatsIndex" } });
            var indexList = Task.FromResult(new List<BsonDocument> { indexDoc });

            Assert.NotNull(indexList);

            var cursor = new Mock<IAsyncCursor<BsonDocument>>();

            _dbase.Setup(d => d.GetCollection<ExecutionStats>(It.IsAny<string>(), null)).Returns(_collection.Object);
            _client.Setup(c => c.GetDatabase(It.IsAny<string>(), null)).Returns(_dbase.Object);
            _collection.Setup(c => c.Indexes.ListAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(cursor.Object));

            _repo = new TaskExecutionStatsRepository(_client.Object, _options, _logger.Object);
        }

        [Fact]
        public void ExecutionStats_Should_Contain_All_Fields()
        {
            const string workflowId = nameof(workflowId);
            const string workflowInstanceId = nameof(workflowInstanceId);
            const string correlationId = nameof(correlationId);
            const string taskId = nameof(taskId);
            const string executionId = nameof(executionId);
            var started = DateTime.Now.ToUniversalTime();

            var testObj = new TaskExecution
            {
                TaskId = taskId,
                ExecutionId = executionId,
                WorkflowInstanceId = workflowInstanceId
            };

            testObj.TaskStartTime = started;

            var output = new ExecutionStats(testObj, workflowId, correlationId);

            Assert.Equal(started, output.StartedUTC);
            Assert.Equal(executionId, output.ExecutionId);
            Assert.Equal(workflowInstanceId, output.WorkflowInstanceId);
            Assert.Equal(workflowId, output.WorkflowId);
            Assert.Equal(correlationId, output.CorrelationId);
            Assert.Equal(taskId, output.TaskId);
        }

        [Fact]
        public void TaskExecutionStatsRepository_Should_Check_For_Null_Logger()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => new TaskExecutionStatsRepository(_client.Object, _options, default));
        }

        [Fact]
        public void TaskExecutionStatsRepository_Should_Check_For_Null_Client()
        {
            Assert.Throws<ArgumentNullException>(() => new TaskExecutionStatsRepository(default, _options, _logger.Object));
        }


        [Fact]
        public async Task TaskExecutionStatsRepository_Insert_Should_Check_For_Null_Event()
        {
            var service = new TaskExecutionStatsRepository(_client.Object, _options, _logger.Object);
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateAsync(default, default, default));
        }

        [Fact]
        public async Task TaskExecutionStatsRepository_update_Should_Check_For_Null_Event()
        {
            var service = new TaskExecutionStatsRepository(_client.Object, _options, _logger.Object);
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateExecutionStatsAsync(default, default));
        }

        [Fact]
        public async Task TaskExecutionStatsRepository_UpdateExecutionStatsAsync_Should_Check_For_Null_Event()
        {
            var service = new TaskExecutionStatsRepository(_client.Object, _options, _logger.Object);
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateExecutionStatsAsync(null, "correlationId"));
        }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        [Fact]
        public async Task ExecutionStats_Create_Should_Store_All()
        {
            var correlationId = "correlationId";
            var workflowId = "workflowId";
            var testStore = new TaskExecution
            {
                ExecutionId = nameof(TaskDispatchEvent.ExecutionId),
                WorkflowInstanceId = nameof(TaskDispatchEvent.WorkflowInstanceId),
                TaskId = nameof(TaskDispatchEvent.TaskId),
                Status = TaskExecutionStatus.Created,
            };

            await _repo.CreateAsync(testStore, workflowId, correlationId);

            _collection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<ExecutionStats>>(),
                It.Is<ExecutionStats>(t =>
                    t.CorrelationId == correlationId &&
                    t.TaskId == testStore.TaskId &&
                    t.Status == testStore.Status.ToString() &&
                    t.WorkflowInstanceId == testStore.WorkflowInstanceId &&
                    t.ExecutionId == testStore.ExecutionId
                    ),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecutionStats_Create_Should_Log_On_Exception()
        {
            var correlationId = "correlationId";
            var workflowId = "workflowId";
            var testStore = new TaskExecution
            {
                ExecutionId = nameof(TaskDispatchEvent.ExecutionId),
                WorkflowInstanceId = nameof(TaskDispatchEvent.WorkflowInstanceId),
                TaskId = nameof(TaskDispatchEvent.TaskId),
                Status = TaskExecutionStatus.Created,
            };

            _collection.Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<ExecutionStats>>(),
                It.IsAny<ExecutionStats>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>())).Throws(new Exception());

            await _repo.CreateAsync(testStore, workflowId, correlationId);

            _logger.Verify(l =>
                l.IsEnabled(LogLevel.Error)
                , Times.Once);
        }

        [Fact]
        public async Task ExecutionStats_Update_Should_Log_On_Exception()
        {
            var workflowId = "workflowId";
            var testStore = new TaskExecution
            {
                ExecutionId = nameof(TaskDispatchEvent.ExecutionId),
                WorkflowInstanceId = nameof(TaskDispatchEvent.WorkflowInstanceId),
                TaskId = nameof(TaskDispatchEvent.TaskId),
                Status = TaskExecutionStatus.Created,
                ExecutionStats = new System.Collections.Generic.Dictionary<string, string> {
                    { "CompletedAt" , (new DateTime(2023,3,3,8,0,12)).ToString() },
                    { "podStartTime0","2023-03-30T08:33:00+00:00"},
                    { "podFinishTime0","2023-03-30T08:36:00+00:00"},
                }
            };

            _collection.Setup(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<ExecutionStats>>(),
                It.IsAny<UpdateDefinition<ExecutionStats>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>())).Throws(new Exception());

            await _repo.UpdateExecutionStatsAsync(testStore, workflowId);

            _logger.Verify(l =>
                l.IsEnabled(LogLevel.Error)
                , Times.Once);
        }

        [Fact]
        public async Task ExecutionStats_Update_Should_Update_All()
        {
            var workflowId = "workflowId";
            var testStore = new TaskExecution
            {
                ExecutionId = nameof(TaskDispatchEvent.ExecutionId),
                WorkflowInstanceId = nameof(TaskDispatchEvent.WorkflowInstanceId),
                TaskId = nameof(TaskDispatchEvent.TaskId),
                Status = TaskExecutionStatus.Created,
                ExecutionStats = new System.Collections.Generic.Dictionary<string, string> {
                    { "CompletedAt" , (new DateTime(2023,3,3,8,0,12)).ToString() },
                    { "podStartTime0","2023-03-30T08:33:00+00:00"},
                    { "podFinishTime0","2023-03-30T08:36:00+00:00"},
                }
            };

            await _repo.UpdateExecutionStatsAsync(testStore, workflowId);

            _collection.Verify(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<ExecutionStats>>(),
                It.IsAny<UpdateDefinition<ExecutionStats>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecutionStats_Get_Stats_Should_Filter()
        {
            var results = new List<ExecutionStats>() {
                new ExecutionStats { Status = TaskExecutionStatus.Accepted.ToString() },
                 new ExecutionStats { Status = TaskExecutionStatus.Succeeded.ToString() }
            };

            var mockasynccursor = new Mock<IAsyncCursor<ExecutionStats>>();
            _collection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<ExecutionStats>>(),
                It.IsAny<FindOptions<ExecutionStats>>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(mockasynccursor.Object);

            var result = await _repo.GetStatsAsync(new DateTime(), new DateTime(), 10, 1, "", "");

        }

        [Fact]
        public void ExecutionStats_New_TaskCancellationEvent_Should_initialize()
        {
            var collerationId = "colleration";
            var workflowInstanceId = "WorkflowInstanceId";
            var taskId = "TaskId";
            var executionId = "ExecutionId";
            var workflowId = "workflowId";

            var stats = new ExecutionStats(
                new TaskCancellationEvent
                {
                    WorkflowInstanceId = workflowInstanceId,
                    TaskId = taskId,
                    ExecutionId = executionId,
                }, workflowId, collerationId);


            Assert.Equal(collerationId, stats.CorrelationId);
            Assert.Equal(workflowInstanceId, stats.WorkflowInstanceId);
            Assert.Equal(taskId, stats.TaskId);
            Assert.Equal(executionId, stats.ExecutionId);
            Assert.Equal(TaskExecutionStatus.Failed.ToString(), stats.Status);
        }

        [Fact]
        public async Task UpdateExecutionStatsAsync_CanceledEvent_Should_Update_All()
        {

            var testStore = new TaskCancellationEvent
            {
                ExecutionId = nameof(TaskDispatchEvent.ExecutionId),
                WorkflowInstanceId = nameof(TaskDispatchEvent.WorkflowInstanceId),
                TaskId = nameof(TaskDispatchEvent.TaskId),
            };
            var collerationId = "collerationId";
            var workflowId = "workflowId";

            await _repo.UpdateExecutionStatsAsync(testStore, workflowId, collerationId);

            _collection.Verify(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<ExecutionStats>>(),
                It.IsAny<UpdateDefinition<ExecutionStats>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetStatsStatusCountAsync_Should_Call_Count()
        {
            var starttime = new DateTime(2023, 4, 1);
            var endtime = new DateTime(2023, 4, 10);

            await _repo.GetStatsStatusCountAsync(starttime, endtime);

            _collection.Verify(c => c.CountDocumentsAsync(
                It.IsAny<FilterDefinition<ExecutionStats>>(),
                It.IsAny<CountOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
