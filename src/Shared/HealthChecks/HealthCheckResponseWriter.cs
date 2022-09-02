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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.HealthChecks
{
    /// <summary>
    /// Response writer method for health check endpoints.
    /// </summary>
    public static class HealthCheckResponseWriter
    {
        /// <summary>
        /// Writes the response for health check endpoints.
        /// </summary>
        /// <param name="context">HttpContext.</param>
        /// <param name="report">HealthReport from the health checks in services config.</param>
        /// <returns>Task.</returns>
        public static async Task WriteResponse(HttpContext context, HealthReport report)
        {
            var path = context.Request.Path.Value;
            var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
            context.Response.ContentType = "application/json";

            await ResponseWriteAsync(report, path, isAuthenticated, context.Response.WriteAsync, context.RequestAborted);
        }

        /// <summary>
        /// Writes the response for health check endpoints.
        /// </summary>
        /// <param name="report">HealthReport from the health checks in services config.</param>
        /// <param name="path">HttpContext Path.</param>
        /// <param name="isAuthenticated">Context user is authenticated</param>
        /// <param name="funcResponseWriteAsync">Writes the given text response.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public static async Task ResponseWriteAsync(HealthReport report, string path, bool isAuthenticated, Func<string, CancellationToken, Task> funcResponseWriteAsync, CancellationToken token)
        {
            var service = GetServiceHealthCheckRequest(path);
            var response = new HealthCheckResponse
            {
                Status = report.Status.ToString(),
                Checks = report.Entries.Select(entity =>
                {
                    return ServicesHealthCheckReport(entity, isAuthenticated);
                }),
                Duration = report.TotalDuration,
            };

            if (string.IsNullOrWhiteSpace(service))
            {
                await funcResponseWriteAsync(JsonConvert.SerializeObject(response), token);
                return;
            }

            if (service == "live")
            {
                await funcResponseWriteAsync(response.Status, token);
                return;
            }

            var serviceCheck = response.Checks.FirstOrDefault(c =>
                string.Equals(c.Component, service, StringComparison.InvariantCultureIgnoreCase));

            if (serviceCheck == null)
            {
                await funcResponseWriteAsync("Unknown Service", token);
                return;
            }

            await funcResponseWriteAsync(serviceCheck.Status, token);
        }

        /// <summary>
        /// Trims path to figure out which health check service is being requested.
        /// </summary>
        /// <param name="path">Request path.</param>
        /// <returns>Requested service.</returns>
        private static string GetServiceHealthCheckRequest(string path)
        {
            return path.Replace("/taskmanager", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                .Replace("/health", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                .TrimStart('/');
        }

        /// <summary>
        /// Gets health check report for given entity.
        /// </summary>
        /// <param name="entity">Key Value Pair for health report entity.</param>
        /// <param name="isAuthenticated">User context is authenticated.</param>
        /// <returns>Health Check.</returns>
        public static HealthCheck ServicesHealthCheckReport(KeyValuePair<string, HealthReportEntry> entity, bool isAuthenticated)
        {
            var description = entity.Value.Description;

            if (isAuthenticated)
            {
                if (entity.Value.Exception is not null)
                {
                    description = $"{description}, Exception: {entity.Value.Exception.Message}";
                }
                if (entity.Value.Data is not null)
                {
                    description = $"{description}, Data:";
                    foreach (var item in entity.Value.Data.Values.Where(v => v is string vs && !string.IsNullOrEmpty(vs)))
                    {
                        description = $"{description}, {item}";
                    }
                }
            }

            return new HealthCheck
            {
                Component = entity.Key,
                Status = entity.Value.Status.ToString(),
                Description = description ?? string.Empty,
                Tags = entity.Value.Tags,
            };
        }
    }
}
