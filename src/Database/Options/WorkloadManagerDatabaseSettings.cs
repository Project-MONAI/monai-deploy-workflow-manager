// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Database.Options
{
    public class WorkloadManagerDatabaseSettings
    {
        [ConfigurationKeyName("ConnectionString")]
        public string ConnectionString { get; set; } = null!;

        [ConfigurationKeyName("DatabaseName")]
        public string DatabaseName { get; set; } = null!;

        [ConfigurationKeyName("WorkflowCollectionName")]
        public string WorkflowCollectionName { get; set; } = null!;

        [ConfigurationKeyName("WorkflowInstanceCollectionName")]
        public string WorkflowInstanceCollectionName { get; set; } = null!;
    }
}
