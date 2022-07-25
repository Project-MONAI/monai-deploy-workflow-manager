/*
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

using System;
using System.Collections.Generic;
using System.Net;
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
    public class WorkflowsInstanceControllerTests
    {
        private WorkflowInstanceController WorkflowInstanceController { get; set; }

        private readonly Mock<IWorkflowInstanceService> _workflowInstanceService;
        private readonly Mock<ILogger<WorkflowInstanceController>> _logger;
        private readonly Mock<IUriService> _uriService;
        private readonly IOptions<WorkflowManagerOptions> _options;

        public WorkflowsInstanceControllerTests()
        {
            _options = Options.Create(new WorkflowManagerOptions());
            _workflowInstanceService = new Mock<IWorkflowInstanceService>();
            _logger = new Mock<ILogger<WorkflowInstanceController>>();
            _uriService = new Mock<IUriService>();

            WorkflowInstanceController = new WorkflowInstanceController(_workflowInstanceService.Object, _logger.Object, _uriService.Object, _options);
        }

        [Fact]
        public async Task GetListAsync_WorkflowInstancesExist_ReturnsList()
        {
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
                        new TaskExecution
                        {
                            TaskId = Guid.NewGuid().ToString(),
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            };

            _workflowInstanceService.Setup(w => w.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Status?>())).ReturnsAsync(() => workflowsInstances);
            _workflowInstanceService.Setup(w => w.CountAsync()).ReturnsAsync(workflowsInstances.Count);
            _uriService.Setup(s => s.GetPageUriString(It.IsAny<Filter.PaginationFilter>(), It.IsAny<string>())).Returns(() => "unitTest");

            var result = await WorkflowInstanceController.GetListAsync(new Filter.PaginationFilter());

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

        [Fact]
        public async Task GetListAsync_ServiceException_ReturnProblem()
        {
            _workflowInstanceService.Setup(w => w.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Status>())).ThrowsAsync(new Exception());
            _workflowInstanceService.Setup(w => w.CountAsync()).ReturnsAsync(0);

            var result = await WorkflowInstanceController.GetListAsync(new Filter.PaginationFilter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetByIdAsync_WorkflowInstancesExist_ReturnsOk()
        {
            var workflowsInstance = new WorkflowInstance
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = Guid.NewGuid().ToString(),
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
            };

            _workflowInstanceService.Setup(w => w.GetByIdAsync(workflowsInstance.WorkflowId)).ReturnsAsync(workflowsInstance);

            var result = await WorkflowInstanceController.GetByIdAsync(workflowsInstance.WorkflowId);

            var objectResult = Assert.IsType<OkObjectResult>(result);

            objectResult.Value.Should().BeEquivalentTo(workflowsInstance);
        }

        [Fact]
        public async Task GetByIdAsync_WorkflowInstanceDoesNotExist_ReturnsNotFound()
        {
            var workflowId = Guid.NewGuid().ToString();

            var result = await WorkflowInstanceController.GetByIdAsync(workflowId);

            var objectResult = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            var workflowId = "2";

            var result = await WorkflowInstanceController.GetByIdAsync(workflowId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetByIdAsync_ServiceException_ReturnProblem()
        {
            var workflowId = Guid.NewGuid().ToString();
            _workflowInstanceService.Setup(w => w.GetByIdAsync(workflowId)).ThrowsAsync(new Exception());

            var result = await WorkflowInstanceController.GetByIdAsync(workflowId);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
        }

    }
}
