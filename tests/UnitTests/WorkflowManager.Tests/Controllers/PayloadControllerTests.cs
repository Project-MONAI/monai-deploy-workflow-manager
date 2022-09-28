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
            var payloads = new List<Payload>
            {
                new Payload
                {
                    Id = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                }
            };

            _payloadService.Setup(w => w.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payloads);
            _payloadService.Setup(w => w.CountAsync()).ReturnsAsync(payloads.Count);
            _uriService.Setup(s => s.GetPageUriString(It.IsAny<Filter.PaginationFilter>(), It.IsAny<string>())).Returns(() => "unitTest");

            var result = await PayloadController.GetAllAsync(new Filter.PaginationFilter());

            var objectResult = Assert.IsType<OkObjectResult>(result);

            var responseValue = (PagedResponse<List<Payload>>)objectResult.Value;
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
        public async Task GetListAsync_ServiceException_ReturnProblem()
        {
            _payloadService.Setup(w => w.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());

            var result = await PayloadController.GetAllAsync(new Filter.PaginationFilter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
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
            string expectedErrorMessage = $"Failed to find payload with payload id: {payloadId}";
            responseValue.Detail.Should().BeEquivalentTo(expectedErrorMessage);

            Assert.Equal((int)HttpStatusCode.NotFound, responseValue.Status);
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            var payloadId = "2";

            var result = await PayloadController.GetAsync(payloadId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetByIdAsync_ServiceException_ReturnProblem()
        {
            var payloadId = Guid.NewGuid().ToString();
            _payloadService.Setup(w => w.GetByIdAsync(payloadId)).ThrowsAsync(new Exception());

            var result = await PayloadController.GetAsync(payloadId);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
        }

    }
}
