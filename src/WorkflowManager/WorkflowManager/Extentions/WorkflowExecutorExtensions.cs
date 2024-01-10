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

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Parser;
using Monai.Deploy.WorkflowManager.Common.Database.Repositories;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.Services.InformaticsGateway;
using Monai.Deploy.WorkflowManager.Common.Storage.Services;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Common;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Services;
using Monai.Deploy.WorkflowManager.PayloadListener.Services;
using Monai.Deploy.WorkflowManager.PayloadListener.Validators;

namespace Monai.Deploy.WorkflowManager.Common.Extensions
{
    /// <summary>
    ///  Sets up workflow executor service collection.
    /// </summary>
    public static class WorkflowExecutorExtensions
    {
        /// <summary>
        /// Adds workflow executor and dependencies to service collection.
        /// </summary>
        /// <param name="services">Service collection to add workflow executor to.</param>
        /// <param name="hostContext"><see cref="HostBuilderContext"/> object.</param>
        /// <returns>Updated IServiceCollection.</returns>
        public static IServiceCollection AddWorkflowExecutor(this IServiceCollection services, HostBuilderContext hostContext)
        {
            ArgumentNullException.ThrowIfNull(hostContext, nameof(hostContext));

            services.AddTransient<IMonaiServiceLocator, MonaiServiceLocator>();
            services.AddTransient<IWorkflowService, WorkflowService>();
            services.AddTransient<IWorkflowInstanceService, WorkflowInstanceService>();
            services.AddTransient<IPayloadService, PayloadService>();
            services.AddTransient<ITasksService, TasksService>();
            services.AddTransient<IDicomService, DicomService>();
            services.AddTransient<IInformaticsGatewayService, InformaticsGatewayService>();

            services.AddSingleton<IArtifactsRepository, ArtifactsRepository>();
            services.AddSingleton<IEventPayloadReceiverService, EventPayloadReceiverService>();
            services.AddTransient<IEventPayloadValidator, EventPayloadValidator>();
            services.AddSingleton<IWorkflowExecuterService, WorkflowExecuterService>();
            services.AddSingleton<IArtifactMapper, ArtifactMapper>();

            services.AddSingleton<IConditionalParameterParser, ConditionalParameterParser>(s =>
            {
                var logger = s.GetService<ILogger<ConditionalParameterParser>>();
                var payloadService = s.GetService<IPayloadService>();
                var workflowService = s.GetService<IWorkflowService>();
                var dicomStore = s.GetService<IDicomService>();
                var workflowInstanceService = s.GetService<IWorkflowInstanceService>();

#pragma warning disable CS8604 // Possible null reference argument.
                return new ConditionalParameterParser(logger, dicomStore, workflowInstanceService, payloadService, workflowService);
#pragma warning restore CS8604 // Possible null reference argument.
            });

            services.AddSingleton<PayloadListenerService>();
            services.AddHostedService(p => p.GetService<PayloadListenerService>());

            return services;
        }
    }
}
