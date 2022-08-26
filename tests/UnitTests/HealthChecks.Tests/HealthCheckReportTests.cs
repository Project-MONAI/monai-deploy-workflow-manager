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

using System.Collections.ObjectModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Monai.Deploy.WorkflowManager.HealthChecks.Tests
{
    public class HealthCheckReportTests
    {
        #region WriteResponse
        [Theory]
        [InlineData("/taskManager/health/live", "componentA", "desc1", new[] { "tag1", "tag2" }, 5, "Healthy")]
        [InlineData("/taskManager/health/donkey", "componentB", "desc2", null, 15, "Unknown Service")]
        [InlineData("/taskManager/health/componentC", "componentC", "desc3", null, 1, "Healthy")]
        public async Task ResponseWriteAsync_GivenData_ShouldWriteExpectedOutputAsync(string path, string component, string description, IEnumerable<string> tags, int seconds, string expectedOutput)
        {
            var kvpList = new[]
            {
                KeyValuePair.Create(component, new HealthReportEntry(HealthStatus.Healthy, description, TimeSpan.FromSeconds(seconds), null, null, tags))
            };

            var dict = new ReadOnlyDictionary<string, HealthReportEntry>(
                    new Dictionary<string, HealthReportEntry>(kvpList));

            var healthReport = new HealthReport(dict, HealthStatus.Healthy, TimeSpan.FromSeconds(5));

            Task FuncResponseWriteAsync(string output, CancellationToken _)
            {
                Assert.Equal(expectedOutput, output);
                return Task.CompletedTask;
            }

            await HealthCheckResponseWriter.ResponseWriteAsync(healthReport, path, true, FuncResponseWriteAsync, new CancellationToken());
        }

        [Fact]
        public async Task ResponseWriteAsync_GivenRootPath_ShouldWriteExpectedOutputAsync()
        {
            var path = "/health";
            var component = "componentA";
            var description = "desc1";
            IEnumerable<string> tags = new[] { "tag1", "tag2" };
            var seconds = 5;

            var expectedOutput = "{\"Status\":\"Healthy\",\"Checks\":[{\"Status\":\"Healthy\",\"Component\":\"componentA\",\"Description\":\"desc1\",\"Tags\":[\"tag1\",\"tag2\"]}],\"Duration\":\"00:00:05\"}";

            var kvpList = new[]
            {
                KeyValuePair.Create(component, new HealthReportEntry(HealthStatus.Healthy, description, TimeSpan.FromSeconds(seconds), null, null, tags))
            };

            var dict = new ReadOnlyDictionary<string, HealthReportEntry>(
                    new Dictionary<string, HealthReportEntry>(kvpList));

            var healthReport = new HealthReport(dict, HealthStatus.Healthy, TimeSpan.FromSeconds(5));

            Task FuncResponseWriteAsync(string output, CancellationToken _)
            {
                Assert.Equal(expectedOutput, output);
                return Task.CompletedTask;
            }

            await HealthCheckResponseWriter.ResponseWriteAsync(healthReport, path, true, FuncResponseWriteAsync, new CancellationToken());
        }
        #endregion

        #region ServicesHealthCheckReport
        [Theory]
        [InlineData("componentA", "desc1", new[] { "tag1", "tag2" }, 5)]
        [InlineData("componentB", "desc2", null, 15)]
        [InlineData("componentC", "desc3", null, 1)]
        public void ServicesHealthCheckReport_GivenEntityDataAndNotAuthenticated_ShouldReturnHealthCheck(string component, string description, IEnumerable<string> tags, int seconds)
        {
            var time = TimeSpan.FromSeconds(seconds);
            Exception? ex = null;

            var entity = new HealthReportEntry(HealthStatus.Healthy, description, time, ex, null, tags);
            var kvp = KeyValuePair.Create(component, entity);

            var report = HealthCheckResponseWriter.ServicesHealthCheckReport(kvp, false);

            var expectedTags = tags;
            if (tags == null)
            {
                expectedTags = Array.Empty<string>();
            }
            Assert.NotNull(report);
            Assert.Equal(component, report.Component);
            Assert.Equal(description, report.Description);
            Assert.Equal(expectedTags, report.Tags);
        }

        [Theory]
        [InlineData("componentA", "desc1", new[] { "tag1", "tag2" }, 5)]
        [InlineData("componentB", "desc2", null, 15)]
        [InlineData("componentC", "desc3", null, 1)]
        public void ServicesHealthCheckReport_GivenEntityDataAndNotAuthenticatedAndException_ShouldReturnHealthCheck(string component, string description, IEnumerable<string> tags, int seconds)
        {
            var time = TimeSpan.FromSeconds(seconds);
            var ex = new Exception("boom");

            var entity = new HealthReportEntry(HealthStatus.Healthy, description, time, ex, null, tags);
            var kvp = KeyValuePair.Create(component, entity);

            var report = HealthCheckResponseWriter.ServicesHealthCheckReport(kvp, false);

            var expectedTags = tags;
            if (tags == null)
            {
                expectedTags = Array.Empty<string>();
            }

            Assert.NotNull(report);
            Assert.Equal(component, report.Component);
            Assert.Equal(description, report.Description);
            Assert.Equal(expectedTags, report.Tags);
        }

        [Theory]
        [InlineData("componentA", "desc1", new[] { "tag1", "tag2" }, 5)]
        [InlineData("componentB", "desc2", null, 15)]
        [InlineData("componentC", "desc3", null, 1)]
        public void ServicesHealthCheckReport_GivenEntityDataAndIsAuthenticatedAndException_ShouldReturnHealthCheck(string component, string description, IEnumerable<string> tags, int seconds)
        {
            var time = TimeSpan.FromSeconds(seconds);
            var ex = new Exception("boom");
            var isAuthenticated = true;

            var entity = new HealthReportEntry(HealthStatus.Healthy, description, time, ex, null, tags);
            var kvp = KeyValuePair.Create(component, entity);

            var report = HealthCheckResponseWriter.ServicesHealthCheckReport(kvp, isAuthenticated);

            var expectedTags = tags;
            if (tags == null)
            {
                expectedTags = Array.Empty<string>();
            }

            var expectedDescription = $"{description}, {ex.Message}";

            Assert.NotNull(report);
            Assert.Equal(component, report.Component);
            Assert.Equal(expectedDescription, report.Description);
            Assert.Equal(expectedTags, report.Tags);
        }

        [Theory]
        [InlineData("componentA", "desc1", new[] { "tag1", "tag2" }, 5)]
        [InlineData("componentB", "desc2", null, 15)]
        [InlineData("componentC", "desc3", null, 1)]
        public void ServicesHealthCheckReport_GivenEntityDataAndIsAuthenticated_ShouldReturnHealthCheck(string component, string description, IEnumerable<string> tags, int seconds)
        {
            var time = TimeSpan.FromSeconds(seconds);
            Exception? ex = null;
            var isAuthenticated = true;

            var entity = new HealthReportEntry(HealthStatus.Healthy, description, time, ex, null, tags);
            var kvp = KeyValuePair.Create(component, entity);

            var report = HealthCheckResponseWriter.ServicesHealthCheckReport(kvp, isAuthenticated);

            var expectedTags = tags;
            if (tags == null)
            {
                expectedTags = Array.Empty<string>();
            }

            Assert.NotNull(report);
            Assert.Equal(component, report.Component);
            Assert.Equal(description, report.Description);
            Assert.Equal(expectedTags, report.Tags);
        }
        #endregion


    }
}
