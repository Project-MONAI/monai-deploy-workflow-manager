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

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Moq;

namespace Monai.Deploy.WorkflowManager.Test.Services.Http
{
    public class MonaiHealthCheckTests
    {
        private readonly Mock<IMonaiServiceLocator> _monaiServiceLocator;

        public MonaiHealthCheckTests()
        {
            _monaiServiceLocator = new Mock<IMonaiServiceLocator>();
        }

        [Fact]
        public async Task GivenAllServicesRunning_WhenCheckHealthAsyncIsCalled_ReturnsHealthy()
        {
            _monaiServiceLocator.Setup(p => p.GetServiceStatus()).Returns(new Dictionary<string, ServiceStatus>()
            {
                { "A", ServiceStatus.Running },
                { "B", ServiceStatus.Running },
                { "C", ServiceStatus.Running },
            });

            var svc = new MonaiHealthCheck(_monaiServiceLocator.Object);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var result = await svc.CheckHealthAsync(null);

            Assert.Equal(HealthStatus.Healthy, result.Status);
        }

        [Fact]
        public async Task GivenSomeServicesNotRunning_WhenCheckHealthAsyncIsCalled_ReturnsDegraded()
        {
            _monaiServiceLocator.Setup(p => p.GetServiceStatus()).Returns(new Dictionary<string, ServiceStatus>()
            {
                { "A", ServiceStatus.Running },
                { "B", ServiceStatus.Cancelled },
                { "C", ServiceStatus.Stopped },
            });

            var svc = new MonaiHealthCheck(_monaiServiceLocator.Object);
            var result = await svc.CheckHealthAsync(null);
            Assert.Equal(HealthStatus.Degraded, result.Status);
            Assert.Equal(ServiceStatus.Cancelled, result.Data["B"]);
            Assert.Equal(ServiceStatus.Stopped, result.Data["C"]);
        }

        [Fact]
        public async Task GivenAllServicesNotRunning_WhenCheckHealthAsyncIsCalled_ReturnsUnhealthy()
        {
            _monaiServiceLocator.Setup(p => p.GetServiceStatus()).Returns(new Dictionary<string, ServiceStatus>()
            {
                { "A", ServiceStatus.Stopped },
                { "B", ServiceStatus.Cancelled },
                { "C", ServiceStatus.Stopped },
            });

            var svc = new MonaiHealthCheck(_monaiServiceLocator.Object);
            var result = await svc.CheckHealthAsync(null);

            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.Equal(ServiceStatus.Stopped, result.Data["A"]);
            Assert.Equal(ServiceStatus.Cancelled, result.Data["B"]);
            Assert.Equal(ServiceStatus.Stopped, result.Data["C"]);
        }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
