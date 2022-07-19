// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.IO.Abstractions;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Hosting;
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
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Database;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkflowManager.Services;
using Monai.Deploy.WorkflowManager.Services.DataRetentionService;
using Monai.Deploy.WorkflowManager.Services.Http;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public static class TaskManagerStartup
    {
        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
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
                })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.CaptureStartupErrors(true);
                webBuilder.UseStartup<Startup>();
            });

        public static IHost StartTaskManager()
        {
            var host = CreateHostBuilder().Build();
            host.RunAsync();
            return host;
        }

        public static async Task<HttpResponseMessage> GetConsumers(HttpClient httpClient)
        {
            var svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(TestExecutionConfig.RabbitConfig.User + ":" + TestExecutionConfig.RabbitConfig.Password));

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", svcCredentials);

            return await httpClient.GetAsync($"http://{TestExecutionConfig.RabbitConfig.Host}:{TestExecutionConfig.RabbitConfig.Port}/api/consumers");
        }
    }
}
