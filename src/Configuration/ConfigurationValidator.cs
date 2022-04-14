// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Monai.Deploy.WorkflowManager.Configuration
{
    /// <summary>
    /// Validates configuration based on application requirements and DICOM VR requirements.
    /// </summary>
    public class ConfigurationValidator : IValidateOptions<WorkflowManagerOptions>
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

        public ValidateOptionsResult Validate(string name, WorkflowManagerOptions options)
        {
            var valid = true;

            valid &= IsTopicsValid(options.Messaging.Topics);

            _validationErrors.ForEach(p => _logger.Log(LogLevel.Error, p));

            return valid ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(string.Join(Environment.NewLine, _validationErrors));
        }

        public bool IsTopicsValid(MessageBrokerConfigurationKeys configurationKeys)
        {
            var valid = true;

            valid &= IsStringValueNotNull(nameof(configurationKeys.WorkflowRequest), configurationKeys.WorkflowRequest);
            valid &= IsStringValueNotNull(nameof(configurationKeys.ExportRequestPrefix), configurationKeys.ExportRequestPrefix);
            valid &= IsStringValueNotNull(nameof(configurationKeys.ExportComplete), configurationKeys.ExportComplete);

            return valid;
        }

        private bool IsStringValueNotNull(string source, string value)
        {
            if (value != null) return true;

            _validationErrors.Add($"Value of {source} must not be null.");
            return false;
        }
    }
}
