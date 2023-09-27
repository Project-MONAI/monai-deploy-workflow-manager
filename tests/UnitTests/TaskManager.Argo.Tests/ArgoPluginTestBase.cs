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
using System.Collections.Generic;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Moq;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.Tests
{
    public class ArgoPluginTestBase
    {
        protected readonly Mock<IServiceScopeFactory> ServiceScopeFactory;
        protected readonly Mock<IServiceScope> ServiceScope;
        protected readonly Mock<IKubernetesProvider> KubernetesProvider;
        protected readonly Mock<IArgoClient> ArgoClient;
        protected readonly Mock<IKubernetes> KubernetesClient;
        protected readonly Mock<ITaskDispatchEventService> TaskDispatchEventService;
        protected readonly IOptions<WorkflowManagerOptions> Options;
        // ReSharper disable once InconsistentNaming
        protected readonly Mock<ICoreV1Operations> K8sCoreOperations;
        protected readonly Mock<IArgoProvider> ArgoProvider;
        protected readonly int ArgoTtlStatergySeconds = 360;
        protected readonly int MinAgoTtlStatergySeconds = 30;
        protected readonly string InitContainerCpuLimit = "100m";
        protected readonly string InitContainerMemoryLimit = "200Mi";
        protected readonly string WaitContainerCpuLimit = "200m";
        protected readonly string WaitContainerMemoryLimit = "300Mi";
        protected readonly string MessageGeneratorContainerCpuLimit = "300m";
        protected readonly string MessageGeneratorContainerMemoryLimit = "400Mi";
        protected readonly string MessageSenderContainerCpuLimit = "400m";
        protected readonly string MessageSenderContainerMemoryLimit = "500Mi";

        public ArgoPluginTestBase()
        {
            Options = Microsoft.Extensions.Options.Options.Create(new WorkflowManagerOptions());

            ArgoProvider = new Mock<IArgoProvider>();
            K8sCoreOperations = new Mock<ICoreV1Operations>();

            ServiceScopeFactory = new Mock<IServiceScopeFactory>();
            ServiceScope = new Mock<IServiceScope>();
            KubernetesProvider = new Mock<IKubernetesProvider>();
            TaskDispatchEventService = new Mock<ITaskDispatchEventService>();
            ArgoClient = new Mock<IArgoClient>();
            KubernetesClient = new Mock<IKubernetes>();

            ServiceScopeFactory.Setup(p => p.CreateScope()).Returns(ServiceScope.Object);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IKubernetesProvider)))
                .Returns(KubernetesProvider.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IArgoProvider)))
                .Returns(ArgoProvider.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(ITaskDispatchEventService)))
                .Returns(TaskDispatchEventService.Object);

            ServiceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            KubernetesProvider.Setup(p => p.CreateClient()).Returns(KubernetesClient.Object);
            TaskDispatchEventService.Setup(p => p.UpdateTaskPluginArgsAsync(It.IsAny<TaskDispatchEventInfo>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(new TaskDispatchEventInfo(new TaskDispatchEvent()));
            KubernetesClient.SetupGet(p => p.CoreV1).Returns(K8sCoreOperations.Object);
            ArgoProvider.Setup(p => p.CreateClient(It.IsAny<string>(), It.IsAny<string?>(), true)).Returns(ArgoClient.Object);

            Options.Value.Messaging.PublisherSettings.Add("endpoint", "1.2.2.3/virtualhost");
            Options.Value.Messaging.PublisherSettings.Add("username", "username");
            Options.Value.Messaging.PublisherSettings.Add("password", "password");
            Options.Value.Messaging.PublisherSettings.Add("exchange", "exchange");
            Options.Value.Messaging.PublisherSettings.Add("virtualHost", "vhost");
            Options.Value.Messaging.Topics.TaskCallbackRequest = "md.tasks.callback";
            Options.Value.ArgoTtlStrategyFailureSeconds = ArgoTtlStatergySeconds;
            Options.Value.ArgoTtlStrategySuccessSeconds = ArgoTtlStatergySeconds;
            Options.Value.MinArgoTtlStrategySeconds = MinAgoTtlStatergySeconds;
            Options.Value.TaskManager.ArgoPluginArguments.InitContainerCpuLimit = InitContainerCpuLimit;
            Options.Value.TaskManager.ArgoPluginArguments.InitContainerMemoryLimit = InitContainerMemoryLimit;
            Options.Value.TaskManager.ArgoPluginArguments.WaitContainerCpuLimit = WaitContainerCpuLimit;
            Options.Value.TaskManager.ArgoPluginArguments.WaitContainerMemoryLimit = WaitContainerMemoryLimit;
            Options.Value.TaskManager.ArgoPluginArguments.MessageGeneratorContainerCpuLimit = MessageGeneratorContainerCpuLimit;
            Options.Value.TaskManager.ArgoPluginArguments.MessageGeneratorContainerMemoryLimit = MessageGeneratorContainerMemoryLimit;
            Options.Value.TaskManager.ArgoPluginArguments.MessageSenderContainerCpuLimit = MessageSenderContainerCpuLimit;
            Options.Value.TaskManager.ArgoPluginArguments.MessageSenderContainerMemoryLimit = MessageSenderContainerMemoryLimit;
            Options.Value.TaskManager.ArgoPluginArguments.TaskPriorityClass = "standard";
        }
    }
}
