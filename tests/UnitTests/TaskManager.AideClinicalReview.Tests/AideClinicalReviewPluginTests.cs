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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Moq;
using Xunit;

namespace TaskManager.AideClinicalReview.Tests
{
    public class AideClinicalReviewPluginTests
    {
        private readonly Mock<ILogger<AideClinicalReviewPlugin>> _logger;
        private readonly Mock<IMessageBrokerPublisherService> _messageBrokerPublisherService;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly Mock<IServiceScope> _serviceScope;

        public AideClinicalReviewPluginTests()
        {
            _logger = new Mock<ILogger<AideClinicalReviewPlugin>>();
            _messageBrokerPublisherService = new Mock<IMessageBrokerPublisherService>();

            _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            _serviceScope = new Mock<IServiceScope>();
            _options = Options.Create(new WorkflowManagerOptions());
            _options.Value.Messaging.Topics.AideClinicalReviewRequest = "aide.clinical_review.request";

            _serviceScopeFactory.Setup(p => p.CreateScope()).Returns(_serviceScope.Object);

            var serviceProvider = new Mock<IServiceProvider>();

            _serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            _logger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        }

        [Fact(DisplayName = "Throws when missing required plug-in arguments")]
        public void AideClinicalReviewPlugin_ThrowsWhenMissingPluginArguments()
        {
            var message = GenerateTaskDispatchEvent();
            Assert.Throws<InvalidTaskException>(() => new AideClinicalReviewPlugin(_serviceScopeFactory.Object, _messageBrokerPublisherService.Object, _options, _logger.Object, message));

            foreach (var key in Keys.RequiredParameters.Take(Keys.RequiredParameters.Count - 1))
            {
                message.TaskPluginArguments.Add(key, Guid.NewGuid().ToString());
                Assert.Throws<InvalidTaskException>(() => new AideClinicalReviewPlugin(_serviceScopeFactory.Object, _messageBrokerPublisherService.Object, _options, _logger.Object, message));
            }
        }

        [Fact(DisplayName = "ExecuteTask - returns ExecutionStatus on failure")]
        public async Task AideClinicalReviewPlugin_ExecuteTask_ReturnsExecutionStatusOnFailure()
        {
            var message = GenerateTaskDispatchEventWithValidArguments();

            _messageBrokerPublisherService.Setup(p => p.Publish(It.IsAny<string>(), It.IsAny<Message>()))
                .ThrowsAsync(new Exception());

            var runner = new AideClinicalReviewPlugin(_serviceScopeFactory.Object, _messageBrokerPublisherService.Object, _options, _logger.Object, message);
            var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(TaskExecutionStatus.Failed, result.Status);
            Assert.Equal(FailureReason.PluginError, result.FailureReason);
            Assert.Equal("Exception of type 'System.Exception' was thrown.", result.Errors);
        }

        [Fact(DisplayName = "ExecuteTask - returns ExecutionStatus on success")]
        public async Task AideClinicalReviewPlugin_ExecuteTask_ReturnsExecutionStatusOnSuccess()
        {
            var message = GenerateTaskDispatchEventWithValidArguments();

            var runner = new AideClinicalReviewPlugin(_serviceScopeFactory.Object, _messageBrokerPublisherService.Object, _options, _logger.Object, message);
            var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
            Assert.Equal(FailureReason.None, result.FailureReason);
            Assert.Equal("", result.Errors);

            _messageBrokerPublisherService.Verify(p => p.Publish(It.Is<string>(m => m == _options.Value.Messaging.Topics.AideClinicalReviewRequest), It.IsAny<Message>()), Times.Once());
        }

        private static TaskDispatchEvent GenerateTaskDispatchEventWithValidArguments()
        {
            var message = GenerateTaskDispatchEvent();
            message.TaskPluginArguments[Keys.WorkflowName] = "workflowName";
            message.TaskPluginArguments[Keys.PatientName] = "patientname";
            message.TaskPluginArguments[Keys.PatientId] = "patientid";
            message.TaskPluginArguments[Keys.PatientDob] = "patientdob";
            message.TaskPluginArguments[Keys.PatientSex] = "patientsex";
            message.TaskPluginArguments[Keys.ReviewedTaskId] = "reviewedtaskid";
            message.TaskPluginArguments[Keys.ReviewedExecutionId] = "reviewedexecutionid";
            message.TaskPluginArguments[Keys.ApplicationVersion] = "applicationversion";
            message.TaskPluginArguments[Keys.ApplicationName] = "applicationname";
            message.TaskPluginArguments[Keys.Mode] = "mode";
            return message;
        }

        private static TaskDispatchEvent GenerateTaskDispatchEvent()
        {
            var message = new TaskDispatchEvent
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ExecutionId = Guid.NewGuid().ToString(),
                TaskPluginType = Guid.NewGuid().ToString(),
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                TaskId = Guid.NewGuid().ToString(),
                IntermediateStorage = new Storage
                {
                    Name = Guid.NewGuid().ToString(),
                    Endpoint = Guid.NewGuid().ToString(),
                    Credentials = new Credentials
                    {
                        AccessKey = Guid.NewGuid().ToString(),
                        AccessToken = Guid.NewGuid().ToString()
                    },
                    Bucket = Guid.NewGuid().ToString(),
                    RelativeRootPath = Guid.NewGuid().ToString(),
                }
            };
            message.Inputs.Add(new Storage
            {
                Name = "input-dicom",
                Endpoint = Guid.NewGuid().ToString(),
                Credentials = new Credentials
                {
                    AccessKey = Guid.NewGuid().ToString(),
                    AccessToken = Guid.NewGuid().ToString()
                },
                Bucket = Guid.NewGuid().ToString(),
                RelativeRootPath = Guid.NewGuid().ToString(),
            });
            message.Inputs.Add(new Storage
            {
                Name = "input-ehr",
                Endpoint = Guid.NewGuid().ToString(),
                Credentials = new Credentials
                {
                    AccessKey = Guid.NewGuid().ToString(),
                    AccessToken = Guid.NewGuid().ToString()
                },
                Bucket = Guid.NewGuid().ToString(),
                RelativeRootPath = Guid.NewGuid().ToString(),
            });
            message.Outputs.Add(new Storage
            {
                Name = "output",
                Endpoint = Guid.NewGuid().ToString(),
                Credentials = new Credentials
                {
                    AccessKey = Guid.NewGuid().ToString(),
                    AccessToken = Guid.NewGuid().ToString()
                },
                Bucket = Guid.NewGuid().ToString(),
                RelativeRootPath = Guid.NewGuid().ToString(),
            });
            return message;
        }
    }
}
