// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Configuration
{
    public class WorkflowManagerOptions
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

        [ConfigurationKeyName("taskManager")]
        public TaskManagerOptions TaskManager { get; set; }

        [ConfigurationKeyName("storage")]
        public StorageOptions Storage { get; set; }

        public WorkflowManagerOptions()
        {
            Messaging = new MessageBrokerConfiguration();
            TaskManager = new TaskManagerOptions();
            Storage = new StorageOptions();
        }
    }
}
