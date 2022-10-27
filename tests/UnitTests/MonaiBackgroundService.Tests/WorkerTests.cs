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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Moq;

namespace Monai.Deploy.WorkflowManager.MonaiBackgroundService.Tests
{
    public class WorkerTests
    {
        private const string IdentityKey = "IdentityKey";
        private readonly Worker _service;
        private readonly Mock<IMessageBrokerPublisherService> _pubService;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly Mock<ITasksRepository> _repo;

        public WorkerTests()
        {
            var logger = new Mock<ILogger<Worker>>();
            _repo = new Mock<ITasksRepository>();
            var taskService = new TasksService(_repo.Object);
            _pubService = new Mock<IMessageBrokerPublisherService>();
            _options = Options.Create(new WorkflowManagerOptions());
            _service = new Worker(logger.Object, taskService, _pubService.Object, _options);
        }

        [Fact]
        public void MonaiBackgroundService_ServiceName()
        {
            var expectedServiceName = "Monai Background Service";
            Assert.Equal(Worker.ServiceName, expectedServiceName);
        }

        [Fact]
        public async Task MonaiBackgroundService_DoWork_ShouldPublishMessages()
        {
            var expectedTaskId = Guid.NewGuid().ToString();
            var expectedExecutionId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();

            var taskExecution = new List<TaskExecution> {
                new TaskExecution
                {
                    ExecutionId = expectedExecutionId,
                    TaskId = expectedTaskId,
                    WorkflowInstanceId = workflowInstanceId,
                    Status = TaskExecutionStatus.Dispatched,
                    TimeoutInterval = -2,
                    TaskStartTime = DateTime.UtcNow,
                    ExecutionStats = new Dictionary<string, string>()
                    {
                        { IdentityKey, Guid.NewGuid().ToString() }
                    }
                }
            };

            _pubService.Setup(p => p.Publish(It.IsAny<string>(), It.IsAny<Message>())).Returns(Task.CompletedTask);
            _repo.Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(() => (Tasks: taskExecution, Count: 1));
            var tokenSource = new CancellationTokenSource();

            await _service.DoWork();

            // Verify DoWork publishes TaskCancellationRequest
            _pubService.Verify(p => p.Publish(It.Is<string>(m => m == _options.Value.Messaging.Topics.TaskCancellationRequest), It.IsAny<Message>()), Times.Once());
            // Verify DoWork publishes TaskUpdateRequest
            _pubService.Verify(p => p.Publish(It.Is<string>(m => m == _options.Value.Messaging.Topics.TaskUpdateRequest), It.IsAny<Message>()), Times.Once());

            Assert.False(_service.IsRunning);
        }
    }
}
