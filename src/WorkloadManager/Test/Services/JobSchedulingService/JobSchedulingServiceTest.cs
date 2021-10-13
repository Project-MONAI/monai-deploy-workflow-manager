// Copyright 2021 MONAI Consortium
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkloadManager.Contracts.Rest;
using Monai.Deploy.WorkloadManager.Services.JobSchedulingService;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Monai.Deploy.WorkloadManager.Test.Services.Http
{
    public class JobSchedulingServiceTest
    {
        private readonly Mock<ILogger<JobSchedulingService>> logger;
        private readonly CancellationTokenSource cancellationTokenSource;

        public JobSchedulingServiceTest()
        {
            this.logger = new Mock<ILogger<JobSchedulingService>>();
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        [Fact(DisplayName = "JobSchedulingService - Constructor Test")]
        public void ConstructorTest()
        {
            Assert.Throws<ArgumentNullException>(() => new JobSchedulingService(null));
        }

        [Fact(DisplayName = "JobSchedulingService - Can start and stop service")]
        public async Task CanStartStop()
        {
            var service = new JobSchedulingService(this.logger.Object);
            Assert.Equal(ServiceStatus.Unknown, service.Status);

            await service.StartAsync(cancellationTokenSource.Token);
            Assert.Equal(ServiceStatus.Running, service.Status);

            await service.StopAsync(this.cancellationTokenSource.Token);
            Assert.Equal(ServiceStatus.Stopped, service.Status);

            service.Dispose();
            Assert.Equal(ServiceStatus.Disposed, service.Status);
        }
    }
}
