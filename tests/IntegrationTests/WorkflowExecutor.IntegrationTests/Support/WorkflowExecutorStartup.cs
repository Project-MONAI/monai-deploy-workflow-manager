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
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Database.Options;
using Monai.Deploy.WorkflowManager.Common.Database.Repositories;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.Common.Services.DataRetentionService;
using Monai.Deploy.WorkflowManager.Common.Services.Http;
using Monai.Deploy.WorkflowManager.Common.Validators;
using Mongo.Migration.Startup.DotNetCore;
using Mongo.Migration.Startup;
using MongoDB.Driver;
using NLog.Web;
using Monai.Deploy.WorkflowManager.Common.Database;
using Monai.Deploy.WorkflowManager.Common.Extensions;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support
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

#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
#pragma warning disable CS8621 // Nullability of reference types in return type doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
                services.AddHostedService(p => p.GetService<DataRetentionService>());
#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
#pragma warning restore CS8621 // Nullability of reference types in return type doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.

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

                // StorageService - Since mc.exe is unavailable during e2e, skip admin check
#pragma warning disable CS8604 // Possible null reference argument.
                services.AddMonaiDeployStorageService(hostContext.Configuration.GetSection("WorkflowManager:storage:serviceAssemblyName").Value, HealthCheckOptions.ServiceHealthCheck);
#pragma warning restore CS8604 // Possible null reference argument.

                // MessageBroker
#pragma warning disable CS8604 // Possible null reference argument.
                services.AddMonaiDeployMessageBrokerPublisherService(hostContext.Configuration.GetSection("WorkflowManager:messaging:publisherServiceAssemblyName").Value);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning disable CS8604 // Possible null reference argument.
                services.AddMonaiDeployMessageBrokerSubscriberService(hostContext.Configuration.GetSection("WorkflowManager:messaging:subscriberServiceAssemblyName").Value);
#pragma warning restore CS8604 // Possible null reference argument.

#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
#pragma warning disable CS8621 // Nullability of reference types in return type doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
                services.AddHostedService(p => p.GetService<DataRetentionService>());
#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
#pragma warning restore CS8621 // Nullability of reference types in return type doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.

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
