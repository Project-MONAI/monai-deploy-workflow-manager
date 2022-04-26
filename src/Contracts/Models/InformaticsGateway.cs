// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class InformaticsGateway
    {
        [ConfigurationKeyName("ae_title")]
        public string AeTitle { get; set; }

        [ConfigurationKeyName("data_origins")]
        public string[] DataOrigins { get; set; }

        [ConfigurationKeyName("export_destinations")]
        public string[] ExportDestinations { get; set; }
    }
}
