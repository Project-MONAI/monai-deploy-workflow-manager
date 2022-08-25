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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.HealthChecks
{
    /// <summary>
    /// Response writer method for health check endpoints.
    /// </summary>
    internal static class HealthCheckResponseWriter
    {
        /// <summary>
        /// Writes the response for health check endpoints.
        /// </summary>
        /// <param name="context">HttpContext.</param>
        /// <param name="report">HealthReport from the health checks in services config.</param>
        /// <returns>Task.</returns>
        public static async Task WriteResponse(HttpContext context, HealthReport report)
        {
            var service = GetServiceHealthCheckRequest(context.Request.Path.Value);

            context.Response.ContentType = "application/json";

            var isAuthenticated = context.User.Identity.IsAuthenticated;
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
                await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                return;
            }

            if (service == "live")
            {
                await context.Response.WriteAsync(response.Status);
                return;
            }

            var serviceCheck = response.Checks.First(c =>
                string.Equals(c.Component, service, StringComparison.InvariantCultureIgnoreCase));

            if (serviceCheck == null)
            {
                await context.Response.WriteAsync("Unknown Service");
                return;
            }

            await context.Response.WriteAsync(serviceCheck.Status);
        }

        /// <summary>
        /// Trims path to figure out which health check service is being requested.
        /// </summary>
        /// <param name="path">Request path.</param>
        /// <returns>Requested service.</returns>
        private static string GetServiceHealthCheckRequest(string path)
        {
            return path.Replace("/health", string.Empty).TrimStart('/');
        }

        private static HealthCheck ServicesHealthCheckReport(KeyValuePair<string, HealthReportEntry> entity, bool isAuthenticated)
        {
            const string seperator = " ,";

            var description = entity.Value.Description;

            if (isAuthenticated)
            {
                description = string.Concat(
                    entity.Value.Description,
                    seperator,
                    entity.Value.Exception.Message,
                    seperator,
                    entity.Value.Exception.StackTrace);
            }

            return new HealthCheck
            {
                Component = entity.Key,
                Status = entity.Value.Status.ToString(),
                Description = description,
                Tags = entity.Value.Tags,
            };
        }
    }
}
