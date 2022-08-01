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

using System.Net.Http;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monai.Deploy.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Argo;
using Monai.Deploy.WorkflowManager.TaskManager.Services;

namespace Monai.Deploy.WorkflowManager.Services
{
    /// <summary>
    ///  Sets up task manager service collection.
    /// </summary>
    public static class TaskManagerExtensions
    {
        /// <summary>
        /// Adds task manager and dependencies to service collection.
        /// </summary>
        /// <param name="services">Service collection to add task manager to.</param>
        /// <param name="hostContext">Hostcontext object.</param>
        /// <returns>Updated IServiceCollection.</returns>
        public static IServiceCollection AddTaskManager(this IServiceCollection services, HostBuilderContext hostContext)
        {
            Guard.Against.Null(hostContext, nameof(hostContext));

            services.AddSingleton<IArgoProvider, ArgoProvider>();
            services.AddSingleton<IKubernetesProvider, KubernetesProvider>();
            services.AddTransient<ITaskDispatchEventService, TaskDispatchEventService>();

            services.AddSingleton<TaskManager.TaskManager>();
            services.AddHostedService(p => p.GetRequiredService<TaskManager.TaskManager>());

            services.AddHttpClient("Argo");
            services.AddHttpClient("Argo-Insecure").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    },
            });

            return services;
        }
    }
}
