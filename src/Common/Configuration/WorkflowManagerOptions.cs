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
using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Common.Configuration
{
    public class WorkflowManagerOptions : PagedOptions
    {
        /// <summary>
        /// Name of the key for retrieve database connection string.
        /// </summary>
        public const string DatabaseConnectionStringKey = "WorkflowManagerDatabase";

        /// <summary>
        /// Represents the <c>messaging</c> section of the configuration file.
        /// </summary>
        [ConfigurationKeyName("messaging")]
        public MessageBrokerConfiguration Messaging { get; set; }

        /// <summary>
        /// Represents the <c>storage</c> section of the configuration file.
        /// </summary>
        [ConfigurationKeyName("storage")]
        public StorageConfiguration Storage { get; set; }

        /// <summary>
        /// Represents the <c>taskManager</c> section of the configuration file.
        /// </summary>
        [ConfigurationKeyName("taskManager")]
        public TaskManagerConfiguration TaskManager { get; set; }

        [ConfigurationKeyName("taskTimeoutMinutes")]
        public double TaskTimeoutMinutes { get; set; } = 60;

        [ConfigurationKeyName("perTaskTypeTimeoutMinutes")]
        public Dictionary<string, double> PerTaskTypeTimeoutMinutes { get; set; }


        public TimeSpan TaskTimeout { get => TimeSpan.FromMinutes(TaskTimeoutMinutes); }

        /// <summary>
        /// Represents the <c>backgroundServiceSettings</c> section of the configuration file.
        /// </summary>
        public BackgroundServiceSettings BackgroundServiceSettings { get; set; }

        [ConfigurationKeyName("argoTtlStrategyFailureSeconds")]
        public int ArgoTtlStrategyFailureSeconds { get; set; } = 60;

        [ConfigurationKeyName("argoTtlStrategySuccessSeconds")]
        public int ArgoTtlStrategySuccessSeconds { get; set; } = 60;

        [ConfigurationKeyName("minArgoTtlStrategySeconds")]
        public int MinArgoTtlStrategySeconds { get; set; } = 30; // time to get logs before cleanup !

        [ConfigurationKeyName("dicomTagsDisallowed")]
        public string DicomTagsDisallowed { get; set; } = string.Empty;

        [ConfigurationKeyName("migExternalAppPlugins")]
        public string[] MigExternalAppPlugins { get; set; }

        [ConfigurationKeyName("dataRetentionDays")]
        public int DataRetentionDays { get; set; }

        public WorkflowManagerOptions()
        {
            Messaging = new MessageBrokerConfiguration();
            TaskManager = new TaskManagerConfiguration();
            Storage = new StorageConfiguration();
            EndpointSettings = new EndpointSettings();
            BackgroundServiceSettings = new BackgroundServiceSettings();
        }
    }
}
