// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Controllers;
using Monai.Deploy.WorkflowManager.Services;
using Monai.Deploy.WorkflowManager.Wrappers;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Test.Controllers
{
    public class TasksControllerTests
    {
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
        public async Task GetListAsync_WorkflowInstancesExist_ReturnsList()
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

            _tasksService.Setup(w => w.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(() => new List<TaskExecution> { taskExecution });
            _tasksService.Setup(w => w.CountAsync()).ReturnsAsync(workflowsInstances.First().Tasks.Count);
            _uriService.Setup(s => s.GetPageUriString(It.IsAny<Filter.PaginationFilter>(), It.IsAny<string>())).Returns(() => "unitTest");

            var result = await TasksController.GetListAsync(new Filter.PaginationFilter());

            var objectResult = Assert.IsType<OkObjectResult>(result);

            var responseValue = (PagedResponse<List<WorkflowInstance>>)objectResult.Value;
            responseValue.Data.Should().BeEquivalentTo(workflowsInstances);
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
    }
}
