/*
 * Copyright 2022 MONAI Consortium
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

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Exceptions;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Storage.Services;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Configuration;

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous.Tests.Services
{
    public class PayloadServiceTests
    {
        private IPayloadService PayloadService { get; set; }

        private readonly Mock<IPayloadRepository> _payloadRepository;
        private readonly Mock<IWorkflowInstanceRepository> _workflowInstanceRepository;
        private readonly Mock<IWorkflowRepository> _workflowRepository;
        private readonly Mock<IDicomService> _dicomService;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
        private readonly Mock<IServiceProvider> _serviceProvider;
        private readonly Mock<IServiceScope> _serviceScope;
        private readonly Mock<IStorageService> _storageService;
        private readonly Mock<ILogger<PayloadService>> _logger;

        public PayloadServiceTests()
        {
            _payloadRepository = new Mock<IPayloadRepository>();
            _workflowInstanceRepository = new Mock<IWorkflowInstanceRepository>();
            _workflowRepository = new Mock<IWorkflowRepository>();
            _dicomService = new Mock<IDicomService>();
            _serviceProvider = new Mock<IServiceProvider>();
            _storageService = new Mock<IStorageService>();
            _logger = new Mock<ILogger<PayloadService>>();

            _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            _serviceScope = new Mock<IServiceScope>();

            _serviceScope.Setup(x => x.ServiceProvider).Returns(_serviceProvider.Object);

            _serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(_serviceScope.Object);

            _serviceProvider
                .Setup(x => x.GetService(typeof(IStorageService)))
                .Returns(_storageService.Object);

            var opts = Options.Create(new WorkflowManagerOptions { DataRetentionDays = 99 });

            PayloadService = new PayloadService(
                _payloadRepository.Object,
                _dicomService.Object,
                _workflowInstanceRepository.Object,
                _workflowRepository.Object,
                _serviceScopeFactory.Object,
                opts,
                _logger.Object);
        }

        [Fact]
        public async Task CreateAsync_ValidWorkflowAndDicom_ReturnsTrue()
        {
            var workflowRequest = new WorkflowRequestEvent
            {
                Timestamp = DateTime.UtcNow,
                Bucket = "bucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                CorrelationId = Guid.NewGuid().ToString(),
                PayloadId = Guid.NewGuid(),
                Workflows = new List<string> { Guid.NewGuid().ToString() },
                FileCount = 0
            };

            var patientDetails = new PatientDetails
            {
                PatientDob = new DateTime(1996, 02, 05),
                PatientId = Guid.NewGuid().ToString(),
                PatientName = "Steve",
                PatientSex = "male"
            };

            var expected = new Payload
            {
                Timestamp = workflowRequest.Timestamp,
                Bucket = workflowRequest.Bucket,
                FileCount = workflowRequest.FileCount,
                DataTrigger = workflowRequest.DataTrigger,
                CorrelationId = workflowRequest.CorrelationId,
                PayloadId = workflowRequest.PayloadId.ToString(),
                PatientDetails = patientDetails,
                Workflows = workflowRequest.Workflows
            };


            _payloadRepository.Setup(w => w.CreateAsync(It.IsAny<Payload>())).ReturnsAsync(true);

            _dicomService.Setup(d => d.GetPayloadPatientDetailsAsync(workflowRequest.PayloadId.ToString(), workflowRequest.Bucket)).ReturnsAsync(patientDetails);

            var result = await PayloadService.CreateAsync(workflowRequest);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateAsync_ValidWorkflowPayloadExists_ReturnsExisting()
        {
            var workflowRequest = new WorkflowRequestEvent
            {
                Timestamp = DateTime.UtcNow,
                Bucket = "bucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                CorrelationId = Guid.NewGuid().ToString(),
                PayloadId = Guid.NewGuid(),
                Workflows = new List<string> { Guid.NewGuid().ToString() },
                FileCount = 0
            };

            var patientDetails = new PatientDetails
            {
                PatientDob = new DateTime(1996, 02, 05),
                PatientId = Guid.NewGuid().ToString(),
                PatientName = "Steve",
                PatientSex = "male"
            };

            var expected = new Payload
            {
                Timestamp = workflowRequest.Timestamp,
                Bucket = workflowRequest.Bucket,
                FileCount = workflowRequest.FileCount,
                DataTrigger = workflowRequest.DataTrigger,
                CorrelationId = workflowRequest.CorrelationId,
                PayloadId = workflowRequest.PayloadId.ToString(),
                PatientDetails = patientDetails,
                Workflows = workflowRequest.Workflows
            };


            _payloadRepository.Setup(p => p.GetByIdAsync(expected.PayloadId)).ReturnsAsync(expected);

            var result = await PayloadService.CreateAsync(workflowRequest);

            Assert.NotNull(result);
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        [Fact]
        public async Task CreateAsync_NullPayload_ReturnsThrowsException() => await Assert.ThrowsAsync<ArgumentNullException>(async () => await PayloadService.CreateAsync(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsPayload()
        {
            var patientDetails = new PatientDetails
            {
                PatientDob = new DateTime(1996, 02, 05),
                PatientId = Guid.NewGuid().ToString(),
                PatientName = "Steve",
                PatientSex = "male"
            };

            var payload = new Payload
            {
                Timestamp = DateTime.UtcNow,
                PatientDetails = patientDetails,
                Bucket = "bucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                CorrelationId = Guid.NewGuid().ToString(),
                PayloadId = Guid.NewGuid().ToString(),
                Workflows = new List<string> { Guid.NewGuid().ToString() },
            };

            _payloadRepository.Setup(p => p.GetByIdAsync(payload.PayloadId)).ReturnsAsync(payload);
            var result = await PayloadService.GetByIdAsync(payload.PayloadId);

            result.Should().BeEquivalentTo(payload);
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        [Fact]
        public async Task GetByIdAsync_NullId_ReturnsThrowsException() => await Assert.ThrowsAsync<ArgumentNullException>(async () => await PayloadService.GetByIdAsync(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        [Fact]
        public async Task GetAll_ReturnsCompletedPayloads()
        {
            var patientDetails = new PatientDetails
            {
                PatientDob = new DateTime(1996, 02, 05),
                PatientId = Guid.NewGuid().ToString(),
                PatientName = "Steve",
                PatientSex = "male"
            };

            var input = new List<Payload>
            {
                new Payload
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    PatientDetails = patientDetails,
                    Bucket = "bucket",
                    DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Workflows = new List<string> { Guid.NewGuid().ToString() }
                }
            };

            var expected = input.Select(payload => new PayloadDto(payload)).ToList();
            expected.First().PayloadStatus = PayloadStatus.Complete;

            _payloadRepository.Setup(p =>
                p.GetAllAsync(
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())
                ).ReturnsAsync(() => input);
            var param = new List<string>() { input.First().Id };
            _workflowInstanceRepository.Setup(r =>
                r.GetByPayloadIdsAsync(param)
                ).ReturnsAsync(() => new List<WorkflowInstance>());

            var result = await PayloadService.GetAllAsync(null, null);
            result.First().PayloadStatus.Should().Be(PayloadStatus.Complete);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetAll_ReturnsPayloads()
        {
            var patientDetails = new PatientDetails
            {
                PatientDob = new DateTime(1996, 02, 05),
                PatientId = Guid.NewGuid().ToString(),
                PatientName = "Steve",
                PatientSex = "male"
            };

            var input = new List<Payload>
            {
                new Payload
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    PatientDetails = patientDetails,
                    Bucket = "bucket",
                    DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Workflows = new List<string> { Guid.NewGuid().ToString() }
                },
                new Payload
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    PatientDetails = patientDetails,
                    Bucket = "bucket",
                    DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Workflows = new List<string> { Guid.NewGuid().ToString() }
                },
                new Payload
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    PatientDetails = patientDetails,
                    Bucket = "bucket",
                    DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Workflows = new List<string> { Guid.NewGuid().ToString() }
                }
            };

            var expected = input.Select(payload => new PayloadDto(payload)).ToList();
            expected[0].PayloadStatus = PayloadStatus.InProgress;
            expected[1].PayloadStatus = PayloadStatus.Complete;
            expected[2].PayloadStatus = PayloadStatus.Complete;

            _payloadRepository.Setup(p =>
                p.GetAllAsync(
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())
                ).ReturnsAsync(() => input);
            var param = input.Select(i => i.PayloadId).ToList();
            _workflowInstanceRepository.Setup(r =>
                r.GetByPayloadIdsAsync(param)
                ).ReturnsAsync(() =>
                {
                    return new List<WorkflowInstance>()
                    {
                        new WorkflowInstance()
                        {
                            PayloadId = input.First().PayloadId,
                            Status = Status.Created
                        },
                        new WorkflowInstance()
                        {
                            PayloadId = input.Skip(1).First().PayloadId,
                            Status = Status.Succeeded,
                        }
                    };
                });

            var result = await PayloadService.GetAllAsync(null, null);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DeletePayloadFromStorageAsync_ReturnsTrue()
        {
            var payloadId = Guid.NewGuid().ToString();

            _payloadRepository.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(() => new Payload());
            _payloadRepository.Setup(p => p.UpdateAsyncWorkflowIds(It.IsAny<Payload>())).ReturnsAsync(() => true);
            _workflowInstanceRepository.Setup(r => r.GetByPayloadIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(() => new List<WorkflowInstance>());

            _storageService.Setup(s => s.RemoveObjectsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), default));

            var result = await PayloadService.DeletePayloadFromStorageAsync(payloadId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeletePayloadFromStorageAsync_ThrowsMonaiNotFoundException()
        {
            var payloadId = Guid.NewGuid().ToString();

#pragma warning disable CS8603 // Possible null reference return.
            _payloadRepository.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(() => null);
#pragma warning restore CS8603 // Possible null reference return.

            await Assert.ThrowsAsync<MonaiNotFoundException>(async () => await PayloadService.DeletePayloadFromStorageAsync(payloadId));
        }

        [Fact]
        public async Task DeletePayloadFromStorageAsync_ThrowsMonaiBadRequestExceptionWhenDeletionAlreadyInProgress()
        {
            var payloadId = Guid.NewGuid().ToString();

            _payloadRepository.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(() => new Payload
            {
                PayloadDeleted = PayloadDeleted.InProgress
            });

            await Assert.ThrowsAsync<MonaiBadRequestException>(async () => await PayloadService.DeletePayloadFromStorageAsync(payloadId));
        }

        [Fact]
        public async Task DeletePayloadFromStorageAsync_ThrowsMonaiBadRequestExceptionWhenWorkflowInstancesInProgress()
        {
            var payloadId = Guid.NewGuid().ToString();

            _payloadRepository.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(() => new Payload());
            _payloadRepository.Setup(p => p.UpdateAsyncWorkflowIds(It.IsAny<Payload>())).ReturnsAsync(() => true);
            _workflowInstanceRepository.Setup(r => r.GetByPayloadIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(() => new List<WorkflowInstance>
            {
                new WorkflowInstance
                {
                    Status = Status.Created,
                }
            });

            await Assert.ThrowsAsync<MonaiBadRequestException>(async () => await PayloadService.DeletePayloadFromStorageAsync(payloadId));
        }


        [Fact]
        public async Task GetExpiry_Should_use_Config_if_not_set()
        {
            _workflowInstanceRepository.Setup(r =>
                r.GetByPayloadIdsAsync(It.IsAny<List<string>>())
                ).ReturnsAsync(() => new List<WorkflowInstance>());
            var workflow = new WorkflowRevision { Workflow = new Workflow { DataRetentionDays = null } };


            _workflowRepository.Setup(r =>
                    r.GetByWorkflowIdAsync(It.IsAny<string>())
                    ).ReturnsAsync(workflow);

            var now = new DateTime(2021, 1, 1);
            var expires = await PayloadService.GetExpiry(now, "workflowInstanceId");
            Assert.Equal(now.AddDays(99), expires);
        }

        [Fact]
        public async Task GetExpiry_Should_return_null_if_minusOne()
        {
            _workflowInstanceRepository.Setup(r =>
                r.GetByWorkflowInstanceIdAsync(It.IsAny<string>())
                ).ReturnsAsync(() => new WorkflowInstance());
            var workflow = new WorkflowRevision { Workflow = new Workflow { DataRetentionDays = -1 } };


            _workflowRepository.Setup(r =>
                    r.GetByWorkflowIdAsync(It.IsAny<string>())
                    ).ReturnsAsync(workflow);

            var now = new DateTime(2021, 1, 1);
            var expires = await PayloadService.GetExpiry(now, "workflowInstanceId");
            Assert.Null(expires);
        }

        [Fact]
        public async Task GetExpiry_Should_use_Workflow_Value_if_set()
        {
            _workflowInstanceRepository.Setup(r =>
                    r.GetByWorkflowInstanceIdAsync(It.IsAny<string>())
                    ).ReturnsAsync(() => new WorkflowInstance());
            var workflow = new WorkflowRevision { Workflow = new Workflow { DataRetentionDays = 4 } };

            _workflowRepository.Setup(r =>
                    r.GetByWorkflowIdAsync(It.IsAny<string>())
                    ).ReturnsAsync(workflow);
            var now = new DateTime(2021, 1, 1);
            var expires = await PayloadService.GetExpiry(now, "workflowInstanceId");
            Assert.Equal(now.AddDays(4), expires);
        }

        [Fact]
        public void PayloadServiceCreate_Should_Throw_If_No_Options_Passed()
        {
            Assert.Throws<ArgumentNullException>(() => new PayloadService(
                               _payloadRepository.Object,
                               _dicomService.Object,
                               _workflowInstanceRepository.Object,
                               _workflowRepository.Object,
                               _serviceScopeFactory.Object,
                               null!,
                               _logger.Object));
        }

        [Fact]
        public void PayloadServiceCreate_Should_Throw_If_No_workflowRepository_Passed()
        {
            var opts = Options.Create(new WorkflowManagerOptions { DataRetentionDays = 99 });

            Assert.Throws<ArgumentNullException>(() => new PayloadService(
                   _payloadRepository.Object,
                   _dicomService.Object,
                   _workflowInstanceRepository.Object,
                   null!,
                   _serviceScopeFactory.Object,
                   opts,
                   _logger.Object));
        }

        [Fact]
        public async Task PayloadServiceCreate_Should_Call_GetExpiry()
        {
            _payloadRepository.Setup(p => p.CreateAsync(It.IsAny<Payload>())).ReturnsAsync(true);

            var payload = await PayloadService.CreateAsync(new WorkflowRequestEvent
            {
                Timestamp = DateTime.UtcNow,
                Bucket = "bucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                CorrelationId = Guid.NewGuid().ToString(),
                PayloadId = Guid.NewGuid(),
                Workflows = new List<string> { Guid.NewGuid().ToString() },
                FileCount = 0
            });

            var daysdiff = (payload!.Expires! - DateTime.UtcNow).Value.TotalDays + 0.5;

            Assert.Equal(99, (int)daysdiff);
        }


        [Fact]
        public async Task UpdateAsync_NullPayload_ThrowsArgumentNullException()
        {

#pragma warning disable CS8604 // Possible null reference argument.
            // Arrange
            Payload payload = null;

            // Act & Assert

            await Assert.ThrowsAsync<ArgumentNullException>(() => PayloadService.UpdateAsyncWorkflowIds(payload));
#pragma warning restore CS8604 // Possible null reference argument.
        }

    }
}
