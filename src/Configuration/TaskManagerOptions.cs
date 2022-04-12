// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Configuration
{
    public class TaskManagerOptions
    {
        [ConfigurationKeyName("concurrency")]
        public uint MaximumNumberOfConcurrentJobs { get; set; } = 1;
    }
}
