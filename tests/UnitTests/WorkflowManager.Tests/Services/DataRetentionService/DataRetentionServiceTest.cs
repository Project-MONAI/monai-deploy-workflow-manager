// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Contracts.Rest;
using Monai.Deploy.WorkflowManager.Services.DataRetentionService;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Test.Services.Http
{
    public class DataRetentionServiceTest
    {
        private readonly Mock<ILogger<DataRetentionService>> _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public DataRetentionServiceTest()
        {
            _logger = new Mock<ILogger<DataRetentionService>>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        [Fact(DisplayName = "DataRetentionService - Constructor Test")]
        public void ConstructorTest()
        {
            Assert.Throws<ArgumentNullException>(() => new DataRetentionService(null));
        }

        [Fact(DisplayName = "DataRetentionService - Can start and stop service")]
        public async Task CanStartStop()
        {
            var service = new DataRetentionService(_logger.Object);
            Assert.Equal(ServiceStatus.Unknown, service.Status);

            await service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            Assert.Equal(ServiceStatus.Running, service.Status);

            await service.StopAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            Assert.Equal(ServiceStatus.Stopped, service.Status);

            service.Dispose();
            Assert.Equal(ServiceStatus.Disposed, service.Status);
        }
    }
}
