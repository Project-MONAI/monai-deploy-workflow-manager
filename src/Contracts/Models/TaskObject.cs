// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class TaskObject
    {
        [ConfigurationKeyName("id")]
        public string Id { get; set; }

        [ConfigurationKeyName("description")]
        public string Description { get; set; }

        [ConfigurationKeyName("type")]
        public string Type { get; set; }

        [ConfigurationKeyName("args")]
        public object Args { get; set; }

        [ConfigurationKeyName("ref")]
        public string Ref { get; set; }

        [ConfigurationKeyName("task_destinations")]
        public TaskDestination[] TaskDestinations { get; set; }

        [ConfigurationKeyName("export_destinations")]
        public TaskDestination[] ExportDestinations { get; set; }

        [ConfigurationKeyName("artifacts")]
        public ArtifactMap Artifacts { get; set; }
    }
}
