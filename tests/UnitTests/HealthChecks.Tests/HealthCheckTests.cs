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

namespace Monai.Deploy.WorkflowManager.HealthChecks.Tests
{
    public class HealthCheckTests
    {
        [Fact]
        public void HealthCheckResponse_GivenData_ShouldHaveExpectedData()
        {
            var status = "Healthy";
            var component = "component";
            var description = "description";
            var tags = new[] { "tag1", "tag2" };

            var healthCheck = new HealthCheck
            {
                Status = status,
                Component = component,
                Description = description,
                Tags = tags,
            };

            var responseStatus = "Healthy";
            var checks = new[] { healthCheck };
            var time = TimeSpan.FromSeconds(5);

            var response = new HealthCheckResponse()
            {
                Status = responseStatus,
                Checks = checks,
                Duration = time,
            };

            Assert.Equal(responseStatus, response.Status);
            Assert.Equal(time, response.Duration);
            Assert.Contains(healthCheck, response.Checks);
        }

        [Fact]
        public void HealthCheck_GivenData_ShouldHaveExpectedData()
        {
            var status = "Healthy";
            var component = "component";
            var description = "description";
            var tags = new[] { "tag1", "tag2" };

            var healthCheck = new HealthCheck
            {
                Status = status,
                Component = component,
                Description = description,
                Tags = tags,
            };

            Assert.Equal(healthCheck.Status, status);
            Assert.Equal(healthCheck.Component, component);
            Assert.Equal(healthCheck.Description, description);
            Assert.Equal(healthCheck.Tags, tags);
        }

    }
}
