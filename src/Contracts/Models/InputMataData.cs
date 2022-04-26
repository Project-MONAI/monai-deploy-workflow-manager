// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class InputMataData
    {
        [ConfigurationKeyName("input_type")]
        public string InputType { get; set; }
    }
}
