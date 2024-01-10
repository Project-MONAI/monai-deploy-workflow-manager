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

using System.Reflection;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monai.Deploy.Security.Authentication.Configurations;
using Monai.Deploy.Security.Authentication.Extensions;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Argo;
using Monai.Deploy.WorkflowManager.TaskManager.Docker;
using Monai.Deploy.WorkflowManager.TaskManager.Services;
using NLog;

namespace Monai.Deploy.WorkflowManager.TaskManager.Extensions
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
            var logger = LogManager.GetCurrentClassLogger();

            ArgumentNullException.ThrowIfNull(hostContext, nameof(hostContext));

            services.AddTransient<IMonaiServiceLocator, MonaiServiceLocator>();

            // TODO: the plug-in dependencies need to be injected dynamically similar to how storage lib is loaded
            services.AddSingleton<IArgoProvider, ArgoProvider>();
            services.AddSingleton<IKubernetesProvider, KubernetesProvider>();

            services.AddTransient<IDockerClientFactory, DockerClientFactory>();
            services.AddTransient<IContainerStatusMonitor, ContainerStatusMonitor>();

            services.AddTransient<ITaskDispatchEventService, TaskDispatchEventService>();

            services.AddSingleton<TaskManager>();
            services.AddHostedService(p => p.GetRequiredService<TaskManager>());

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

            services.CheckAddControllerPlugins(hostContext.Configuration, logger);

            return services;
        }

        private static void CheckAddControllerPlugins(this IServiceCollection services, IConfiguration configuration, Logger logger)
        {
            var numberAdded = 0;
            var allFiles = Directory.GetFiles(".", "*.dll");
            foreach (var dll in allFiles)
            {
                var assembly = Assembly.LoadFrom(dll);
                if (assembly.CustomAttributes.Any(n => n.AttributeType.Name.Equals("PlugInAttribute")))
                {
                    services.AddControllers().AddApplicationPart(assembly).AddControllersAsServices();
                    logger.Debug($"Found a plugin with `PlugInAttribute` added plugin {assembly.GetName().Name}");
                    ++numberAdded;
                }
            }

            if (numberAdded > 0)
            {
                services.AddOptions<AuthenticationOptions>()
                    .Bind(configuration.GetSection("MonaiDeployAuthentication"));
                services.AddMonaiAuthentication();
            }
        }
    }
}
