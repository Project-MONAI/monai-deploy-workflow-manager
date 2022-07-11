// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.PayloadListener.Services;
using Monai.Deploy.WorkflowManager.PayloadListener.Validators;
using Monai.Deploy.WorkflowManager.Storage.Services;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Common;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Services;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Common;

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

                return new ConditionalParameterParser(logger, payloadService, workflowService, dicomStore);
            });

            services.AddSingleton<PayloadListenerService>();
            services.AddHostedService(p => p.GetService<PayloadListenerService>());

            return services;
        }
    }
}
