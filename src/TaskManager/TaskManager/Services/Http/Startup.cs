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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.HealthChecks;
using RabbitMQ.Client;

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

            GetRequiredServicesForHealthChecks(services, out var dbSettings, out var subscriberQueueFactory, out var publisherQueueFactory);
            const HealthStatus failiureStatus = HealthStatus.Unhealthy;

            services.AddHealthChecks()
                .AddMongoDb(
                    dbSettings.Value.ConnectionString,
                    HealthCheckSettings.DatabaseHealthCheckName,
                    failiureStatus,
                    HealthCheckSettings.DatabaseTags,
                    HealthCheckSettings.DatabaseHealthCheckTimeout)
                .AddRabbitMQ(
                    _ => subscriberQueueFactory,
                    HealthCheckSettings.SubscriberQueueHealthCheckName,
                    failiureStatus,
                    HealthCheckSettings.SubscriberQueueTags,
                    HealthCheckSettings.SubscriberQueueHealthCheckTimeout)
                .AddRabbitMQ(
                    _ => publisherQueueFactory,
                    HealthCheckSettings.PublisherQueueHealthCheckName,
                    failiureStatus,
                    HealthCheckSettings.PublisherQueueTags,
                    HealthCheckSettings.PublisherQueueHealthCheckTimeout);
        }

        /// <summary>
        /// Configure.
        /// </summary>
        /// <param name="app">IApplication Builder.</param>
        /// <param name="env">IWebhostEnviroment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsProduction() is false)
            {
                app.UseDeveloperExceptionPage();
            }

            var options = new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.WriteResponse,
            };

            app.UseHealthChecks("/taskmanager/health", options)
                .UseHealthChecks("/taskmanager/health/live", options);
        }

        private static void GetRequiredServicesForHealthChecks(IServiceCollection services, out IOptions<WorkloadManagerDatabaseSettings> dbSettings, out IConnectionFactory subscriberQueueFactory, out IConnectionFactory publisherQueueFactory)
        {
            var sp = services.BuildServiceProvider();
            dbSettings = sp.GetService<IOptions<WorkloadManagerDatabaseSettings>>();
            var messageBrokerConfig = sp.GetService<IOptions<MessageBrokerServiceConfiguration>>();

            subscriberQueueFactory = RabbitHealthCheckFactory.Create(messageBrokerConfig.Value.SubscriberSettings);
            publisherQueueFactory = RabbitHealthCheckFactory.Create(messageBrokerConfig.Value.PublisherSettings);
        }
    }
}
#pragma warning restore CA1822 // Mark members as static
