// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Options;

namespace Monai.Deploy.WorkflowManager.Configuration
{
    public class ConfigurationValidator : IValidateOptions<WorkflowManagerOptions>
    {
        public ValidateOptionsResult Validate(string name, WorkflowManagerOptions options)
        {
            return new ValidateOptionsResult();
        }
    }
}
