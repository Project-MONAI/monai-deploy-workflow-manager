/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Monai.Deploy.WorkflowManager.Common.Configuration
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
            valid &= IsStringValueNotNull(nameof(configurationKeys.TaskDispatchRequest), configurationKeys.TaskDispatchRequest);
            valid &= IsStringValueNotNull(nameof(configurationKeys.ExportHL7), configurationKeys.ExportHL7);
            valid &= IsStringValueNotNull(nameof(configurationKeys.ExportHL7Complete), configurationKeys.ExportHL7Complete);


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
