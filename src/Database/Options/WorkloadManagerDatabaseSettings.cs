// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.Database.Options
{
    public class WorkloadManagerDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string WorkflowCollectionName { get; set; } = null!;

        public string WorkflowInstanceCollectionName { get; set; } = null!;
    }
}
