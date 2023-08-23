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

using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Moq;

namespace Monai.Deploy.WorkflowManager.Test.Repositories
{
    public class TestServiceA : IMonaiService
    {
        public ServiceStatus Status { get; set; } = ServiceStatus.Running;

        public string ServiceName => nameof(TestServiceA);
    }

    public class TestServiceB : IMonaiService
    {
        public ServiceStatus Status { get; set; } = ServiceStatus.Running;

        public string ServiceName => nameof(TestServiceB);
    }

    public class MonaiServiceLocatorTests
    {
        private readonly Mock<IServiceProvider> _serviceProvider;

        public MonaiServiceLocatorTests()
        {
            _serviceProvider = new Mock<IServiceProvider>();
            _serviceProvider.Setup(p => p.GetService(It.IsAny<Type>())).Returns((Type type) =>
            {
                var mock = new Mock<IMonaiService>();
                mock.SetupGet(p => p.Status).Returns(ServiceStatus.Running);
                mock.SetupGet(p => p.ServiceName).Returns(type.Name);
                return mock.Object;
            });
        }

        [Fact(DisplayName = "GetMonaiServices")]
        public void GetMonaiServices()
        {
            var serviceLocator = new MonaiServiceLocator(_serviceProvider.Object);
            var result = serviceLocator.GetMonaiServices();

            Assert.Collection(result,
                items => items.ServiceName.Equals(nameof(TestServiceA)),
                items => items.ServiceName.Equals(nameof(TestServiceB)));
        }

        [Fact(DisplayName = "GetServiceStatus")]
        public void GetServiceStatus()
        {
            var serviceLocator = new MonaiServiceLocator(_serviceProvider.Object);
            var result = serviceLocator.GetServiceStatus();

            Assert.Equal(2, result.Count);
            foreach (var svc in result.Keys)
            {
                Assert.Equal(ServiceStatus.Running, result[svc]);
            }
        }
    }
}
