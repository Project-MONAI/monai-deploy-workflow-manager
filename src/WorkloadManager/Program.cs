// Copyright 2021 MONAI Consortium
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkloadManager.Configuration;
using Monai.Deploy.WorkloadManager.Database;
using Monai.Deploy.WorkloadManager.Services.ApplicationDiscoveryService;
using Monai.Deploy.WorkloadManager.Services.DataDiscoveryService;
using Monai.Deploy.WorkloadManager.Services.DataExportCollectionService;
using Monai.Deploy.WorkloadManager.Services.DataRetentionService;
using Monai.Deploy.WorkloadManager.Services.Http;
using Monai.Deploy.WorkloadManager.Services.JobSchedulingService;
using Monai.Deploy.WorkloadManager.Services.NotificationService;
using System;
using System.IO;

namespace Monai.Deploy.WorkloadManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            InitializeDatabase(host);
            host.Run();
        }

        private static void InitializeDatabase(IHost host)
        {
            Guard.Against.Null(host, nameof(host));

            using (var serviceScope = host.Services.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<WorkloadManagerContext>();
                context.Database.Migrate();
            }
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
                    services.AddOptions<WorkloadManagerOptions>()
                        .Bind(hostContext.Configuration.GetSection("WorkloadManager"))
                        .PostConfigure(options =>
                        {
                        });
                    services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<WorkloadManagerOptions>, ConfigurationValidator>());

                    services.AddDbContext<WorkloadManagerContext>(
                        options => options.UseSqlite(hostContext.Configuration.GetConnectionString(WorkloadManagerOptions.DatabaseConnectionStringKey)),
                        ServiceLifetime.Transient);

                    services.AddSingleton<ConfigurationValidator>();

                    services.AddSingleton<ApplicationDiscoveryService>();
                    services.AddSingleton<JobSchedulingService>();
                    services.AddSingleton<NotificationService>();
                    services.AddSingleton<DataDiscoveryService>();
                    services.AddSingleton<DataExportCollectionService>();
                    services.AddSingleton<DataRetentionService>();

                    services.AddHostedService<ApplicationDiscoveryService>(p => p.GetService<ApplicationDiscoveryService>());
                    services.AddHostedService<JobSchedulingService>(p => p.GetService<JobSchedulingService>());
                    services.AddHostedService<NotificationService>(p => p.GetService<NotificationService>());
                    services.AddHostedService<DataDiscoveryService>(p => p.GetService<DataDiscoveryService>());
                    services.AddHostedService<DataExportCollectionService>(p => p.GetService<DataExportCollectionService>());
                    services.AddHostedService<DataRetentionService>(p => p.GetService<DataRetentionService>());
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.CaptureStartupErrors(true);
                    webBuilder.UseStartup<Startup>();
                });

        private static double GetConfigAndConvertToMinutes(IConfiguration configuration, string key, int defaultValue)
        {
            var configSection = configuration.GetSection(key);
            if (Int32.TryParse(configSection?.Value, out int value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}
