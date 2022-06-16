// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.Storage.MinioAdmin.Interfaces;
using Monai.Deploy.WorkflowManager.TaskManager.Argo;

namespace Monai.Deploy.WorkflowManager.Services
{
    internal static class TaskManagerExtensions
    {
        public static IServiceCollection AddTaskManager(this IServiceCollection services, HostBuilderContext hostContext)
        {
            Guard.Against.Null(hostContext, nameof(hostContext));

            services.AddSingleton<IArgoProvider, ArgoProvider>();
            services.AddSingleton<IKubernetesProvider, KubernetesProvider>();

            services.AddSingleton<IMinioAdmin>((implementationFactory) =>
            {
                var options = implementationFactory.GetService<IOptions<StorageServiceConfiguration>>();
                var executable = options.Value.Settings["executableLocation"];
                var endpoint = options.Value.Settings["endpoint"];
                var secretKey = options.Value.Settings["accessToken"];
                var accessKey = options.Value.Settings["accessKey"];
                return new Storage.MinioAdmin.Shell(executable, "minioApp", endpoint, accessKey, secretKey);
            });

            services.AddSingleton<TaskManager.TaskManager>();
            services.AddHostedService(p => p.GetRequiredService<TaskManager.TaskManager>());

            return services;
        }
    }
}
