/*
 * Copyright 2023 MONAI Consortium
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
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.ControllersShared;
using Monai.Deploy.WorkflowManager.Common.Models;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;
using Moq;
using Xunit;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Filter;

namespace Monai.Deploy.WorkflowManager.Common.Test.Controllers
{
    public class TasksControllerTests
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        private TasksController TasksController { get; set; }

        private readonly Mock<ITasksService> _tasksService;
        private readonly Mock<ILogger<TasksController>> _logger;
        private readonly Mock<IUriService> _uriService;
        private readonly IOptions<WorkflowManagerOptions> _options;

        public TasksControllerTests()
        {
            _options = Options.Create(new WorkflowManagerOptions());
            _tasksService = new Mock<ITasksService>();
            _logger = new Mock<ILogger<TasksController>>();
            _uriService = new Mock<IUriService>();

            TasksController = new TasksController(_tasksService.Object, _logger.Object, _uriService.Object, _options);
        }

        [Fact]
        public async Task GetListAsync_TasksExist_ReturnsList()
        {
            var taskExecution = new TaskExecution
            {
                TaskId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Dispatched
            };
            var workflowsInstances = new List<WorkflowInstance>
            {
                new WorkflowInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Status = Status.Created,
                    BucketId = "bucket",
                    Tasks = new List<TaskExecution>
                    {
                        taskExecution
                    }
                }
            };
            _tasksService.Setup(w => w.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(() => (Tasks: new List<TaskExecution> { taskExecution }, Count: 1));
            _uriService.Setup(s => s.GetPageUriString(It.IsAny<PaginationFilter>(), It.IsAny<string>())).Returns(() => "unitTest");

            var result = await TasksController.GetListAsync(new PaginationFilter());

            var objectResult = Assert.IsType<OkObjectResult>(result);

            var responseValue = (PagedResponse<IEnumerable<TaskExecution>>)objectResult.Value;
            responseValue.Data.Should().BeEquivalentTo(workflowsInstances.First().Tasks);
            responseValue.FirstPage.Should().Be("unitTest");
            responseValue.LastPage.Should().Be("unitTest");
            responseValue.PageNumber.Should().Be(1);
            responseValue.PageSize.Should().Be(10);
            responseValue.TotalPages.Should().Be(1);
            responseValue.TotalRecords.Should().Be(1);
            responseValue.Succeeded.Should().Be(true);
            responseValue.PreviousPage.Should().Be(null);
            responseValue.NextPage.Should().Be(null);
            responseValue.Errors.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task GetAsync_TasksExist_ReturnsTask()
        {
            var expectedTaskId = Guid.NewGuid().ToString();
            var expectedExecutionId = Guid.NewGuid().ToString();
            var expectedWorkflowId = Guid.NewGuid().ToString();

            var taskExecution = new TaskExecution
            {
                ExecutionId = expectedExecutionId,
                TaskId = expectedTaskId,
                Status = TaskExecutionStatus.Dispatched
            };

            var workflowsInstances = new List<WorkflowInstance>
            {
                new WorkflowInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = expectedWorkflowId,
                    PayloadId = Guid.NewGuid().ToString(),
                    Status = Status.Created,
                    BucketId = "bucket",
                    Tasks = new List<TaskExecution>
                    {
                        taskExecution
                    }
                }
            };

            _tasksService.Setup(w => w.GetTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => taskExecution);
            var request = new TasksRequest()
            {
                ExecutionId = expectedWorkflowId,
                TaskId = expectedTaskId,
                WorkflowInstanceId = expectedExecutionId
            };

            var result = await TasksController.GetAsync(request);

            var objectResult = Assert.IsType<OkObjectResult>(result);

            var responseValue = (TaskExecution)objectResult.Value;

            responseValue.Should().BeEquivalentTo(workflowsInstances.First().Tasks.First(t => t.TaskId == expectedTaskId));
        }

        [Fact]
        public async Task GetAsync_InvalidId_ReturnsBadRequest()
        {
            var invalidId = string.Empty;
            var invalidRequest = new TasksRequest()
            {
                ExecutionId = invalidId,
                TaskId = invalidId,
                WorkflowInstanceId = invalidId
            };
            var result = await TasksController.GetAsync(invalidRequest);

            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be(400);

            var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
            problemDetails.Detail.Should().Be("Failed to validate ids, not a valid guid");

            const string expectedInstance = "/tasks";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task GetAsync_TaskDoesNotExist_ReturnsBadRequest()
        {
            var expectedTaskId = Guid.NewGuid().ToString(); // that does not exist
            var expectedExecutionId = Guid.NewGuid().ToString();
            var expectedWorkflowId = Guid.NewGuid().ToString();
            var invalidRequest = new TasksRequest()
            {
                ExecutionId = expectedWorkflowId,
                TaskId = expectedTaskId,
                WorkflowInstanceId = expectedExecutionId
            };

            _tasksService.Setup(w => w.GetTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);

            var result = await TasksController.GetAsync(invalidRequest);

            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be(404);

            var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
            problemDetails.Detail.Should().Be("Failed to validate ids, workflow or task not found");

            const string expectedInstance = "/tasks";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }
    }
}
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
