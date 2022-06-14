// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.PayloadListener.Services;
using Monai.Deploy.WorkflowManager.PayloadListener.Validators;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Common;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Services;

namespace Monai.Deploy.WorkflowManager.Services
{
    internal static class WorkflowExecutorExtensions
    {
        public static IServiceCollection AddWorkflowExecutorIfEnabled(this IServiceCollection services, HostBuilderContext hostContext)
        {
            Guard.Against.Null(hostContext, nameof(hostContext));

            var config = hostContext.Configuration.GetSection("WorkflowManager");

            if (config is null)
            {
                throw new ConfigurationException("WorkflowManager is missing");
            }

            if (config["disabled"] == null || bool.Parse(config["disabled"]) == false)
            {
                services.AddTransient<IWorkflowService, WorkflowService>();
                services.AddTransient<IWorkflowInstanceService, WorkflowInstanceService>();

                services.AddSingleton<IEventPayloadReceiverService, EventPayloadReceiverService>();
                services.AddTransient<IEventPayloadValidator, EventPayloadValidator>();
                services.AddSingleton<IWorkflowExecuterService, WorkflowExecuterService>();
                services.AddSingleton<IArtifactMapper, ArtifactMapper>();

                services.AddSingleton<PayloadListenerService>();
                services.AddHostedService(p => p.GetService<PayloadListenerService>());

                return services;
            }

            return services;
        }
    }
}
