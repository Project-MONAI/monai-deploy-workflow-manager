// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Globalization;
using System.IO.Abstractions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Common;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.Argo;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.StaticValues;

namespace Monai.Deploy.WorkflowManager.TaskManager.Runner
{
    internal class Program
    {
        protected Program()
        { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Test application")]
        private static async Task Main(string[] args)
        {

            var exitEvent = new ManualResetEvent(false);
            var host = CreateHostBuilder(args).Build();
            _ = host.StartAsync();

            var messagingKeys = new MessageBrokerConfigurationKeys();
            var taskManager = host.Services.GetRequiredService<TaskManager>();
            Guard.Against.NullService(taskManager, nameof(TaskManager));
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            Guard.Against.NullService(logger, nameof(ILogger<Program>));
            var publisher = host.Services.GetRequiredService<IMessageBrokerPublisherService>();
            Guard.Against.NullService(publisher, nameof(IMessageBrokerPublisherService));
            var subscriber = host.Services.GetRequiredService<IMessageBrokerSubscriberService>();
            Guard.Against.NullService(subscriber, nameof(IMessageBrokerSubscriberService));
            var wmConfig = host.Services.GetRequiredService<IOptions<WorkflowManagerOptions>>();
            Guard.Against.NullService(wmConfig, nameof(IOptions<WorkflowManagerOptions>));

            subscriber.Subscribe(messagingKeys.TaskUpdateRequest, string.Empty, (args) =>
            {
                logger.LogInformation($"{args.Message.MessageDescription} received.");
                var updateMessage = args.Message.ConvertToJsonMessage<TaskUpdateEvent>();

                logger.LogInformation($"Task updated with new status: {updateMessage.Body.Status}");
                subscriber.Acknowledge(args.Message);
            }, 1);

            while (taskManager.Status != Contracts.Rest.ServiceStatus.Running)
            {
                logger.LogInformation($"Waiting for Task Manager to be ready: state={taskManager.Status}...");
                await Task.Delay(100).ConfigureAwait(false);
            }
            Console.CancelKeyPress += (sender, eventArgs) =>
                    {
                        eventArgs.Cancel = true;
                        exitEvent.Set();
                    };

            // await Task.Run(() =>
            // {
            //     var message = GenerateDispatchEvent(argoBaseUri, wmConfig.Value);
            //     logger.LogInformation($"Queuing new job with correlation ID={message.CorrelationId}.");
            //     publisher.Publish(messagingKeys.TaskDispatchRequest, message);
            // }).ConfigureAwait(false);

            exitEvent.WaitOne();
            logger.LogInformation("Stopping Task Manager...");

            await host.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }

        private static Message GenerateDispatchEvent(string argBaseUri, WorkflowManagerOptions wmConfig)
        {
            var correlationId = Guid.NewGuid().ToString();
            var message = new JsonMessage<TaskDispatchEvent>(new TaskDispatchEvent
            {
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                TaskId = Guid.NewGuid().ToString(),
                ExecutionId = Guid.NewGuid().ToString(),
                CorrelationId = correlationId,
                TaskPluginType = PluginStrings.Argo,
            }, applicationId: "TaskManagerRunner", correlationId: correlationId, deliveryTag: "1");
            message.Body.TaskPluginArguments.Add(Keys.BaseUrl, argBaseUri);
            message.Body.TaskPluginArguments.Add(Keys.WorkflowTemplateName, "list-input-artifacts-template");
            message.Body.TaskPluginArguments.Add(Keys.MessagingEnddpoint, @$"{wmConfig.Messaging.PublisherSettings["endpoint"]}/{wmConfig.Messaging.PublisherSettings["virtualHost"]}");
            message.Body.TaskPluginArguments.Add(Keys.MessagingUsername, wmConfig.Messaging.PublisherSettings["username"]);
            message.Body.TaskPluginArguments.Add(Keys.MessagingPassword, wmConfig.Messaging.PublisherSettings["password"]);
            message.Body.TaskPluginArguments.Add(Keys.MessagingExchange, wmConfig.Messaging.PublisherSettings["exchange"]);
            message.Body.TaskPluginArguments.Add(Keys.MessagingTopic, wmConfig.Messaging.Topics.TaskCallbackRequest);
            message.Body.Inputs.Add(new Messaging.Common.Storage
            {
                Name = "input-dicom",
                Endpoint = wmConfig.Storage.Settings["endpoint"],
                Bucket = wmConfig.Storage.Settings["bucket"],
                SecuredConnection = Convert.ToBoolean(wmConfig.Storage.Settings["securedConnection"], CultureInfo.InvariantCulture),
                RelativeRootPath = "/e08b7d7d-f30c-4f31-87d5-8ce5049aa956/dcm"
            });
            message.Body.Inputs.Add(new Messaging.Common.Storage
            {
                Name = "input-ehr",
                Endpoint = wmConfig.Storage.Settings["endpoint"],
                Bucket = wmConfig.Storage.Settings["bucket"],
                SecuredConnection = Convert.ToBoolean(wmConfig.Storage.Settings["securedConnection"], CultureInfo.InvariantCulture),
                RelativeRootPath = "/e08b7d7d-f30c-4f31-87d5-8ce5049aa956/ehr"
            });
            message.Body.Outputs.Add(new Messaging.Common.Storage
            {
                Name = "tempStorage",
                Endpoint = wmConfig.Storage.Settings["endpoint"],
                Bucket = wmConfig.Storage.Settings["bucket"],
                SecuredConnection = Convert.ToBoolean(wmConfig.Storage.Settings["securedConnection"], CultureInfo.InvariantCulture),
                RelativeRootPath = "/rabbit"
            });
            return message.ToMessage();
        }

        private static void PrintHelp()
        {
            Console.WriteLine($"Arguments: Argo_endpoint MinIO_endpoint");
            Console.WriteLine($"\te.g.: 'http://argo:2746/'");
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
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureLogging((builderContext, configureLogging) =>
                {
                    configureLogging.AddConfiguration(builderContext.Configuration.GetSection("Logging"));
                    configureLogging.AddFile(o => o.RootPath = AppContext.BaseDirectory);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions<WorkflowManagerOptions>().Bind(hostContext.Configuration.GetSection("WorkflowManager"));
                    services.AddOptions<StorageServiceConfiguration>().Bind(hostContext.Configuration.GetSection("WorkflowManager:storage"));
                    services.AddOptions<MessageBrokerServiceConfiguration>().Bind(hostContext.Configuration.GetSection("WorkflowManager:messaging"));
                    services.AddHttpClient();

                    services.AddMonaiDeployStorageService(hostContext.Configuration.GetSection("WorkflowManager:storage:serviceAssemblyName").Value);
                    services.AddMonaiDeployMessageBrokerPublisherService(hostContext.Configuration.GetSection("WorkflowManager:messaging:publisherServiceAssemblyName").Value);
                    services.AddMonaiDeployMessageBrokerSubscriberService(hostContext.Configuration.GetSection("WorkflowManager:messaging:subscriberServiceAssemblyName").Value);


                    services.AddSingleton<TaskManager>();
                    services.AddSingleton<IArgoProvider, ArgoProvider>();
                    services.AddSingleton<IKubernetesProvider, KubernetesProvider>();
                    services.AddTransient<IFileSystem, FileSystem>();

                    services.AddHostedService<TaskManager>(p => p.GetRequiredService<TaskManager>());
                });
    }
}
