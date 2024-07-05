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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
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
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;
using Moq;
using Xunit;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Filter;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Common.Test.Controllers
{
    public class WorkflowsInstanceControllerTests
    {
        private WorkflowInstanceController WorkflowInstanceController { get; set; }

        private readonly Mock<IWorkflowInstanceService> _workflowInstanceService;
        private readonly Mock<ILogger<WorkflowInstanceController>> _logger;
        private readonly Mock<IUriService> _uriService;
        private readonly IOptions<WorkflowManagerOptions> _options;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        public WorkflowsInstanceControllerTests()
        {
            _options = Options.Create(new WorkflowManagerOptions());
            _workflowInstanceService = new Mock<IWorkflowInstanceService>();
            _logger = new Mock<ILogger<WorkflowInstanceController>>();
            _uriService = new Mock<IUriService>();

            _logger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
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
            _uriService.Setup(s => s.GetPageUriString(It.IsAny<PaginationFilter>(), It.IsAny<string>())).Returns(() => "unitTest");

            var result = await WorkflowInstanceController.GetListAsync(new PaginationFilter());

            var objectResult = Assert.IsType<OkObjectResult>(result);

            var responseValue = (PagedResponse<IEnumerable<WorkflowInstance>>)objectResult.Value;
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

            var result = await WorkflowInstanceController.GetListAsync(new PaginationFilter(), null, null, true);

            var objectResult = Assert.IsType<OkObjectResult>(result);

            var responseValue = (List<WorkflowInstance>)objectResult.Value;
            responseValue.Should().BeEquivalentTo(workflowsInstances);
        }

        [Fact]
        public async Task GetListAsync_InvalidPayloadId_Returns400()
        {
            var expectedErrorMessage = "Failed to validate payloadId, not a valid guid";
            var result = await WorkflowInstanceController.GetListAsync(new PaginationFilter(), null, "invalid", true);

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
            _workflowInstanceService.Setup(w => w.CountAsync(Builders<WorkflowInstance>.Filter.Empty)).ReturnsAsync(0);

            var result = await WorkflowInstanceController.GetListAsync(new PaginationFilter());

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
            var expectedErrorMessage = $"Failed to find workflow instance with Id: {workflowId}";
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

        [Fact]
        public async Task AcknowledgeTaskError_ServiceException_ReturnProblem()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();
            var executionId = Guid.NewGuid().ToString();
            _workflowInstanceService.Setup(w => w.AcknowledgeTaskError(workflowInstanceId, executionId)).ThrowsAsync(new Exception());

            var result = await WorkflowInstanceController.AcknowledgeTaskError(workflowInstanceId, executionId);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task AcknowledgeTaskError_InvalidWorkflowInstanceId_ReturnsBadRequest()
        {
            var workflowInstanceId = "2";
            var executionId = Guid.NewGuid().ToString();

            var result = await WorkflowInstanceController.AcknowledgeTaskError(workflowInstanceId, executionId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task AcknowledgeTaskError_InvalidExecutionId_ReturnsBadRequest()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();
            var executionId = "2";

            var result = await WorkflowInstanceController.AcknowledgeTaskError(workflowInstanceId, executionId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task AcknowledgeTaskError_WorkflowInstanceUpdated_ReturnsOk()
        {
            var workflowsInstance = new WorkflowInstance
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Failed,
                AcknowledgedWorkflowErrors = DateTime.UtcNow,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = Guid.NewGuid().ToString(),
                            ExecutionId = Guid.NewGuid().ToString(),
                            Status = TaskExecutionStatus.Failed,
                            AcknowledgedTaskErrors = DateTime.UtcNow
                        }
                    }
            };

            _workflowInstanceService.Setup(w => w.AcknowledgeTaskError(workflowsInstance.WorkflowId, workflowsInstance.Tasks.First().ExecutionId)).ReturnsAsync(workflowsInstance);

            var result = await WorkflowInstanceController.AcknowledgeTaskError(workflowsInstance.WorkflowId, workflowsInstance.Tasks.First().ExecutionId);

            var objectResult = Assert.IsType<OkObjectResult>(result);

            objectResult.Value.Should().BeEquivalentTo(workflowsInstance);
        }

        [Theory]
        [InlineData("2022-02-21", "en-GB")]
        [InlineData("2022-02-21", "en-US")]
        [InlineData("2022-02-21", "fil-PH")]
        [InlineData("2022-02-21", "de-DE")]
        [InlineData("2022-02-21", "sk-SK")]
        [InlineData("2022-02-21", "fr-FR")]
        [InlineData("2022-02-21", "hi-IN")]
        [InlineData("2022-02-21", "de-CH")]
        [InlineData("2022-02-21T00:00", "en-GB")]
        [InlineData("2022-02-21T00:00", "en-US")]
        [InlineData("2022-02-21T00:00", "fil-PH")]
        [InlineData("2022-02-21T00:00", "de-DE")]
        [InlineData("2022-02-21T00:00", "sk-SK")]
        [InlineData("2022-02-21T00:00", "fr-FR")]
        [InlineData("2022-02-21T00:00", "hi-IN")]
        [InlineData("2022-02-21T00:00", "de-CH")]
        [InlineData("21-02-2022", "en-GB")]
        [InlineData("21-02-2022", "de-DE")]
        [InlineData("21-02-2022", "sk-SK")]
        [InlineData("21-02-2022", "fr-FR")]
        [InlineData("21-02-2022", "hi-IN")]
        [InlineData("21-02-2022", "de-CH")]
        [InlineData("21-02-2022", "es-ES")]
        [InlineData("02-21-2022", "en-US")]
        [InlineData("02-02-2022", "en-GB")]
        [InlineData("02-12-1971", "en-GB")]
        [InlineData("02-12-1971", "fil-PH")]
        [InlineData("10-1980", "en-GB")]
        [InlineData("01-01", "en-GB")]
        [InlineData("15-12-2021", "en-GB")]
        public async Task TaskGetFailedAsync_GivenCorrectDateString_ReturnsWorkflows(string inputString, string controlerCulture)
        {
            var input = DateTime.Parse(inputString, new CultureInfo(controlerCulture));
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
            _workflowInstanceService.Setup(w => w.GetAllFailedAsync())
                .ReturnsAsync(expectedResponse);

            Thread.CurrentThread.CurrentCulture = new CultureInfo(controlerCulture);
            var result = await WorkflowInstanceController.GetFailedAsync();

            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
        }

        [Fact]
        public async Task TaskGetFailedAsync_GivenGetAllFailedAsyncReturnsNoResults_ReturnsEmptyList()
        {
            _workflowInstanceService.Setup(w => w.GetAllFailedAsync())
                .ReturnsAsync(new List<WorkflowInstance>() { });

            var result = await WorkflowInstanceController.GetFailedAsync();

            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            objectResult.Value.Should().BeEquivalentTo(new List<WorkflowInstance>() { });
        }

        [Fact]
        public async Task TaskGetFailedAsync_GivenGetAllFailedAsyncReturnsThrowsException_ReturnsInternalServiceError()
        {
            _workflowInstanceService.Setup(w => w.GetAllFailedAsync()).ThrowsAsync(new Exception());

            var result = await WorkflowInstanceController.GetFailedAsync();

            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            var responseValue = (ProblemDetails)objectResult.Value;

            responseValue.Status.Should().Be((int)HttpStatusCode.InternalServerError);

            var problemMessage = "Unexpected error occurred.";
            responseValue.Detail.Should().Be(problemMessage);

            const string expectedInstance = "/workflowinstances";
            var expectedErrorMessage = "Unexpected error occurred in GET /workflowinstances/failed API.";

            Assert.StartsWith(expectedInstance, responseValue.Instance);

            _logger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 100006),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == expectedErrorMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
}
