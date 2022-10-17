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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Services.DataRetentionService;
using Monai.Deploy.WorkflowManager.Shared;
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
