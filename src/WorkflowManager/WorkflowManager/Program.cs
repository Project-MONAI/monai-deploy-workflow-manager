/*
 * Copyright 2023 MONAI Consortium
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
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Database;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Database.Options;
using Monai.Deploy.WorkflowManager.Common.Database.Repositories;
using Monai.Deploy.WorkflowManager.Common.Extensions;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.MonaiBackgroundService;
using Monai.Deploy.WorkflowManager.Common.Services.DataRetentionService;
using Monai.Deploy.WorkflowManager.Common.Services.Http;
using Monai.Deploy.WorkflowManager.Common.Validators;
using Mongo.Migration.Startup;
using Mongo.Migration.Startup.DotNetCore;
using MongoDB.Driver;
using NLog;
using NLog.Web;

namespace Monai.Deploy.WorkflowManager.Common
{
#pragma warning disable SA1600 // Elements should be documented

    public class Program
    {
        protected Program()
        {
        }

        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    var env = builderContext.HostingEnvironment;
                    config
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging((builderContext, configureLogging) =>
                {
                    configureLogging.ClearProviders();
                    configureLogging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    ConfigureServices(hostContext, services);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.CaptureStartupErrors(true);
                    webBuilder.UseStartup<Startup>();
                })
               .UseNLog();

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddOptions<WorkflowManagerOptions>()
                .Bind(hostContext.Configuration.GetSection("WorkflowManager"))
                .PostConfigure(options =>
                {
                });
            services.AddOptions<MessageBrokerServiceConfiguration>()
                .Bind(hostContext.Configuration.GetSection("WorkflowManager:messaging"))
                .PostConfigure(options =>
                {
                });
            services.AddOptions<StorageServiceConfiguration>()
                .Bind(hostContext.Configuration.GetSection("WorkflowManager:storage"))
                .PostConfigure(options =>
                {
                });
            services.AddOptions<InformaticsGatewayConfiguration>()
                .Bind(hostContext.Configuration.GetSection("InformaticsGateway"))
                .PostConfigure(options =>
                {
                });
            services.AddOptions<EndpointSettings>()
                .Bind(hostContext.Configuration.GetSection("WorkflowManager:endpointSettings"))
                .PostConfigure(options =>
                {
                });
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<WorkflowManagerOptions>, ConfigurationValidator>());

            services.AddSingleton<ConfigurationValidator>();
            services.AddTransient<WorkflowValidator>();

            services.AddSingleton<DataRetentionService>();

#pragma warning disable CS8603 // Possible null reference return.
            services.AddHostedService(p => p.GetService<DataRetentionService>());
#pragma warning restore CS8603 // Possible null reference return.

            // Services
            services.AddTransient<IFileSystem, FileSystem>();
            services.AddHttpClient();

            // Mongo DB
            services.Configure<WorkloadManagerDatabaseSettings>(hostContext.Configuration.GetSection("WorkloadManagerDatabase"));
            services.Configure<ExecutionStatsDatabaseSettings>(hostContext.Configuration.GetSection("WorkloadManagerDatabase"));
            services.AddSingleton<IMongoClient, MongoClient>(s => new MongoClient(hostContext.Configuration["WorkloadManagerDatabase:ConnectionString"]));
            services.AddTransient<IWorkflowRepository, WorkflowRepository>();
            services.AddTransient<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
            services.AddTransient<IPayloadRepository, PayloadRepository>();
            services.AddTransient<ITasksRepository, TasksRepository>();
            services.AddTransient<ITaskExecutionStatsRepository, TaskExecutionStatsRepository>();
            services.AddMigration(new MongoMigrationSettings
            {
                ConnectionString = hostContext.Configuration.GetSection("WorkloadManagerDatabase:ConnectionString").Value,
                Database = hostContext.Configuration.GetSection("WorkloadManagerDatabase:DatabaseName").Value,
            });

            // StorageService
            services.AddMonaiDeployStorageService(hostContext.Configuration.GetSection("WorkflowManager:storage:serviceAssemblyName").Value, HealthCheckOptions.ServiceHealthCheck);

            // MessageBroker
            services.AddMonaiDeployMessageBrokerPublisherService(hostContext.Configuration.GetSection("WorkflowManager:messaging:publisherServiceAssemblyName").Value);
            services.AddMonaiDeployMessageBrokerSubscriberService(hostContext.Configuration.GetSection("WorkflowManager:messaging:subscriberServiceAssemblyName").Value);

            services.AddWorkflowExecutor(hostContext);

            services.AddHttpContextAccessor();
            services.AddSingleton<IUriService>(p =>
            {
                var accessor = p.GetRequiredService<IHttpContextAccessor>();
                var request = accessor?.HttpContext?.Request;
                var uri = string.Concat(request?.Scheme, "://", request?.Host.ToUriComponent());
                var newUri = new Uri(uri);
                return new UriService(newUri);
            });

            services.AddHostedService<Worker>();
        }

        private static void Main(string[] args)
        {
            var version = typeof(Program).Assembly;
            var assemblyVersionNumber = version.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.1";

            var logger = ConfigureNLog(assemblyVersionNumber);
            logger.Info($"Initializing MONAI Deploy Workflow Manager v{assemblyVersionNumber}");

            var host = CreateHostBuilder(args).Build();
            host.Run();
            logger.Info("MONAI Deploy Workflow Manager shutting down.");

            NLog.LogManager.Shutdown();
        }

        private static Logger ConfigureNLog(string assemblyVersionNumber)
        {
            return LogManager.Setup().SetupExtensions(ext =>
            {
                ext.RegisterLayoutRenderer("servicename", logEvent => typeof(Program).Namespace);
                ext.RegisterLayoutRenderer("serviceversion", logEvent => assemblyVersionNumber);
                ext.RegisterLayoutRenderer("machinename", logEvent => Environment.MachineName);
                ext.RegisterLayoutRenderer("appname", logEvent => "WorkflowManager");
            })
            .LoadConfigurationFromAppSettings()
            .GetCurrentClassLogger();
        }

#pragma warning restore SA1600 // Elements should be documented
    }
}
