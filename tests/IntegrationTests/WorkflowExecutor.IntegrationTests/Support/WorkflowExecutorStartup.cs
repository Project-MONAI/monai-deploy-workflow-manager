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

using System.IO.Abstractions;
using System.Net.Http.Headers;
using System.Text;
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
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkflowManager.Database.Repositories;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.Services;
using Monai.Deploy.WorkflowManager.Services.DataRetentionService;
using Monai.Deploy.WorkflowManager.Services.Http;
using Monai.Deploy.WorkflowManager.Validators;
using Mongo.Migration.Startup.DotNetCore;
using Mongo.Migration.Startup;
using MongoDB.Driver;
using NLog.Web;
using Monai.Deploy.WorkflowManager.Shared.Services;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public static class WorkflowExecutorStartup
    {
        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                var env = builderContext.HostingEnvironment;
                config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: false);
            })
            .ConfigureLogging((builderContext, configureLogging) =>
            {
                configureLogging.ClearProviders();
                configureLogging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddOptions<WorkflowManagerOptions>()
                    .Bind(hostContext.Configuration.GetSection("WorkflowManager"))
                    .PostConfigure(options =>
                    {
                    });
                services.AddOptions<InformaticsGatewayConfiguration>()
                    .Bind(hostContext.Configuration.GetSection("InformaticsGateway"))
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
                services.AddOptions<EndpointSettings>()
                    .Bind(hostContext.Configuration.GetSection("WorkflowManager:endpointSettings"))
                    .PostConfigure(options =>
                    {
                    });
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<WorkflowManagerOptions>, ConfigurationValidator>());

                services.AddSingleton<ConfigurationValidator>();
                services.AddSingleton<WorkflowValidator>();

                services.AddSingleton<DataRetentionService>();

#pragma warning disable CS8603 // Possible null reference return.
                services.AddHostedService<DataRetentionService>(p => p.GetService<DataRetentionService>());
#pragma warning restore CS8603 // Possible null reference return.

                // Services
                services.AddTransient<IFileSystem, FileSystem>();
                services.AddHttpClient();

                // Mongo DB
                services.Configure<WorkloadManagerDatabaseSettings>(hostContext.Configuration.GetSection("WorkloadManagerDatabase"));
                services.AddSingleton<IMongoClient, MongoClient>(s => new MongoClient(hostContext.Configuration["WorkloadManagerDatabase:ConnectionString"]));
                services.AddMigration(new MongoMigrationSettings
                {
                    ConnectionString = hostContext.Configuration.GetSection("WorkloadManagerDatabase:ConnectionString").Value,
                    Database = hostContext.Configuration.GetSection("WorkloadManagerDatabase:DatabaseName").Value,
                });
                services.AddTransient<IWorkflowRepository, WorkflowRepository>();
                services.AddTransient<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
                services.AddTransient<IPayloadRepsitory, PayloadRepository>();
                services.AddTransient<ITasksRepository, TasksRepository>();

                // StorageService - Since mc.exe is unavailable during e2e, skip admin check
                services.AddMonaiDeployStorageService(hostContext.Configuration.GetSection("WorkflowManager:storage:serviceAssemblyName").Value, HealthCheckOptions.ServiceHealthCheck);

                // MessageBroker
                services.AddMonaiDeployMessageBrokerPublisherService(hostContext.Configuration.GetSection("WorkflowManager:messaging:publisherServiceAssemblyName").Value);
                services.AddMonaiDeployMessageBrokerSubscriberService(hostContext.Configuration.GetSection("WorkflowManager:messaging:subscriberServiceAssemblyName").Value);

                services.AddHostedService(p => p.GetService<DataRetentionService>());

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
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.CaptureStartupErrors(true);
                webBuilder.UseStartup<Startup>();
            })
            .UseNLog();

        public static IHost StartWorkflowExecutor()
        {
            var host = CreateHostBuilder().Build();
            host.RunAsync();
            return host;
        }

        public static async Task<HttpResponseMessage> GetQueueStatus(HttpClient httpClient, string vhost, string queue)
        {
            var svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(TestExecutionConfig.RabbitConfig.User + ":" + TestExecutionConfig.RabbitConfig.Password));

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", svcCredentials);

            return await httpClient.GetAsync($"http://{TestExecutionConfig.RabbitConfig.Host}:{TestExecutionConfig.RabbitConfig.WebPort}/api/queues/{vhost}/{queue}");
        }
    }
}
