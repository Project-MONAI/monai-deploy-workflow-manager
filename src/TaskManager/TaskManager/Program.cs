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
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.Database;
using Monai.Deploy.WorkflowManager.TaskManager.Database.Options;
using Monai.Deploy.WorkflowManager.TaskManager.Extensions;
using MongoDB.Driver;
using NLog;
using NLog.LayoutRenderers;
using NLog.Web;

namespace Monai.Deploy.WorkflowManager.TaskManager
{
    internal class Program
    {
        protected Program()
        { }

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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
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
                    configureLogging.AddConfiguration(builderContext.Configuration.GetSection("Logging"));
                    configureLogging.AddFile(o => o.RootPath = AppContext.BaseDirectory);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    ConfigureServices(hostContext, services);
                })
                .UseNLog();

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddOptions<WorkflowManagerOptions>().Bind(hostContext.Configuration.GetSection("WorkflowManager"));
            services.AddOptions<StorageServiceConfiguration>().Bind(hostContext.Configuration.GetSection("WorkflowManager:storage"));
            services.AddOptions<MessageBrokerServiceConfiguration>().Bind(hostContext.Configuration.GetSection("WorkflowManager:messaging"));
            services.AddHttpClient();

            services.AddMonaiDeployStorageService(hostContext.Configuration.GetSection("WorkflowManager:storage:serviceAssemblyName").Value);
            services.AddMonaiDeployMessageBrokerPublisherService(hostContext.Configuration.GetSection("WorkflowManager:messaging:publisherServiceAssemblyName").Value);
            services.AddMonaiDeployMessageBrokerSubscriberService(hostContext.Configuration.GetSection("WorkflowManager:messaging:subscriberServiceAssemblyName").Value);

            // Mongo DB (Workflow Manager)
            services.Configure<TaskManagerDatabaseSettings>(hostContext.Configuration.GetSection("WorkloadManagerDatabase"));
            services.AddSingleton<IMongoClient, MongoClient>(s => new MongoClient(hostContext.Configuration["WorkloadManagerDatabase:ConnectionString"]));
            services.AddTransient<ITaskDispatchEventRepository, TaskDispatchEventRepository>();
            services.AddTransient<IFileSystem, FileSystem>();

            services.AddTransient<IContentTypeProvider, FileExtensionContentTypeProvider>();

            services.AddTaskManager(hostContext);
        }

        private static Logger ConfigureNLog(string assemblyVersionNumber)
        {
            LayoutRenderer.Register("servicename", logEvent => typeof(Program).Namespace);
            LayoutRenderer.Register("serviceversion", logEvent => assemblyVersionNumber);
            LayoutRenderer.Register("machinename", logEvent => Environment.MachineName);

            return LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        }
    }
}
