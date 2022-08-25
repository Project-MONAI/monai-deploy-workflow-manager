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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkflowManager.HealthChecks;
using Monai.Deploy.WorkflowManager.Logging.Attributes;
using Newtonsoft.Json.Converters;
using RabbitMQ.Client;

#pragma warning disable CA1822 // Mark members as static
namespace Monai.Deploy.WorkflowManager.Services.Http
{
    /// <summary>
    /// Startup Class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/>.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets configuration settings.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configure Services.
        /// </summary>
        /// <param name="services">Services.</param>
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
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "aspnet6 v1"));
            }

            var options = new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.WriteResponse,
            };

            app.UseHealthChecks("/health", options)
                .UseHealthChecks("/health/live", options)
                .UseHealthChecks($"/health/{HealthCheckSettings.DatabaseHealthCheckName}", options)
                .UseHealthChecks($"/health/{HealthCheckSettings.SubscriberQueueHealthCheckName}", options)
                .UseHealthChecks($"/health/{HealthCheckSettings.PublisherQueueHealthCheckName}", options);

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
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
