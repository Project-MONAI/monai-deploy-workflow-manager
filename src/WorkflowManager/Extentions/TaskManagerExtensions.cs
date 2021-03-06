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

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Monai.Deploy.Storage.API;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.Storage.MinIO;
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

            services.AddSingleton<IStorageAdminService, StorageAdminService>();

            services.AddSingleton<TaskManager.TaskManager>();
            services.AddHostedService(p => p.GetRequiredService<TaskManager.TaskManager>());

            return services;
        }
    }
}
