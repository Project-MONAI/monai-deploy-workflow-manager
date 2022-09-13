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

        public IKubernetes CreateClient()
        {
            var configuration = KubernetesClientConfiguration.BuildDefaultConfig();
            _logger.CreatingKubernetesClient(configuration.Host, configuration.Namespace);

            return new Kubernetes(configuration);
        }
    }
}
