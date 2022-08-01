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
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Parser;
using Monai.Deploy.WorkflowManager.PayloadListener.Services;
using Monai.Deploy.WorkflowManager.PayloadListener.Validators;
using Monai.Deploy.WorkflowManager.Storage.Services;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Common;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Services;

namespace Monai.Deploy.WorkflowManager.Services
{
    public static class WorkflowExecutorExtensions
    {
        public static IServiceCollection AddWorkflowExecutor(this IServiceCollection services, HostBuilderContext hostContext)
        {
            Guard.Against.Null(hostContext, nameof(hostContext));

            services.AddTransient<IWorkflowService, WorkflowService>();
            services.AddTransient<IWorkflowInstanceService, WorkflowInstanceService>();
            services.AddTransient<IPayloadService, PayloadService>();
            services.AddTransient<ITasksService, TasksService>();
            services.AddTransient<IDicomService, DicomService>();

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

                return new ConditionalParameterParser(logger, dicomStore, workflowInstanceService, payloadService, workflowService);
            });

            services.AddSingleton<PayloadListenerService>();
            services.AddHostedService(p => p.GetService<PayloadListenerService>());

            return services;
        }
    }
}
