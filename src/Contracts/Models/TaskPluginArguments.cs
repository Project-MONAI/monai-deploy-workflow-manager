// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class TaskPluginArguments
    {
        [ConfigurationKeyName("workflow_id")]
        public string WorkflowId { get; set; }

        [ConfigurationKeyName("server_url")]
        public string ServerUrl { get; set; }
    }
}
