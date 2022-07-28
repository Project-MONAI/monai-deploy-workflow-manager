﻿/*
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

using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Test.Services
{
    public class TasksServiceTests
    {
        private readonly Mock<ITasksRepository> _tasksRepository;

        private ITasksService TasksService { get; set; }

        public TasksServiceTests()
        {
            _tasksRepository = new Mock<ITasksRepository>();

            TasksService = new TasksService(_tasksRepository.Object);
        }

        [Fact]
        public async Task GetListAsync_TasksExist_ReturnsList()
        {
            var expectedTaskId = Guid.NewGuid().ToString();
            var expectedExecutionId = Guid.NewGuid().ToString();
            var expectedWorkflowId = Guid.NewGuid().ToString();

            var taskExecution = new List<TaskExecution> {
                new TaskExecution
                {
                    ExecutionId = expectedExecutionId,
                    TaskId = expectedTaskId,
                    Status = TaskExecutionStatus.Dispatched
                }
            };

            _tasksRepository.Setup(tr => tr.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(() => taskExecution);

            var result = await TasksService.GetAllAsync();

            result.Should().NotBeNull();
            result.Count.Should().Be(1);

            var objectResult = Assert.IsType<TaskExecution>(result[0]);

            objectResult.Should().NotBeNull();
            objectResult.ExecutionId.Should().Be(expectedExecutionId);
            objectResult.TaskId.Should().Be(expectedTaskId);
            objectResult.Status.Should().Be(TaskExecutionStatus.Dispatched);
        }
    }
}
