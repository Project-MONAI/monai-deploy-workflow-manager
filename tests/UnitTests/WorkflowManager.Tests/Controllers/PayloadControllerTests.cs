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
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Exceptions;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.ControllersShared;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;
using Moq;
using Xunit;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Filter;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace Monai.Deploy.WorkflowManager.Common.Test.Controllers
{
    public class PayloadControllerTests
    {
        private PayloadsController PayloadController { get; set; }

        private readonly Mock<IPayloadService> _payloadService;
        private readonly Mock<ILogger<PayloadsController>> _logger;
        private readonly Mock<IUriService> _uriService;
        private readonly IOptions<WorkflowManagerOptions> _options;

        public PayloadControllerTests()
        {
            _options = Options.Create(new WorkflowManagerOptions());
            _payloadService = new Mock<IPayloadService>();
            _logger = new Mock<ILogger<PayloadsController>>();
            _uriService = new Mock<IUriService>();

            PayloadController = new PayloadsController(_payloadService.Object, _logger.Object, _uriService.Object, _options);
        }

        [Fact]
        public async Task GetListAsync_PayloadsExist_ReturnsList()
        {
            var payloads = new List<PayloadDto>
            {
                new PayloadDto
                {
                    Id = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                }
            };

            _payloadService.Setup(w => w.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payloads);
            _payloadService.Setup(w => w.CountAsync(Builders<Payload>.Filter.Empty)).ReturnsAsync(payloads.Count);
            _uriService.Setup(s => s.GetPageUriString(It.IsAny<PaginationFilter>(), It.IsAny<string>())).Returns(() => "unitTest");

            var result = await PayloadController.GetAllAsync(new PaginationFilter());

            var objectResult = Assert.IsType<OkObjectResult>(result);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var responseValue = (PagedResponse<IEnumerable<PayloadDto>>)objectResult.Value;

            responseValue.Data.Should().BeEquivalentTo(payloads);
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
        public async Task GetAllAsync_WithFilter_CallsPayloadServiceCountAsyncWithFilter()
        {

            // Arrange
            var filter = new PaginationFilter
            {
                PageNumber = 1,
                PageSize = 10
            };
            var patientId = "123";
            var patientName = "John";
            var accessionId = "456";

            var pagedData = new List<PayloadDto>();
            var dataTotal = 5;

            FilterDefinition<Payload> capturedFilter = null; // Declare a variable to capture the filter

            _payloadService.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(pagedData);

            _payloadService.Setup(x => x.CountAsync(It.IsAny<FilterDefinition<Payload>>()))
                .Callback<FilterDefinition<Payload>>(filter => capturedFilter = filter) // Capture the filter
                .ReturnsAsync(dataTotal);

            // Act
            var result = await PayloadController.GetAllAsync(filter, patientId, patientName, accessionId);

            // Assert
            _payloadService.Verify(x => x.CountAsync(It.IsAny<FilterDefinition<Payload>>()), Times.Once);
            Assert.NotNull(capturedFilter); // Assert that the filter was captured
            var json = capturedFilter.Render(BsonSerializer.SerializerRegistry.GetSerializer<Payload>(), BsonSerializer.SerializerRegistry);
            Assert.Contains(patientId, json.ToString());
            Assert.Contains(patientName, json.ToString());
            Assert.Contains(accessionId, json.ToString());
            Assert.Contains("PatientDetails.PatientId", json.ToString());
            Assert.Contains("PatientDetails.PatientName", json.ToString());
            Assert.Contains("AccessionId", json.ToString());
        }

        [Fact]
        public async Task GetListAsync_ServiceException_ReturnProblem()
        {
            _payloadService.Setup(w => w.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());

            var result = await PayloadController.GetAllAsync(new PaginationFilter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);

            const string expectedInstance = "/payload";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task GetByIdAsync_PayloadExists_ReturnsOk()
        {
            var payloadId = Guid.NewGuid().ToString();
            var payload = new Payload
            {
                Id = Guid.NewGuid().ToString(),
                PayloadId = payloadId,
            };

            _payloadService.Setup(w => w.GetByIdAsync(payloadId)).ReturnsAsync(payload);

            var result = await PayloadController.GetAsync(payloadId);

            var objectResult = Assert.IsType<OkObjectResult>(result);

            objectResult.Value.Should().BeEquivalentTo(payload);
        }

        [Fact]
        public async Task GetByIdAsync_PayloadDoesNotExist_ReturnsNotFound()
        {
            var payloadId = Guid.NewGuid().ToString();

            var result = await PayloadController.GetAsync(payloadId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            var responseValue = (ProblemDetails)objectResult.Value;
            var expectedErrorMessage = $"Failed to find payload with payload id: {payloadId}";
            responseValue.Detail.Should().BeEquivalentTo(expectedErrorMessage);

            Assert.Equal((int)HttpStatusCode.NotFound, responseValue.Status);

            const string expectedInstance = "/payload";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            var payloadId = "2";

            var result = await PayloadController.GetAsync(payloadId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);

            const string expectedInstance = "/payload";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task GetByIdAsync_ServiceException_ReturnProblem()
        {
            var payloadId = Guid.NewGuid().ToString();
            _payloadService.Setup(w => w.GetByIdAsync(payloadId)).ThrowsAsync(new Exception());

            var result = await PayloadController.GetAsync(payloadId);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);

            const string expectedInstance = "/payload";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsAccepted()
        {
            var payloadId = Guid.NewGuid().ToString();

            _payloadService.Setup(w => w.DeletePayloadFromStorageAsync(payloadId)).ReturnsAsync(true);

            var result = await PayloadController.DeleteAsync(payloadId);

            Assert.IsType<AcceptedResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_InvalidPayloadId_ReturnsProblem()
        {
            var payloadId = "invalid";

            var result = await PayloadController.DeleteAsync(payloadId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);

            const string expectedInstance = "/payload";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task DeleteAsync_NoPayloadFound_ReturnsProblem()
        {
            var payloadId = Guid.NewGuid().ToString();

            _payloadService.Setup(w => w.DeletePayloadFromStorageAsync(payloadId)).ThrowsAsync(new MonaiNotFoundException());

            var result = await PayloadController.DeleteAsync(payloadId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);

            const string expectedInstance = "/payload";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task DeleteAsync_PayloadDeletedInProgress_ReturnsProblem()
        {
            var payloadId = Guid.NewGuid().ToString();

            _payloadService.Setup(w => w.DeletePayloadFromStorageAsync(payloadId)).ThrowsAsync(new MonaiBadRequestException());

            var result = await PayloadController.DeleteAsync(payloadId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);

            const string expectedInstance = "/payload";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task DeleteAsync_ErrorThrown_ReturnsProblem()
        {
            var payloadId = Guid.NewGuid().ToString();

            _payloadService.Setup(w => w.DeletePayloadFromStorageAsync(payloadId)).ThrowsAsync(new Exception());

            var result = await PayloadController.DeleteAsync(payloadId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);

            const string expectedInstance = "/payload";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }
    }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
}
