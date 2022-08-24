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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Monai.Deploy.WorkflowManager.HealthChecks;
using Monai.Deploy.WorkflowManager.Logging.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Monai.Deploy.WorkflowManager.Services.Http
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

#pragma warning disable CA1822 // Mark members as static
        public void ConfigureServices(IServiceCollection services)
#pragma warning restore CA1822 // Mark members as static
        {
            var monew MongoHealth();
            services.AddHealthChecks().AddAsyncCheck(nameof(MongoHealth), MongoHealth.Check, new[] { "Database" })
                .AddAsyncCheck(nameof(ReddisHealth), ReddisHealth.Check, new[] { "Queue" })
                .AddAsyncCheck(nameof(ArgoHealth), ArgoHealth.Check, new[] { "WorkflowEngine" });
            ;
            services.AddHttpContextAccessor();
            services.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                });

            services.AddVersionedApiExplorer(
                options =>
                {
                    // Add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // NOTE: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // NOTE: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });

            services.AddControllers(options => options.Filters.Add(typeof(LogActionFilterAttribute))).AddNewtonsoftJson(opts => opts.SerializerSettings.Converters.Add(new StringEnumConverter()));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MONAI Workflow Manager", Version = "v1" });
                c.DescribeAllParametersInCamelCase();
            });
        }

#pragma warning disable CA1822 // Mark members as static
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
#pragma warning restore CA1822 // Mark members as static
        {
            if (env.IsProduction() is false)
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "aspnet6 v1"));
            }

            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = WriteHealthCheckResponse,
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var isAuthenticated = context.User.Identity.IsAuthenticated;
            var response = new HealthCheckResponse
            {
                Status = report.Status.ToString(),
                Checks = report.Entries.Select(entity =>
                {
                    return ServicesHealthCheckReport(entity, isAuthenticated);
                }),
                Duration = report.TotalDuration
            };

            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }

        private static HealthCheck ServicesHealthCheckReport(KeyValuePair<string, HealthReportEntry> entity, bool isAuthenticated)
        {
            const string seperator = " ,";

            var description = entity.Value.Description;

            if (isAuthenticated)
            {
                description = string.Concat(entity.Value.Description, seperator,
                                            entity.Value.Exception.Message, seperator,
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

        //private static Task WriteHealthCheckResponse(HttpContext context, HealthReport healthReport)
        //{
        //    context.Response.ContentType = "application/json; charset=utf-8";

        //    var options = new JsonWriterOptions { Indented = true };

        //    using var memoryStream = new MemoryStream();
        //    using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
        //    {
        //        jsonWriter.WriteStartObject();
        //        jsonWriter.WriteString("status", healthReport.Status.ToString());
        //        jsonWriter.WriteStartObject("results");

        //        foreach (var healthReportEntry in healthReport.Entries)
        //        {
        //            jsonWriter.WriteStartObject(healthReportEntry.Key);
        //            jsonWriter.WriteString("status",
        //                healthReportEntry.Value.Status.ToString());
        //            jsonWriter.WriteString("description",
        //                healthReportEntry.Value.Description);
        //            jsonWriter.WriteStartObject("data");

        //            foreach (var item in healthReportEntry.Value.Data)
        //            {
        //                jsonWriter.WritePropertyName(item.Key);

        //                JsonSerializer.Serialize(jsonWriter, item.Value,
        //                    item.Value?.GetType() ?? typeof(object));
        //            }

        //            jsonWriter.WriteEndObject();
        //            jsonWriter.WriteEndObject();
        //        }

        //        jsonWriter.WriteEndObject();
        //        jsonWriter.WriteEndObject();
        //    }

        //    return context.Response.WriteAsync(
        //        Encoding.UTF8.GetString(memoryStream.ToArray()));
        //}
    }
}
