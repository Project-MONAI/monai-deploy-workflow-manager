// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Services;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Tests.Services
{
    public class EventPayloadReceiverServiceTests
    {
        private IEventPayloadReceiverService _eventPayloadReceiverService;
        private Mock<IEventPayloadValidator> _mockEventPayloadValidator;
        private Mock<ILogger<EventPayloadReceiverService>> _mockLogger;
        private Mock<IMessageBrokerSubscriberService> _mockMessageBrokerSubscriberService;
        private Mock<IWorkflowExecuterService> _workflowExecuterService;

        [SetUp]
        public void Setup()
        {
            _mockEventPayloadValidator = new Mock<IEventPayloadValidator>();
            _mockLogger = new Mock<ILogger<EventPayloadReceiverService>>();
            _mockMessageBrokerSubscriberService = new Mock<IMessageBrokerSubscriberService>();
            _workflowExecuterService = new Mock<IWorkflowExecuterService>();
            _eventPayloadReceiverService = new EventPayloadReceiverService(_mockLogger.Object, _mockEventPayloadValidator.Object, _mockMessageBrokerSubscriberService.Object, _workflowExecuterService.Object);
        }

        [Test]
        public void ReceiveWorkflowPayload_ValidateWorkFlowRequest()
        {
            var message = CreateMessageReceivedEventArgs("destination");
            _eventPayloadReceiverService.ReceiveWorkflowPayload(message);

            _mockEventPayloadValidator.Verify(p => p.ValidateWorkflowRequest(It.IsAny<WorkflowRequestEvent>()), Times.Once());
        }

        [Test]
        public void ReceiveWorkflowPayload_WorkFlowRequestIsNotValid_MessageSubscriberRejectsTheMessage()
        {
            var message = CreateMessageReceivedEventArgs("destination");


            _mockEventPayloadValidator.Setup(p => p.ValidateWorkflowRequest(It.IsAny<WorkflowRequestEvent>())).Returns(false);

            _eventPayloadReceiverService.ReceiveWorkflowPayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.Reject(It.IsAny<Message>(), false), Times.Once());
        }

        [Test]
        public void ReceiveWorkflowPayload_WorkFlowRequestIsValid_MessageSubscriberAcknowledgeTheMessage()
        {
            var message = CreateMessageReceivedEventArgs("destination");


            _mockEventPayloadValidator.Setup(p => p.ValidateWorkflowRequest(It.IsAny<WorkflowRequestEvent>())).Returns(true);

            _workflowExecuterService.Setup(p => p.ProcessPayload(It.IsAny<WorkflowRequestEvent>())).ReturnsAsync(true);

            _eventPayloadReceiverService.ReceiveWorkflowPayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.Acknowledge(It.IsAny<Message>()), Times.Once());
        }

        [Test]
        public void TaskUpdatePayload_ValidateTaskUpdate()
        {
            var message = CreateMessageReceivedEventArgs("destination");

            _eventPayloadReceiverService.TaskUpdatePayload(message);

            _mockEventPayloadValidator.Verify(p => p.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>()), Times.Once());
            _mockEventPayloadValidator.VerifyNoOtherCalls();
        }

        [Test]
        public void TaskUpdatePayload_TaskUpdateIsNotValid_MessageSubscriberRejectsTheMessage()
        {
            var message = CreateMessageReceivedEventArgs("destination");

            _mockEventPayloadValidator.Setup(p => p.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>())).Returns(false);

            _eventPayloadReceiverService.TaskUpdatePayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.Reject(It.IsAny<Message>(), false), Times.Once());
            _mockMessageBrokerSubscriberService.VerifyNoOtherCalls();

            _mockEventPayloadValidator.Verify(x => x.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>()), Times.Once);
            _mockEventPayloadValidator.VerifyNoOtherCalls();
        }

        [Test]
        public void TaskUpdatePayload_TaskUpdateIsValid_MessageSubscriberAcknowledgeTheMessage()
        {
            var message = CreateMessageReceivedEventArgs("destination");

            _mockEventPayloadValidator.Setup(p => p.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>())).Returns(true);
            _workflowExecuterService.Setup(p => p.ProcessTaskUpdate(It.IsAny<TaskUpdateEvent>())).ReturnsAsync(true);

            _eventPayloadReceiverService.TaskUpdatePayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.Acknowledge(It.IsAny<Message>()), Times.Once());
            _mockMessageBrokerSubscriberService.VerifyNoOtherCalls();

            _mockEventPayloadValidator.Verify(x => x.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>()), Times.Once);
            _mockEventPayloadValidator.VerifyNoOtherCalls();
        }

        [Test]
        public void TaskUpdatePayload_FailsToProcessUpdateTask_MessageIsRejectedAndRequeued()
        {
            // Arrange
            var message = CreateMessageReceivedEventArgs("destination");

            _mockEventPayloadValidator.Setup(p => p.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>())).Returns(true);
            _workflowExecuterService.Setup(p => p.ProcessTaskUpdate(It.IsAny<TaskUpdateEvent>())).ReturnsAsync(false);

            // Act
            _eventPayloadReceiverService.TaskUpdatePayload(message);

            // Assert
            _mockEventPayloadValidator.Verify(x => x.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>()), Times.Once);
            _mockEventPayloadValidator.VerifyNoOtherCalls();

            _mockMessageBrokerSubscriberService.Verify(p => p.Reject(It.IsAny<Message>(), It.IsAny<bool>()), Times.Once());
            _mockMessageBrokerSubscriberService.VerifyNoOtherCalls();
        }

        [Test]
        public void TaskUpdatePayload_ErrorIsThrown_MessageIsRejectedAndRequeued()
        {
            // Arrange
            var message = CreateMessageReceivedEventArgs("destination");

            _mockEventPayloadValidator.Setup(p => p.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>())).Returns(true);
            _workflowExecuterService.Setup(p => p.ProcessTaskUpdate(It.IsAny<TaskUpdateEvent>())).Throws<Exception>();

            // Act
            _eventPayloadReceiverService.TaskUpdatePayload(message);

            // Assert
            _mockEventPayloadValidator.Verify(x => x.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>()), Times.Once);
            _mockEventPayloadValidator.VerifyNoOtherCalls();

            _mockMessageBrokerSubscriberService.Verify(p => p.Reject(It.IsAny<Message>(), It.IsAny<bool>()), Times.Once());
            _mockMessageBrokerSubscriberService.VerifyNoOtherCalls();
        }

        private static MessageReceivedEventArgs CreateMessageReceivedEventArgs(string destination)
        {
            var exportRequestMessage = new ExportRequestEvent
            {
                ExportTaskId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Destination = destination,
                Files = new[] { "file1" },
                MessageId = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
            };
            var jsonMessage = new JsonMessage<ExportRequestEvent>(exportRequestMessage, MessageBrokerConfiguration.WorkflowManagerApplicationId, exportRequestMessage.CorrelationId, exportRequestMessage.DeliveryTag);

            return new MessageReceivedEventArgs(jsonMessage.ToMessage(), CancellationToken.None);
        }
    }
}
