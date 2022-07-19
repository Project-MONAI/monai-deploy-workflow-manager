// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Configuration
{
    public class EndpointSettings
    {
        [ConfigurationKeyName("defaultPageSize")]
        public int DefaultPageSize { get; set; } = 10;

        [ConfigurationKeyName("maxPageSize")]
        public int MaxPageSize { get; set; } = 10;
    }
}
