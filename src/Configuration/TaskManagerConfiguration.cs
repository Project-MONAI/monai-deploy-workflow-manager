// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Configuration
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

        [ConfigurationKeyName("taskTimeoutMinutes")]
        public double TaskTimeoutMinutes { get; set; } = 60;

        public TimeSpan TaskTimeout { get => TimeSpan.FromMinutes(TaskTimeoutMinutes); }

        public TaskManagerConfiguration()
        {
            PluginAssemblyMappings = new Dictionary<string, string>();
            MetadataAssemblyMappings = new Dictionary<string, string>();
        }
    }
}
