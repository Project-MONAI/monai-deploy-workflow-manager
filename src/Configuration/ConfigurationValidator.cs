// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Options;
using System;

namespace Monai.Deploy.WorkloadManager.Configuration
{
    public class ConfigurationValidator : IValidateOptions<WorkloadManagerOptions>
    {
        public ValidateOptionsResult Validate(string name, WorkloadManagerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
