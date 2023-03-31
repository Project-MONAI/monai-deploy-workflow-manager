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
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Monai.Deploy.WorkflowManager.TaskManager.Database;
using Monai.Deploy.WorkflowManager.TaskManager.Database.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.TaskManager.Tests
{
    public class TaskExecutionStatsRepositoryTests
    {
        private readonly Mock<ILogger<TaskExecutionStatsRepository>> _logger;
        private readonly Mock<IMongoClient> _client;
        private readonly IOptions<TaskExecutionDatabaseSettings> _options;
        private readonly Mock<IMongoDatabase> _dbase;
        private readonly Mock<IMongoCollection<TaskExecutionStats>> _collection;
        private readonly TaskExecutionStatsRepository _repo;

        public TaskExecutionStatsRepositoryTests()
        {
            _logger = new Mock<ILogger<TaskExecutionStatsRepository>>();
            _client = new Mock<IMongoClient>(MockBehavior.Strict);
            _options = Options.Create(new TaskExecutionDatabaseSettings() { DatabaseName = "dbName" });
            _dbase = new Mock<IMongoDatabase>();
            _collection = new Mock<IMongoCollection<TaskExecutionStats>>();

            var IndexDoc = new BsonDocument(new Dictionary<string, string> { { "name", "ExecutionStatsIndex" } });
            var indexList = Task.FromResult(new List<BsonDocument>() { IndexDoc });

            var cursor = new Mock<IAsyncCursor<BsonDocument>>();

            _dbase.Setup(d => d.GetCollection<TaskExecutionStats>(It.IsAny<string>(), null)).Returns(_collection.Object);
            _client.Setup(c => c.GetDatabase(It.IsAny<string>(), null)).Returns(_dbase.Object);
            _collection.Setup(c => c.Indexes.ListAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(cursor.Object));

            _repo = new TaskExecutionStatsRepository(_client.Object, _options, _logger.Object);
        }

        [Fact]
        public void TaskExecutionStats_Should_Contain_All_Fields()
        {
            const string workflowId = nameof(workflowId);
            const string correlationId = nameof(correlationId);
            const string taskId = nameof(taskId);
            const string executionId = nameof(executionId);
            var started = DateTime.Now;

            var testObj = new TaskDispatchEventInfo
            (
                new TaskDispatchEvent
                {
                    CorrelationId = correlationId,
                    TaskId = taskId,
                    ExecutionId = executionId,
                    WorkflowInstanceId = workflowId
                }
            );
            testObj.Started = started;

            var output = new TaskExecutionStats(testObj);

            Assert.Equal(started, output.Started);
            Assert.Equal(executionId, output.ExecutionId);
            Assert.Equal(workflowId, output.WorkflowInstanceId);
            Assert.Equal(correlationId, output.CorrelationId);
            Assert.Equal(taskId, output.TaskId);
        }

        [Fact]
        public void TaskExecutionStatsRepository_Should_Check_For_Null_Logger()
        {
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateAsync(default));
        }


        [Fact]
        public async Task TaskExecutionStatsRepository_update_Should_Check_For_Null_Event()
        {
            var service = new TaskExecutionStatsRepository(_client.Object, _options, _logger.Object);
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateExecutionStatsAsync(default));
        }

        [Fact]
        public async Task TaskExecutionStats_Create_Should_Store_All()
        {
            var testStore = new TaskDispatchEventInfo(new TaskDispatchEvent
            {
                CorrelationId = nameof(TaskDispatchEvent.CorrelationId),
                ExecutionId = nameof(TaskDispatchEvent.ExecutionId),
                WorkflowInstanceId = nameof(TaskDispatchEvent.WorkflowInstanceId),
                TaskId = nameof(TaskDispatchEvent.TaskId),
                Status = TaskExecutionStatus.Created,
            });

            await _repo.CreateAsync(testStore);

            _collection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<TaskExecutionStats>>(),
                It.Is<TaskExecutionStats>(t =>
                    t.CorrelationId == testStore.Event.CorrelationId &&
                    t.TaskId == testStore.Event.TaskId &&
                    t.Status == testStore.Event.Status.ToString() &&
                    t.WorkflowInstanceId == testStore.Event.WorkflowInstanceId &&
                    t.ExecutionId == testStore.Event.ExecutionId
                    ),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()),Times.Once);
        }

        [Fact]
        public async Task TaskExecutionStats_Update_Should_Update_All()
        {

            var testStore = new TaskUpdateEvent
            {
                CorrelationId = nameof(TaskDispatchEvent.CorrelationId),
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

            await _repo.UpdateExecutionStatsAsync(testStore);

            _collection.Verify(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<TaskExecutionStats>>(),
                It.IsAny<UpdateDefinition<TaskExecutionStats>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }


    }
}
