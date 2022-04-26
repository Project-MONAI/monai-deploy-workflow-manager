// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using Microsoft.Extensions.Configuration;
using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class TaskExecution
    {
        [ConfigurationKeyName("execution_id")]
        public Guid ExecutionId { get; set; }

        [ConfigurationKeyName("task_type")]
        public string TaskType { get; set; }

        [ConfigurationKeyName("task_plugin_arguments")]
        public object TaskPluginArguments { get; set; }

        [ConfigurationKeyName("task_id")]
        public string TaskId { get; set; }

        [ConfigurationKeyName("status")]
        public Status Status { get; set; }

        [ConfigurationKeyName("input_artifacts")]
        public InputArtifacts InputArtifacts { get; set; }

        [ConfigurationKeyName("output_directory")]
        public string OutputDirectory { get; set; }

        [ConfigurationKeyName("metadata")]
        public object Metadata { get; set; }
    }
}
