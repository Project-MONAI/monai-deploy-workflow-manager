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

using Docker.DotNet;

namespace Monai.Deploy.WorkflowManager.TaskManager.Podman
{
    public interface IPodmanClientFactory
    {
        IDockerClient CreateClient(Uri podmanEndpoint);
    }

    public class PodmanClientFactory : IPodmanClientFactory
    {
        public IDockerClient CreateClient(Uri podmanEndpoint)
        {
            return new DockerClientConfiguration(podmanEndpoint).CreateClient();
        }
    }
}
