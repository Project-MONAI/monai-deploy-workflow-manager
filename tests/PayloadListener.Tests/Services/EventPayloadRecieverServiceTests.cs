using NUnit.Framework;
using Moq;
using Monai.Deploy.WorkflowManager.PayloadListener.Services;
using System;
using Monai.Deploy.WorkflowManager.PayloadListener.Validators;
using Monai.Deploy.Messaging.Messages;
using System.Threading;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.Messaging.Common;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Tests.Services
{
    public class EventPayloadRecieverServiceTests
    {
        private IEventPayloadRecieverService _eventPayloadRecieverService;
        private Mock<IEventPayloadValidator> _mockEventPayloadValidator;
        private Mock<ILogger<EventPayloadRecieverService>> _mockLogger;
        private Mock<IMessageBrokerSubscriberService> _mockMessageBrokerSubscriberService;

        [SetUp]
        public void Setup()
        {
            _mockEventPayloadValidator = new Mock<IEventPayloadValidator>();
            _mockLogger = new Mock<ILogger<EventPayloadRecieverService>>();
            _mockMessageBrokerSubscriberService = new Mock<IMessageBrokerSubscriberService>();
            _eventPayloadRecieverService = new EventPayloadRecieverService(_mockLogger.Object, _mockEventPayloadValidator.Object, _mockMessageBrokerSubscriberService.Object);
        }

        [Test]
        public void RecieveWorkflowPayload_ValidateWorkFlowRequest()
        {
            var message = CreateMessageReceivedEventArgs("destination");
            _eventPayloadRecieverService.RecieveWorkflowPayload(message);

            _mockEventPayloadValidator.Verify(p => p.ValidateWorkflowRequest(It.IsAny<WorkflowRequestMessage>()), Times.Once());
        }

        [Test]
        public void RecieveWorkflowPayload_WorkFlowRequestIsNotValid_MessageSubscriberRejectsTheMessage()
        {
            var message = CreateMessageReceivedEventArgs("destination");


            _mockEventPayloadValidator.Setup(p => p.ValidateWorkflowRequest(It.IsAny<WorkflowRequestMessage>())).Returns(false);

            _eventPayloadRecieverService.RecieveWorkflowPayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.Reject(It.IsAny<Message>()), Times.Once());
        }

        [Test]
        public void RecieveWorkflowPayload_WorkFlowRequestIsValid_MessageSubscriberAcknowledgeTheMessage()
        {
            var message = CreateMessageReceivedEventArgs("destination");


            _mockEventPayloadValidator.Setup(p => p.ValidateWorkflowRequest(It.IsAny<WorkflowRequestMessage>())).Returns(false);

            _eventPayloadRecieverService.RecieveWorkflowPayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.Acknowledge(It.IsAny<Message>()), Times.Once());
        }

        private static MessageReceivedEventArgs CreateMessageReceivedEventArgs(string destination)
        {
            var exportRequestMessage = new ExportRequestMessage
            {
                ExportTaskId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Destination = destination,
                Files = new[] { "file1" },
                MessageId = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
            };
            var jsonMessage = new JsonMessage<ExportRequestMessage>(exportRequestMessage, MessageBrokerConfiguration.WorkflowManagerApplicationId, exportRequestMessage.CorrelationId, exportRequestMessage.DeliveryTag);

            return new MessageReceivedEventArgs(jsonMessage.ToMessage(), CancellationToken.None);
        }
    }
}
