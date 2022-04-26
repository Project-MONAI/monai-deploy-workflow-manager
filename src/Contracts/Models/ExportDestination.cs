// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class ExportDestination
    {
        [ConfigurationKeyName("name")]
        public string Name { get; set; }

        [ConfigurationKeyName("conditions")]
        public Evaluator[] Conditions { get; set; }

        [ConfigurationKeyName("artifacts")]
        public Artifact[] Artifacts { get; set; }
    }
}
