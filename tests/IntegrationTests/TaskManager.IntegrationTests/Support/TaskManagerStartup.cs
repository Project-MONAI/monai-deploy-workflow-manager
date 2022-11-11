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
using Monai.Deploy.WorkflowManager.TaskManager.Database;
using Monai.Deploy.WorkflowManager.TaskManager.Database.Options;
using Monai.Deploy.WorkflowManager.TaskManager.Extensions;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.TaskManager.Services.Http;
using MongoDB.Driver;
using NLog.Web;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support
{
    public static class TaskManagerStartup
    {
        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                var env = builderContext.HostingEnvironment;
                config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true);
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

                    // Services
                    services.AddTransient<IFileSystem, FileSystem>();
                    services.AddHttpClient();

                    // Mongo DB
                    services.Configure<TaskManagerDatabaseSettings>(hostContext.Configuration.GetSection("WorkloadManagerDatabase"));
                    services.AddSingleton<IMongoClient, MongoClient>(s => new MongoClient(hostContext.Configuration["WorkloadManagerDatabase:ConnectionString"]));
                    services.AddTransient<ITaskDispatchEventRepository, TaskDispatchEventRepository>();

                    // StorageService - Since mc.exe is unavailable during e2e, skip admin check
                    services.AddMonaiDeployStorageService(hostContext.Configuration.GetSection("WorkflowManager:storage:serviceAssemblyName").Value, HealthCheckOptions.ServiceHealthCheck);

                    // MessageBroker
                    services.AddMonaiDeployMessageBrokerPublisherService(hostContext.Configuration.GetSection("WorkflowManager:messaging:publisherServiceAssemblyName").Value);
                    services.AddMonaiDeployMessageBrokerSubscriberService(hostContext.Configuration.GetSection("WorkflowManager:messaging:subscriberServiceAssemblyName").Value);

                    services.AddTaskManager(hostContext);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.CaptureStartupErrors(true);
                    webBuilder.UseStartup<Startup>();
                })
                .UseNLog();

        public static IHost StartTaskManager()
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
