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

using System.IO.Abstractions;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.TaskManager.Database;
using Monai.Deploy.WorkflowManager.TaskManager.Database.Options;
using Monai.Deploy.WorkflowManager.TaskManager.Extensions;
using Monai.Deploy.WorkflowManager.TaskManager.Services.Http;
using Mongo.Migration.Startup;
using Mongo.Migration.Startup.DotNetCore;
using MongoDB.Driver;
using NLog;
using NLog.Web;

namespace Monai.Deploy.WorkflowManager.TaskManager
{
    /// <summary>
    /// Main entry point for TaskManager.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        protected Program()
        {
        }

        /// <summary>
        /// standard host builder construction.
        /// </summary>
        /// <param name="args">args passed in to the runtime.</param>
        /// <returns>host builder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.CaptureStartupErrors(true);
                    webBuilder.UseStartup<Startup>();
                })
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

                .UseNLog();

        private static void Main(string[] args)
        {
            var version = typeof(Program).Assembly;
            var assemblyVersionNumber = version.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.1";

            var logger = ConfigureNLog(assemblyVersionNumber);
            logger.Info($"Initializing MONAI Deploy Task Manager v{assemblyVersionNumber}");

            var host = CreateHostBuilder(args).Build();
            host.Run();
            logger.Info("MONAI Deploy Deploy Task Manager shutting down.");

            NLog.LogManager.Shutdown();
        }

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddOptions<WorkflowManagerOptions>().Bind(hostContext.Configuration.GetSection("WorkflowManager"));
            services.AddOptions<StorageServiceConfiguration>().Bind(hostContext.Configuration.GetSection("WorkflowManager:storage"));
            services.AddOptions<MessageBrokerServiceConfiguration>().Bind(hostContext.Configuration.GetSection("WorkflowManager:messaging"));
            services.AddHttpClient();

            // StorageService
            services.AddMonaiDeployStorageService(hostContext.Configuration.GetSection("WorkflowManager:storage:serviceAssemblyName").Value!, HealthCheckOptions.ServiceHealthCheck | HealthCheckOptions.AdminServiceHealthCheck);

            // MessageBroker
            services.AddMonaiDeployMessageBrokerPublisherService(hostContext.Configuration.GetSection("WorkflowManager:messaging:publisherServiceAssemblyName").Value!, true);
            services.AddMonaiDeployMessageBrokerSubscriberService(hostContext.Configuration.GetSection("WorkflowManager:messaging:subscriberServiceAssemblyName").Value!, true);

            // Mongo DB (Workflow Manager)
            services.Configure<TaskManagerDatabaseSettings>(hostContext.Configuration.GetSection("WorkloadManagerDatabase"));
            services.AddSingleton<IMongoClient, MongoClient>(s => new MongoClient(hostContext.Configuration["WorkloadManagerDatabase:ConnectionString"]));
            services.AddTransient<ITaskDispatchEventRepository, TaskDispatchEventRepository>();
            services.AddTransient<IFileSystem, FileSystem>();
            services.AddMigration(new MongoMigrationSettings
            {
                ConnectionString = hostContext.Configuration.GetSection("WorkloadManagerDatabase:ConnectionString").Value,
                Database = hostContext.Configuration.GetSection("WorkloadManagerDatabase:DatabaseName").Value,
            });

            services.AddTransient<IContentTypeProvider, FileExtensionContentTypeProvider>();

            services.AddTaskManager(hostContext);
            services.AddHostedService<ApplicationPartsLogger>();

            services.AddHttpContextAccessor();
            services.AddSingleton<IUriService>(p =>
            {
                var accessor = p.GetRequiredService<IHttpContextAccessor>();
                var request = accessor?.HttpContext?.Request;
                var uri = string.Concat(request?.Scheme, "://", request?.Host.ToUriComponent());
                var newUri = new Uri(uri);
                return new UriService(newUri);
            });
        }

        private static Logger ConfigureNLog(string assemblyVersionNumber)
        {
            return LogManager.Setup().SetupExtensions(ext =>
            {
                ext.RegisterLayoutRenderer("servicename", logEvent => typeof(Program).Namespace);
                ext.RegisterLayoutRenderer("serviceversion", logEvent => assemblyVersionNumber);
                ext.RegisterLayoutRenderer("machinename", logEvent => Environment.MachineName);
                ext.RegisterLayoutRenderer("appname", logEvent => "TaskManager");
            })
            .LoadConfigurationFromAppSettings()
            .GetCurrentClassLogger();
        }
    }
}
