// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Configuration
{
    public class TaskManagerOptions
    {
        [ConfigurationKeyName("concurrency")]
        public uint MaximumNumberOfConcurrentJobs { get; set; } = uint.MaxValue;

        [ConfigurationKeyName("timeout")]
        public double TaskTimeoutMinutes { get; set; } = 60;

        public TimeSpan TaskTimeout { get => TimeSpan.FromMinutes(TaskTimeoutMinutes); }

        public double RunnerScanIntervalMs { get; set; } = 10000;
    }
}
