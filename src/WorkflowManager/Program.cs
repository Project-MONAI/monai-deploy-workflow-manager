// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.IO;
using System.IO.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.API;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Database;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkflowManager.Services;
using Monai.Deploy.WorkflowManager.Services.DataRetentionService;
using Monai.Deploy.WorkflowManager.Services.Http;
using Monai.Deploy.WorkflowManager.Storage.Services;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Common;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Services;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Common;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager
{
#pragma warning disable SA1600 // Elements should be documented

    internal class Program
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
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureLogging((builderContext, configureLogging) =>
                {
                    configureLogging.AddConfiguration(builderContext.Configuration.GetSection("Logging"));
                    configureLogging.AddFile(o => o.RootPath = AppContext.BaseDirectory);
                })
                .ConfigureServices((hostContext, services) =>
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
                    services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<WorkflowManagerOptions>, ConfigurationValidator>());

                    services.AddSingleton<ConfigurationValidator>();

                    services.AddSingleton<DataRetentionService>();

                    services.AddHostedService<DataRetentionService>(p => p.GetService<DataRetentionService>());

                    // Services
                    services.AddTransient<IFileSystem, FileSystem>();
                    services.AddHttpClient();

                    // Mongo DB
                    services.Configure<WorkloadManagerDatabaseSettings>(hostContext.Configuration.GetSection("WorkloadManagerDatabase"));
                    services.AddSingleton<IMongoClient, MongoClient>(s => new MongoClient(hostContext.Configuration["WorkloadManagerDatabase:ConnectionString"]));
                    services.AddTransient<IWorkflowRepository, WorkflowRepository>();
                    services.AddTransient<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
                    services.AddTransient<IPayloadRepsitory, PayloadRepository>();

                    // StorageService
                    services.AddMonaiDeployStorageService(hostContext.Configuration.GetSection("WorkflowManager:storage:serviceAssemblyName").Value);

                    // MessageBroker
                    services.AddMonaiDeployMessageBrokerPublisherService(hostContext.Configuration.GetSection("WorkflowManager:messaging:publisherServiceAssemblyName").Value);
                    services.AddMonaiDeployMessageBrokerSubscriberService(hostContext.Configuration.GetSection("WorkflowManager:messaging:subscriberServiceAssemblyName").Value);

                    services.AddHostedService(p => p.GetService<DataRetentionService>());

                    services.AddTaskManager(hostContext);
                    services.AddWorkflowExecutor(hostContext);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.CaptureStartupErrors(true);
                    webBuilder.UseStartup<Startup>();
                });

        private static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }

#pragma warning restore SA1600 // Elements should be documented
    }
}
