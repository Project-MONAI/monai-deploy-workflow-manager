﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Monai.Deploy.WorkflowManager.Configuration
{
    public class ConfigurationValidator : IValidateOptions<WorkflowManagerOptions>
    /// <summary>
    /// Validates configuration based on application requirements and DICOM VR requirements.
    /// </summary>
    public class ConfigurationValidator : IValidateOptions<WorkloadManagerOptions>
    {
        private readonly ILogger<ConfigurationValidator> _logger;
        private readonly List<string> _validationErrors;

        /// <summary>
        /// Initializes an instance of the <see cref="ConfigurationValidator"/> class.
        /// </summary>
        /// <param name="configuration">InformaticsGatewayConfiguration to be validated</param>
        /// <param name="logger">Logger to be used by ConfigurationValidator</param>
        public ConfigurationValidator(ILogger<ConfigurationValidator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validationErrors = new List<string>();
        }

        public ValidateOptionsResult Validate(string name, WorkloadManagerOptions options)
        {
            var valid = true;

            _validationErrors.ForEach(p => _logger.Log(LogLevel.Error, p));

            return valid ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(string.Join(Environment.NewLine, _validationErrors));
        }
    }
}
