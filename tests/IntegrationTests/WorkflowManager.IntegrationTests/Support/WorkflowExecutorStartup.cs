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
using Monai.Deploy.Messaging.RabbitMq;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.Storage.MinIo;
using Monai.Deploy.WorkflowManager.Common;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Database;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.PayloadListener.Services;
using Monai.Deploy.WorkflowManager.PayloadListener.Validators;
using Monai.Deploy.WorkflowManager.Services.DataRetentionService;
using Monai.Deploy.WorkflowManager.Services.Http;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Common;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Services;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public static class WorkflowExecutorStartup
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
                    .Bind(hostContext.Configuration.GetSection("messageConnection"))
                    .PostConfigure(options =>
                    {
                    });
                services.AddOptions<StorageServiceConfiguration>()
                    .Bind(hostContext.Configuration.GetSection("storage"))
                    .PostConfigure(options =>
                    {
                    });
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<WorkflowManagerOptions>, ConfigurationValidator>());

                services.AddSingleton<ConfigurationValidator>();

                services.AddSingleton<DataRetentionService>();

                services.AddHostedService<DataRetentionService>(p => p.GetService<DataRetentionService>());

                // Services
                services.AddTransient<IWorkflowService, WorkflowService>();
                services.AddTransient<IWorkflowInstanceService, WorkflowInstanceService>();

                // Mongo DB
                services.Configure<WorkloadManagerDatabaseSettings>(hostContext.Configuration.GetSection("WorkloadManagerDatabase"));
                services.AddSingleton<IMongoClient, MongoClient>(s => new MongoClient(hostContext.Configuration["WorkloadManagerDatabase:ConnectionString"]));
                services.AddTransient<IWorkflowRepository, WorkflowRepository>();
                services.AddTransient<IWorkflowInstanceRepository, WorkflowInstanceRepository>();

                // StorageService
                services.AddSingleton<MinIoStorageService>();
                services.AddSingleton<IStorageService>(implementationFactory =>
                {
                    var options = implementationFactory.GetService<IOptions<StorageServiceConfiguration>>();
                    var serviceProvider = implementationFactory.GetService<IServiceProvider>();
                    var logger = implementationFactory.GetService<ILogger<Program>>();
                    return serviceProvider.LocateService<IStorageService>(logger, options.Value.ServiceAssemblyName);
                });

                // MessageBroker
                services.AddSingleton<RabbitMqMessagePublisherService>();
                services.AddSingleton<IMessageBrokerPublisherService>(implementationFactory =>
                {
                    var options = implementationFactory.GetService<IOptions<WorkflowManagerOptions>>();
                    var serviceProvider = implementationFactory.GetService<IServiceProvider>();
                    var logger = implementationFactory.GetService<ILogger<Program>>();
                    return serviceProvider.LocateService<IMessageBrokerPublisherService>(logger, options.Value.Messaging.PublisherServiceAssemblyName);
                });

                services.AddSingleton<RabbitMqMessageSubscriberService>();
                services.AddSingleton<IMessageBrokerSubscriberService>(implementationFactory =>
                {
                    var options = implementationFactory.GetService<IOptions<WorkflowManagerOptions>>();
                    var serviceProvider = implementationFactory.GetService<IServiceProvider>();
                    var logger = implementationFactory.GetService<ILogger<Program>>();
                    return serviceProvider.LocateService<IMessageBrokerSubscriberService>(logger, options.Value.Messaging.SubscriberServiceAssemblyName);
                });

                services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();

                services.AddSingleton<IEventPayloadReceiverService, EventPayloadReceiverService>();
                services.AddTransient<IEventPayloadValidator, EventPayloadValidator>();
                services.AddSingleton<IWorkflowExecuterService, WorkflowExecuterService>();
                services.AddSingleton<IArtifactMapper, ArtifactMapper>();

                services.AddSingleton<PayloadListenerService>();

                services.AddHostedService<PayloadListenerService>(p => p.GetService<PayloadListenerService>());
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.CaptureStartupErrors(true);
                webBuilder.UseStartup<Startup>();
            });

        public static IHost StartWorkflowExecutor()
        {
            var host = CreateHostBuilder().Build();
            host.RunAsync();
            return host;
        }

        public static async Task<HttpResponseMessage> GetConsumers()
        {
            var httpClient = new HttpClient();

            var svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(TestExecutionConfig.RabbitConfig.User + ":" + TestExecutionConfig.RabbitConfig.Password));

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", svcCredentials);

            return await httpClient.GetAsync($"http://{TestExecutionConfig.RabbitConfig.Host}:{TestExecutionConfig.RabbitConfig.Port}/api/consumers");
        }
    }
}
