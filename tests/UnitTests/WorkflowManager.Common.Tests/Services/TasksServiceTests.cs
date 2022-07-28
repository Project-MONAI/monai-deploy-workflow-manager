// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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
