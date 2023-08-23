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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.SharedTest;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.Configuration.Tests
{
    public class ConfigurationValidatorTest
    {
        private readonly Mock<ILogger<ConfigurationValidator>> _logger;

        public ConfigurationValidatorTest()
        {
            _logger = new Mock<ILogger<ConfigurationValidator>>();
        }

        [Fact(DisplayName = "ConfigurationValidator test with all valid settings")]
        public void AllValid()
        {
            var config = MockValidConfiguration();
            var valid = new ConfigurationValidator(_logger.Object).Validate("", config);
            Assert.True(valid == ValidateOptionsResult.Success);
        }

        [Fact(DisplayName = "ConfigurationValidator test with invalid topic")]
        public void InvalidTopic()
        {
            var config = MockValidConfiguration();
            config.Messaging.Topics.WorkflowRequest = null;

            var valid = new ConfigurationValidator(_logger.Object).Validate("", config);

            var validationMessage = "Value of WorkflowRequest must not be null.";
            Assert.Equal(validationMessage, valid.FailureMessage);
            _logger.VerifyLogging(validationMessage, LogLevel.Error, Times.Once());
        }

        private static WorkflowManagerOptions MockValidConfiguration()
        {
            return new WorkflowManagerOptions
            {
                Messaging = new MessageBrokerConfiguration
                {
                    Topics = new MessageBrokerConfigurationKeys
                    {
                        TaskDispatchRequest = "TaskDispatchRequest",
                        ExportRequestPrefix = "ExportRequestPrefix",
                        WorkflowRequest = "WorkflowRequest"
                    }
                }
            };
        }
    }
}
