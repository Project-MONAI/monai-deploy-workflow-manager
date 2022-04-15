// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using k8s;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Logging;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    public class KubernetesProvider : IKubernetesProvider
    {
        private readonly ILogger<KubernetesProvider> _logger;

        public KubernetesProvider(ILogger<KubernetesProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public Kubernetes CreateClient()
        {
            _logger.CreatingKubernetesClient();

            var configuration = KubernetesClientConfiguration.BuildDefaultConfig();
            return new Kubernetes(configuration);
        }
    }
}
