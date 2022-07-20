// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monai.Deploy.Storage.API;
using Monai.Deploy.Storage.MinIO;
using Monai.Deploy.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Argo;
using Monai.Deploy.WorkflowManager.TaskManager.Services;

namespace Monai.Deploy.WorkflowManager.Services
{
    public static class TaskManagerExtensions
    {
        public static IServiceCollection AddTaskManager(this IServiceCollection services, HostBuilderContext hostContext)
        {
            Guard.Against.Null(hostContext, nameof(hostContext));

            services.AddSingleton<IArgoProvider, ArgoProvider>();
            services.AddSingleton<IKubernetesProvider, KubernetesProvider>();
            services.AddTransient<ITaskDispatchEventService, TaskDispatchEventService>();

            services.AddSingleton<IStorageAdminService, StorageAdminService>();

            services.AddSingleton<TaskManager.TaskManager>();
            services.AddHostedService(p => p.GetRequiredService<TaskManager.TaskManager>());

            return services;
        }
    }
}
