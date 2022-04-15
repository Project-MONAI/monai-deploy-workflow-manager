// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Messaging.RabbitMq;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.MinIo;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.Argo;

namespace Monai.Deploy.WorkflowManager.TaskManager.Runner
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var exitEvent = new ManualResetEvent(false);
            var host = CreateHostBuilder(args).Build();
            var task = host.StartAsync();

            var taskManager = host.Services.GetRequiredService<TaskManager>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            while (taskManager.Status != Contracts.Rest.ServiceStatus.Running)
            {
                logger.LogInformation($"Waitng for Task Manager to be ready: state={taskManager.Status}...");
                await Task.Delay(100);
            }
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            await Task.Run(() =>
            {
                var correlationId = Guid.NewGuid().ToString();
                var message = new JsonMessage<TaskDispatchEvent>(new TaskDispatchEvent
                {
                    WorkflowId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    CorrelationId = correlationId,
                    TaskAssemblyName = typeof(ArgoRunner).AssemblyQualifiedName!,

                }, applicationId: "TaskManagerRunner", correlationId: correlationId, deliveryTag: "1");
                message.Body.TaskPluginArguments.Add(Keys.BaseUrl, "http://10.110.161.149:2746/");
                message.Body.TaskPluginArguments.Add(Keys.WorkflowTemplateName, "list-input-artifacts-template");
                message.Body.TaskPluginArguments.Add(Keys.WorkflowTemplateTemplateRefName, "s3-artifacts-template");
                message.Body.TaskPluginArguments.Add(Keys.ExitWorkflowTemplateName, "http-template");
                message.Body.TaskPluginArguments.Add(Keys.ExitWorkflowTemplateTemplateRefName, "http");
                message.Body.Inputs.Add(new Messaging.Common.Storage
                {
                    Name = "input-dicom",
                    Endpoint = "10.101.65.34",
                    Credentials = new Messaging.Common.Credentials
                    {
                        AccessKey = "minio",
                        AccessToken = "monaideploy"
                    },
                    Bucket = "monaideploy",
                    SecuredConnection = false,
                    RelativeRootPath = "/4acc37cd-5e45-4e60-af9a-8c0a96fb5583/dcm"
                });
                message.Body.Inputs.Add(new Messaging.Common.Storage
                {
                    Name = "input-ehr",
                    Endpoint = "10.101.65.34",
                    Credentials = new Messaging.Common.Credentials
                    {
                        AccessKey = "minio",
                        AccessToken = "monaideploy"
                    },
                    Bucket = "monaideploy",
                    SecuredConnection = false,
                    RelativeRootPath = "/4acc37cd-5e45-4e60-af9a-8c0a96fb5583/ehr"
                });
                logger.LogInformation($"Queueing new job with correlation ID={correlationId}.");
                taskManager.QueueTask(message);
            });

            exitEvent.WaitOne();
            logger.LogInformation("Stopping Task Manager...");

            await host.StopAsync(CancellationToken.None);
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
                    services.AddOptions<MessageBrokerServiceConfiguration>().Bind(hostContext.Configuration.GetSection("RabbitMQ"));

                    services.AddHttpClient();
                    services.UseRabbitMq();
                    services.AddSingleton<IStorageService, MinIoStorageService>();
                    services.AddSingleton<IMessageBrokerPublisherService, RabbitMqMessagePublisherService>();
                    services.AddSingleton<IMessageBrokerSubscriberService, RabbitMqMessageSubscriberService>();

                    services.AddSingleton<TaskManager>();
                    services.AddSingleton<IArgoProvider, ArgoProvider>();
                    services.AddSingleton<IKubernetesProvider, KubernetesProvider>();

                    services.AddHostedService<TaskManager>(p => p.GetService<TaskManager>());
                });
    }
}
