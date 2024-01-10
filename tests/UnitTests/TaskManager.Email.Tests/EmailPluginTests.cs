
/*
 * Copyright 2023 MONAI Consortium
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

using FellowOakDicom.Serialization;
using FellowOakDicom;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Moq;
using Xunit;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Microsoft.Extensions.DependencyInjection;

namespace Monai.Deploy.WorkflowManager.TaskManager.Email.Tests
{
    public class EmailPluginTests
    {
        private readonly Mock<ILogger<EmailPlugin>> _logger;
        private readonly Mock<IMessageBrokerPublisherService> _messageBrokerPublisherService = new Mock<IMessageBrokerPublisherService>();
        private readonly Mock<IStorageService> _storageService = new Mock<IStorageService>();
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory = new Mock<IServiceScopeFactory>();
        private readonly Mock<IServiceScope> _serviceScope = new Mock<IServiceScope>();
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly string _correlationId = Guid.NewGuid().ToString();
        private readonly string _executionId = Guid.NewGuid().ToString();
        private readonly string _taskPluginType = Guid.NewGuid().ToString();
        private readonly string _workflowInstanceId = Guid.NewGuid().ToString();
        private readonly string _taskId = Guid.NewGuid().ToString();
        private readonly string _workflowName = "NewWorkflow";

        public EmailPluginTests()
        {
            _logger = new Mock<ILogger<EmailPlugin>>();


            _options = Options.Create(new WorkflowManagerOptions());
            _options.Value.Messaging.Topics.AideClinicalReviewRequest = "aide.clinical_review.request";

            _serviceScopeFactory.Setup(p => p.CreateScope()).Returns(_serviceScope.Object);
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IStorageService)))
                .Returns(_storageService.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IMessageBrokerPublisherService)))
                .Returns(_messageBrokerPublisherService.Object);

            _serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            _logger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        }

        [Fact(DisplayName = "Throws when missing required plug-in arguments")]
        public void EmailPlugin_ThrowsWhenMissingPluginArguments()
        {
            var message = GenerateTaskDispatchEvent();
            Assert.Throws<InvalidTaskException>(() => new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));

            message.TaskPluginArguments.Add(ValidationConstants.RecipientEmails, "  , ");
            Assert.Throws<InvalidTaskException>(() => new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));

            message.TaskPluginArguments.Add(ValidationConstants.RecipientRoles, "  , ");
            Assert.Throws<InvalidTaskException>(() => new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));
        }

        [Fact(DisplayName = "Throws when missing required logger")]
        public void EmailPlugin_ThrowsWhenMissingLogger()
        {
            var message = GenerateTaskDispatchEvent();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => new EmailPlugin(_serviceScopeFactory.Object, null, _options, message));
        }

        [Fact(DisplayName = "Throws when missing required option")]
        public void EmailPlugin_ThrowsWhenMissingOptions()
        {
            var message = GenerateTaskDispatchEvent();
            Assert.Throws<ArgumentNullException>(() => new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, null, message));
        }

        [Fact(DisplayName = "Throws when missing required scope")]
        public void EmailPlugin_ThrowsWhenMissingScope()
        {
            var message = GenerateTaskDispatchEvent();
            Assert.Throws<ArgumentNullException>(() => new EmailPlugin(null, _logger.Object, _options, message));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Fact(DisplayName = "Throws when missing required EventMessage")]
        public void EmailPlugin_ThrowsWhenMissingMessage()
        {
            var message = GenerateTaskDispatchEvent();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, _options, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Fact(DisplayName = "Throws when missing required attribute")]
        public void EmailPlugin_ThrowsWhenMissingRequiredAttribut()
        {
            var message = new TaskDispatchEvent
            {
                CorrelationId = _correlationId,
                ExecutionId = _executionId,
                TaskPluginType = _taskPluginType,
                WorkflowInstanceId = _workflowInstanceId,
                TaskId = _taskId,
            };
            Assert.Throws<InvalidTaskException>(() => new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));
        }

        [Fact(DisplayName = "Ok when One or other required plug-in arguments are present")]
        public void EmailPlugin_OkWhenPluginArgumentsPresent()
        {
            var message = GenerateTaskDispatchEvent();

            message.TaskPluginArguments.Add(ValidationConstants.RecipientEmails, "fred@fred.com");
            new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);

            message.TaskPluginArguments.Remove(ValidationConstants.RecipientEmails);
            message.TaskPluginArguments.Add(ValidationConstants.RecipientRoles, "fredRole");
            new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        }

        [Fact(DisplayName = "Throws when email address is invalid")]
        public void EmailPlugin_ThrowsOnInvalidEmailAddress()
        {
            var message = GenerateTaskDispatchEvent();

            message.TaskPluginArguments.Add(ValidationConstants.RecipientEmails, "fred@fred.co.uk, @fred..com");
            Assert.Throws<InvalidTaskException>(() => new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));

            message = GenerateTaskDispatchEvent();
            message.TaskPluginArguments.Add(ValidationConstants.RecipientEmails, "..com, fred@fred.co.uk");
            Assert.Throws<InvalidTaskException>(() => new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));
        }

        [Fact(DisplayName = "ExecuteTask - returns ExecutionStatus on failure")]
        public async Task EmailPlugin_ExecuteTask_ReturnsExecutionStatusOnFailure()
        {
            var message = GenerateTaskDispatchEvent();
            message.TaskPluginArguments.Add(ValidationConstants.RecipientEmails, "fred@fred.com");

            _messageBrokerPublisherService.Setup(p => p.Publish(It.IsAny<string>(), It.IsAny<Message>()))
            .ThrowsAsync(new Exception());

            var runner = new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
            var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            Assert.Equal(TaskExecutionStatus.Failed, result.Status);
            Assert.Equal(FailureReason.PluginError, result.FailureReason);
            Assert.Equal("Exception of type 'System.Exception' was thrown.", result.Errors);
        }

        [Fact(DisplayName = "Publish Message called for valid event")]
        public async Task EmailPlugin_ExecuteTask_CallsPublish()
        {
            var messageEvent = GenerateTaskDispatchEvent();
            messageEvent.TaskPluginArguments.Add(ValidationConstants.RecipientEmails, "fred@fred.com");
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Message messageResult = default;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            _messageBrokerPublisherService.Setup(p => p.Publish(It.IsAny<string>(), It.IsAny<Message>()))
                .Callback((string topic, Message jsonMessage) =>
                {
                    messageResult = jsonMessage;
                });

            var runner = new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, _options, messageEvent);
            var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            _messageBrokerPublisherService.Verify(m => m.Publish(It.IsAny<string>(), It.IsAny<Message>()), Times.Once);
            Assert.Contains("fred@fred.com", System.Text.Encoding.UTF8.GetString(messageResult!.Body));
        }

        //[Fact(DisplayName = "Should_Log_If_No_Metadata_Found")]
        //public async Task EmailPlugin_Log_If_no_meta_found()
        //{
        //    var name = "testyMcTestface";
        //    var metaString = $"{{\"name\":\"{name}\",\"dontIncludeName\":\"dontIncludeName\"}}";
        //    var dcm = new DicomFile();
        //    dcm.Dataset.Add(DicomDictionary.Default["SpecificCharacterSet"], "Iso");

        //    _storageService.Setup(s => s.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //        .Returns(Task.FromResult((Stream)new MemoryStream(Encoding.UTF8.GetBytes(metaString))));

        //    var message = GenerateTaskDispatchEvent();
        //    AddInputToTask(message);

        //    message.TaskPluginArguments.Add(ValidationConstants.RecipientEmails, "fred@fred.com");

        //    Message messageResult = default;
        //    _messageBrokerPublisherService.Setup(p => p.Publish(It.IsAny<string>(), It.IsAny<Message>()))
        //        .Callback((string topic, Message jsonMessage) =>
        //        {
        //            messageResult = jsonMessage;
        //        });

        //    var plugin = new EmailPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        //    await plugin.ExecuteTask(CancellationToken.None);

        //    var expectedErrorMessage = "No Metadata found in first dcm/json";
        //    _logger.Verify(logger => logger.Log(
        //        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Debug),
        //        It.Is<EventId>(eventId => eventId.Id == 4),
        //        It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == expectedErrorMessage),
        //        It.IsAny<Exception>(),
        //        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        //        Times.Once);
        //}

        private static readonly IList<DicomVR> DicomVrsToIgnore = new List<DicomVR>() { DicomVR.OB, DicomVR.OD, DicomVR.OF, DicomVR.OL, DicomVR.OV, DicomVR.OW, DicomVR.UN };

        private string ToJson(DicomFile dicomFile, bool validateDicom)
        {
            ArgumentNullException.ThrowIfNull(dicomFile, nameof(dicomFile));

            var options = new JsonSerializerOptions();
            options.Converters.Add(new DicomJsonConverter(
                writeTagsAsKeywords: false,
                autoValidate: validateDicom,
                numberSerializationMode: validateDicom ? NumberSerializationMode.AsNumber : NumberSerializationMode.PreferablyAsNumber));
            options.WriteIndented = false;

            //return JsonSerializer.Serialize(dicomFile.Dataset, options);
            var dataset = dicomFile.Dataset.Clone();
            dataset.Remove(i => DicomVrsToIgnore.Contains(i.ValueRepresentation));
            return JsonSerializer.Serialize(dataset, options);
        }

        private void AddInputToTask(TaskDispatchEvent dispatchEvent)
        {
            dispatchEvent.Inputs.Add(new Messaging.Common.Storage
            {
                Bucket = "bucket",
                Name = "fileName.json",
                RelativeRootPath = "root/",
            });
        }

        private TaskDispatchEvent GenerateTaskDispatchEvent()
        {
            var message = new TaskDispatchEvent
            {
                CorrelationId = _correlationId,
                ExecutionId = _executionId,
                TaskPluginType = _taskPluginType,
                WorkflowInstanceId = _workflowInstanceId,
                TaskId = _taskId,
            };
            message.TaskPluginArguments.Add(ValidationConstants.MetadataValues, "name,address");
            message.TaskPluginArguments.Add(ValidationConstants.WorkflowName, _workflowName);
            return message;
        }
    }
}
