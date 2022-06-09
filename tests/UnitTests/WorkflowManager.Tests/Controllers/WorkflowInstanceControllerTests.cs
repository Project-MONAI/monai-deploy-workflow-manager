// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.Controllers;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Test.Controllers
{
    public class WorkflowsInstanceControllerTests
    {
        private WorkflowInstanceController WorkflowInstanceController { get; set; }

        private readonly Mock<IWorkflowInstanceService> _workflowInstanceService;
        private readonly Mock<ILogger<WorkflowInstanceController>> _logger;

        public WorkflowsInstanceControllerTests()
        {
            _workflowInstanceService = new Mock<IWorkflowInstanceService>();
            _logger = new Mock<ILogger<WorkflowInstanceController>>();

            WorkflowInstanceController = new WorkflowInstanceController(_workflowInstanceService.Object, _logger.Object);
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

            _workflowInstanceService.Setup(w => w.GetListAsync()).ReturnsAsync(workflowsInstances);

            var result = await WorkflowInstanceController.GetListAsync();

            var objectResult = Assert.IsType<OkObjectResult>(result);

            objectResult.Value.Should().BeEquivalentTo(workflowsInstances);
        }

        [Fact]
        public async Task GetListAsync_ServiceException_ReturnProblem()
        {
            _workflowInstanceService.Setup(w => w.GetListAsync()).ThrowsAsync(new Exception());

            var result = await WorkflowInstanceController.GetListAsync();

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
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

            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            var workflowId = "2";

            var result = await WorkflowInstanceController.GetByIdAsync(workflowId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(400, objectResult.StatusCode);
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
