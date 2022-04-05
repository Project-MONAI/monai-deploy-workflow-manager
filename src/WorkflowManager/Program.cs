// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Services.DataRetentionService;
using Monai.Deploy.WorkflowManager.Services.Http;
using Monai.Deploy.MessageBroker;
using Monai.Deploy.MessageBroker.RabbitMq;
using Monai.Deploy.WorkloadManager.Common;
using Monai.Deploy.WorkloadManager.Configuration;
using Monai.Deploy.WorkloadManager.Services.DataRetentionService;
using Monai.Deploy.WorkloadManager.Services.Http;

namespace Monai.Deploy.WorkflowManager
{
    internal class Program
    {
        protected Program()
        { }

        private static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
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
                    services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<WorkflowManagerOptions>, ConfigurationValidator>());

                    services.AddSingleton<ConfigurationValidator>();

                    services.AddSingleton<DataRetentionService>();

                    services.AddHostedService<DataRetentionService>(p => p.GetService<DataRetentionService>());

                    // MessageBroker
                    services.AddSingleton<RabbitMqMessagePublisherService>();
                    services.AddSingleton<IMessageBrokerPublisherService>(implementationFactory =>
                    {
                        var options = implementationFactory.GetService<IOptions<WorkloadManagerOptions>>();
                        var serviceProvider = implementationFactory.GetService<IServiceProvider>();
                        var logger = implementationFactory.GetService<ILogger<Program>>();
                        return serviceProvider.LocateService<IMessageBrokerPublisherService>(logger, options.Value.Messaging.PublisherServiceAssemblyName);
                    });

                    services.AddSingleton<RabbitMqMessageSubscriberService>();
                    services.AddSingleton<IMessageBrokerSubscriberService>(implementationFactory =>
                    {
                        var options = implementationFactory.GetService<IOptions<WorkloadManagerOptions>>();
                        var serviceProvider = implementationFactory.GetService<IServiceProvider>();
                        var logger = implementationFactory.GetService<ILogger<Program>>();
                        return serviceProvider.LocateService<IMessageBrokerSubscriberService>(logger, options.Value.Messaging.SubscriberServiceAssemblyName);
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.CaptureStartupErrors(true);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
