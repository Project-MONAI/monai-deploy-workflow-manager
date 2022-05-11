// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.InformaticsGateway.SharedTest;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Configuration.Tests
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
