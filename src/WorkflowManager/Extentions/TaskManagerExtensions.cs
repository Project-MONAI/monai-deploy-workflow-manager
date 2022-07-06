// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.Argo;

namespace Monai.Deploy.WorkflowManager.Services
{
    public static class TaskManagerExtensions
    {
        public static IServiceCollection AddTaskManager(this IServiceCollection services, HostBuilderContext hostContext)
        {
            Guard.Against.Null(hostContext, nameof(hostContext));

            services.AddSingleton<IArgoProvider, ArgoProvider>();
            services.AddSingleton<IKubernetesProvider, KubernetesProvider>();

            services.AddSingleton<TaskManager.TaskManager>();
            services.AddHostedService(p => p.GetRequiredService<TaskManager.TaskManager>());

            return services;
        }
    }
}
