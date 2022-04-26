// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class Workflow
    {
        [ConfigurationKeyName("id")]
        public Guid Id { get; set; }

        [ConfigurationKeyName("name")]
        public string Name { get; set; }

        [ConfigurationKeyName("version")]
        public string Version { get; set; }

        [ConfigurationKeyName("description")]
        public string Description { get; set; }

        [ConfigurationKeyName("informatics_gateway")]
        public InformaticsGateway InformaticsGateway { get; set; }

        public TaskObject[] Tasks { get; set; }

        public TaskObject[] TaskTemplates { get; set; }
    }
}
