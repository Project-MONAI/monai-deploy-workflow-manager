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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.HealthChecks;

#pragma warning disable CA1822 // Mark members as static
namespace Monai.Deploy.WorkflowManager.TaskManager.Services.Http
{
    internal class Startup
    {
        /// <summary>
        /// Gets configuration settings.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configure Services.
        /// </summary>
        /// <param name="services">Services Collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<WorkloadManagerDatabaseSettings>(Configuration.GetSection("WorkloadManagerDatabase"));
            services.AddOptions<MessageBrokerServiceConfiguration>().Bind(Configuration.GetSection("WorkflowManager:messaging"));

            services.GetRequiredServicesForHealthChecks(out var dbSettings, out var subscriberQueueFactory, out var publisherQueueFactory);
            const HealthStatus failiureStatus = HealthStatus.Unhealthy;

            services.AddMonaiHealthChecks(dbSettings, subscriberQueueFactory, publisherQueueFactory, failiureStatus);
        }

        /// <summary>
        /// Configure.
        /// </summary>
        /// <param name="app">IApplication Builder.</param>
        /// <param name="env">IWebhostEnviroment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var options = new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.WriteResponse,
            };

            app.UseHealthChecks("/taskmanager/health", options)
                .UseHealthChecks("/taskmanager/health/live", options);
        }
    }
}
#pragma warning restore CA1822 // Mark members as static
