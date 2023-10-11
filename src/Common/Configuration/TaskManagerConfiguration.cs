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

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Common.Configuration
{
    public class TaskManagerConfiguration
    {
        [ConfigurationKeyName("concurrency")]
        public uint MaximumNumberOfConcurrentJobs { get; set; } = uint.MaxValue;

        [ConfigurationKeyName("plug-ins")]
        public Dictionary<string, string> PluginAssemblyMappings { get; set; }

        [ConfigurationKeyName("meta-data")]
        public Dictionary<string, string> MetadataAssemblyMappings { get; set; }

        [ConfigurationKeyName("storageCredentialDurationSeconds")]
        public int TemporaryStorageCredentialDurationSeconds { get; set; } = 3600;

        [ConfigurationKeyName("argoExitHookSendMessageContainerImage")]
        public string ArgoExitHookSendMessageContainerImage { get; set; } = "ghcr.io/jandelgado/rabtap:latest";

        [ConfigurationKeyName("argoPluginArguments")]
        public ArgoPluginArguments ArgoPluginArguments { get; set; } = new ArgoPluginArguments();

        public TaskManagerConfiguration()
        {
            PluginAssemblyMappings = new Dictionary<string, string>();
            MetadataAssemblyMappings = new Dictionary<string, string>();
        }
    }

    public class ArgoPluginArguments
    {
        [ConfigurationKeyName("server_url")]
        public string ServerUrl { get; set; } = string.Empty;

        [ConfigurationKeyName("initContainerCpuLimit")]
        public string InitContainerCpuLimit { get; set; } = "1";

        [ConfigurationKeyName("initContainerMemoryLimit")]
        public string InitContainerMemoryLimit { get; set; } = "500Mi";

        [ConfigurationKeyName("waitContainerCpuLimit")]
        public string WaitContainerCpuLimit { get; set; } = "1";

        [ConfigurationKeyName("waitContainerMemoryLimit")]
        public string WaitContainerMemoryLimit { get; set; } = "500Mi";

        [ConfigurationKeyName("messageGeneratorContainerCpuLimit")]
        public string MessageGeneratorContainerCpuLimit { get; set; } = "1";

        [ConfigurationKeyName("messageGeneratorContainerMemoryLimit")]
        public string MessageGeneratorContainerMemoryLimit { get; set; } = "500Mi";

        [ConfigurationKeyName("messageSenderContainerCpuLimit")]
        public string MessageSenderContainerCpuLimit { get; set; } = "1";

        [ConfigurationKeyName("messageSenderContainerMemoryLimit")]
        public string MessageSenderContainerMemoryLimit { get; set; } = "500Mi";

        [ConfigurationKeyName("taskPriorityClass")]
        public string TaskPriorityClass { get; set; } = string.Empty;
    }
}
