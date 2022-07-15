// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Controllers;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Test.Controllers
{
    public class PayloadControllerTests
    {
        private PayloadController PayloadController { get; set; }

        private readonly Mock<IPayloadService> _payloadService;
        private readonly Mock<ILogger<PayloadController>> _logger;

        public PayloadControllerTests()
        {
            _payloadService = new Mock<IPayloadService>();
            _logger = new Mock<ILogger<PayloadController>>();

            PayloadController = new PayloadController(_payloadService.Object, _logger.Object);
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

            _payloadService.Setup(w => w.GetAllAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payloads);

            var result = await PayloadController.GetAllAsync();

            var objectResult = Assert.IsType<OkObjectResult>(result);

            objectResult.Value.Should().BeEquivalentTo(payloads);
        }

        [Fact]
        public async Task GetListAsync_PayloadsExist_ReturnsOrderedList()
        {
            var payloads = new List<Payload>
            {
                new Payload
                {
                    Id = "rainbow",
                    PayloadId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                },
                new Payload
                {
                    Id = "unicorn",
                    PayloadId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now.AddMinutes(-5),
                },
                new Payload
                {
                    Id = "sparkles",
                    PayloadId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now.AddMinutes(-2),
                },
            };

            _payloadService.Setup(w => w.GetAllAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payloads);

            var result = await PayloadController.GetAllAsync();

            var objectResult = Assert.IsType<OkObjectResult>(result);

            var payloadResult = (IOrderedEnumerable<Payload>)objectResult.Value;
            var first = payloadResult.First().Id;
            var second = payloadResult.Skip(1).First().Id;
            Assert.True(first == "rainbow");
            Assert.True(second == "sparkles");
            objectResult.Value.Should().BeEquivalentTo(payloads);
        }

        [Fact]
        public async Task GetListAsync_ServiceException_ReturnProblem()
        {
            _payloadService.Setup(w => w.GetAllAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());

            var result = await PayloadController.GetAllAsync();

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

            var objectResult = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
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
