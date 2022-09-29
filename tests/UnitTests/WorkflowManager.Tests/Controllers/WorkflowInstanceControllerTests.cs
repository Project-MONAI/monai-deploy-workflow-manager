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
using System.Globalization;
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

            _workflowInstanceService.Setup(w => w.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Status?>(), It.IsAny<string>())).ReturnsAsync(() => workflowsInstances);
            _workflowInstanceService.Setup(w => w.FilteredCountAsync(It.IsAny<Status?>(), It.IsAny<string>())).ReturnsAsync(workflowsInstances.Count);
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
        public async Task GetListAsync_PaginationDisabled_ReturnsList()
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

            _workflowInstanceService.Setup(w => w.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Status?>(), It.IsAny<string>())).ReturnsAsync(() => workflowsInstances);

            var result = await WorkflowInstanceController.GetListAsync(new Filter.PaginationFilter(), null, null, true);

            var objectResult = Assert.IsType<OkObjectResult>(result);

            var responseValue = (List<WorkflowInstance>)objectResult.Value;
            responseValue.Should().BeEquivalentTo(workflowsInstances);
        }

        [Fact]
        public async Task GetListAsync_InvalidPayloadId_Returns400()
        {
            var expectedErrorMessage = "Failed to validate payloadId, not a valid guid";
            var result = await WorkflowInstanceController.GetListAsync(new Filter.PaginationFilter(), null, "invalid", true);

            var objectResult = Assert.IsType<ObjectResult>(result);

            var responseValue = (ProblemDetails)objectResult.Value;
            responseValue.Detail.Should().BeEquivalentTo(expectedErrorMessage);

            const string expectedInstance = "/workflowinstances";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task GetListAsync_ServiceException_ReturnProblem()
        {
            _workflowInstanceService.Setup(w => w.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Status>(), It.IsAny<string>())).ThrowsAsync(new Exception());
            _workflowInstanceService.Setup(w => w.CountAsync()).ReturnsAsync(0);

            var result = await WorkflowInstanceController.GetListAsync(new Filter.PaginationFilter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);

            const string expectedInstance = "/workflowinstances";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
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

            var objectResult = Assert.IsType<ObjectResult>(result);

            var responseValue = (ProblemDetails)objectResult.Value;
            string expectedErrorMessage = $"Failed to find workflow instance with Id: {workflowId}";
            responseValue.Detail.Should().BeEquivalentTo(expectedErrorMessage);

            Assert.Equal((int)HttpStatusCode.NotFound, responseValue.Status);

            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);

            const string expectedInstance = "/workflowinstances";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            var workflowId = "2";

            var result = await WorkflowInstanceController.GetByIdAsync(workflowId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);

            const string expectedInstance = "/workflowinstances";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task GetByIdAsync_ServiceException_ReturnProblem()
        {
            var workflowId = Guid.NewGuid().ToString();
            _workflowInstanceService.Setup(w => w.GetByIdAsync(workflowId)).ThrowsAsync(new Exception());

            var result = await WorkflowInstanceController.GetByIdAsync(workflowId);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);

            const string expectedInstance = "/workflowinstances";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Theory]
        [InlineData("21-02-2022")]
        [InlineData("02-02-2022")]
        [InlineData("02-12-1971")]
        [InlineData("10-1980")]
        [InlineData("01-01")]
        [InlineData("15-12-2021")]
        public async Task TaskGetFailedAsync_GivenCorrectDateString_ReturnsWorkflows(string inputString)
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

            var expectedResponse = new List<WorkflowInstance>() { workflowsInstance };
            _workflowInstanceService.Setup(w => w.GetAllFailedAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(expectedResponse);

            var result = await WorkflowInstanceController.GetFailedAsync(inputString);

            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task TaskGetFailedAsync_GivenInvalidInTheFutureDateString_ReturnsProblem()
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

            _workflowInstanceService.Setup(w => w.GetAllFailedAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkflowInstance>() { workflowsInstance });
            var inputString = DateTime.UtcNow.AddDays(5).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
            var result = await WorkflowInstanceController.GetFailedAsync(inputString);

            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            var responseValue = (ProblemDetails)objectResult.Value;
            responseValue.Status.Should().Be((int)HttpStatusCode.BadRequest);

            var startsWithException = "Failed to validate acknowledged value:";
            var endsWithException = "provided time is in the future.";
            Assert.StartsWith(startsWithException, responseValue.Detail);
            Assert.EndsWith(endsWithException, responseValue.Detail);

            const string expectedInstance = "/workflowinstances";
            Assert.StartsWith(expectedInstance, responseValue.Instance);
        }

        [Theory]
        [InlineData("", "Failed to validate, no acknowledged parameter provided")]
        [InlineData(null, "Failed to validate, no acknowledged parameter provided")]
        [InlineData("donkey", "Failed to validate provided date")]
        [InlineData("99-99-9999", "Failed to validate provided date")]
        [InlineData("00-00-0000", "Failed to validate provided date")]
        [InlineData("-01-1980", "Failed to validate provided date")]
        [InlineData("01-12-", "Failed to validate provided date")]
        [InlineData("x1-1x-198x", "Failed to validate provided date")]
        public async Task TaskGetFailedAsync_GivenInvalidDateString_ReturnsProblem(string inputString, string expectedErrorMessage)
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

            _workflowInstanceService.Setup(w => w.GetAllFailedAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkflowInstance>() { workflowsInstance });

            var result = await WorkflowInstanceController.GetFailedAsync(inputString);

            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var responseValue = (ProblemDetails)objectResult.Value;
            responseValue.Status.Should().Be((int)HttpStatusCode.BadRequest);
            responseValue.Detail.Should().Be(expectedErrorMessage);

            const string expectedInstance = "/workflowinstances";
            Assert.StartsWith(expectedInstance, responseValue.Instance);
        }

        [Fact]
        public async Task TaskGetFailedAsync_GivenGetAllFailedAsyncReturnsNoResults_ReturnsProblem()
        {
            _workflowInstanceService.Setup(w => w.GetAllFailedAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkflowInstance>() { });
            var inputString = "21-02-2022";
            var result = await WorkflowInstanceController.GetFailedAsync(inputString);

            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            var responseValue = (ProblemDetails)objectResult.Value;
            responseValue.Status.Should().Be((int)HttpStatusCode.NotFound);

            var problemMessage = "Request failed, no workflow instances found since 21-02-2022";
            responseValue.Detail.Should().Be(problemMessage);

            const string expectedInstance = "/workflowinstances";
            Assert.StartsWith(expectedInstance, responseValue.Instance);
        }

        [Fact]
        public async Task TaskGetFailedAsync_GivenGetAllFailedAsyncReturnsThrowsException_ReturnsInternalServiceError()
        {
            _workflowInstanceService.Setup(w => w.GetAllFailedAsync(It.IsAny<DateTime>())).ThrowsAsync(new Exception());
            var inputString = "21-02-2022";
            var result = await WorkflowInstanceController.GetFailedAsync(inputString);

            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            var responseValue = (ProblemDetails)objectResult.Value;
            responseValue.Status.Should().Be((int)HttpStatusCode.InternalServerError);

            var problemMessage = "Unexpected error occurred.";
            responseValue.Detail.Should().Be(problemMessage);

            const string expectedInstance = "/workflowinstances";
            var expectedErrorMessage = "GetFailedAsync - Failed to get failed workflowInstances";

            Assert.StartsWith(expectedInstance, responseValue.Instance);

            _logger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == expectedErrorMessage && @type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

        }
    }
}
