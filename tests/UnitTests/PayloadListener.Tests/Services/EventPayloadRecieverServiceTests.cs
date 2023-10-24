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
using System.Threading;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Services;
using Monai.Deploy.WorkflowManager.PayloadListener.Services;
using Monai.Deploy.WorkflowManager.PayloadListener.Validators;
using Moq;
using NUnit.Framework;

namespace Monai.Deploy.WorkflowManager.Common.PayloadListener.Tests.Services
{
    public class EventPayloadReceiverServiceTests
    {
        private IEventPayloadReceiverService? _eventPayloadReceiverService;
        private Mock<IEventPayloadValidator>? _mockEventPayloadValidator;
        private Mock<ILogger<EventPayloadReceiverService>>? _mockLogger;
        private Mock<IMessageBrokerSubscriberService>? _mockMessageBrokerSubscriberService;
        private Mock<IWorkflowExecuterService>? _workflowExecuterService;
        private Mock<IPayloadService>? _payloadService;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.

        [SetUp]
        public void Setup()
        {
            _mockEventPayloadValidator = new Mock<IEventPayloadValidator>();
            _mockLogger = new Mock<ILogger<EventPayloadReceiverService>>();
            _mockMessageBrokerSubscriberService = new Mock<IMessageBrokerSubscriberService>();
            _workflowExecuterService = new Mock<IWorkflowExecuterService>();
            _payloadService = new Mock<IPayloadService>();
            _eventPayloadReceiverService = new EventPayloadReceiverService(_mockLogger.Object, _mockEventPayloadValidator.Object, _mockMessageBrokerSubscriberService.Object, _workflowExecuterService.Object, _payloadService.Object);
        }

        [Test]
        public void ReceiveWorkflowPayload_ValidateWorkFlowRequest()
        {
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });
            _eventPayloadReceiverService.ReceiveWorkflowPayload(message);

            _mockEventPayloadValidator.Verify(p => p.ValidateWorkflowRequest(It.IsAny<WorkflowRequestEvent>()), Times.Once());
        }

        [Test]
        public void ReceiveWorkflowPayload_WorkFlowRequestIsNotValid_MessageSubscriberRejectsTheMessage()
        {
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _mockEventPayloadValidator.Setup(p => p.ValidateWorkflowRequest(It.IsAny<WorkflowRequestEvent>())).Returns(false);

            _eventPayloadReceiverService.ReceiveWorkflowPayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.Reject(It.IsAny<Message>(), false), Times.Once());
        }

        [Test]
        public void ReceiveWorkflowPayload_CreatePayloadFails_MessageSubscriberRejectsTheMessage()
        {
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _mockEventPayloadValidator.Setup(p => p.ValidateWorkflowRequest(It.IsAny<WorkflowRequestEvent>())).Returns(true);

            _workflowExecuterService.Setup(p => p.ProcessPayload(It.IsAny<WorkflowRequestEvent>(), It.IsAny<Payload>())).ReturnsAsync(true);

            _payloadService.Setup(p => p.CreateAsync(It.IsAny<WorkflowRequestEvent>())).ReturnsAsync(() => null);

            _eventPayloadReceiverService.ReceiveWorkflowPayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.RequeueWithDelay(It.IsAny<Message>()), Times.Once());
        }

        [Test]
        public void ReceiveWorkflowPayload_WorkFlowRequestIsValid_MessageSubscriberAcknowledgeTheMessage()
        {
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _mockEventPayloadValidator.Setup(p => p.ValidateWorkflowRequest(It.IsAny<WorkflowRequestEvent>())).Returns(true);

            _workflowExecuterService.Setup(p => p.ProcessPayload(It.IsAny<WorkflowRequestEvent>(), It.IsAny<Payload>())).ReturnsAsync(true);

            _payloadService.Setup(p => p.CreateAsync(It.IsAny<WorkflowRequestEvent>())).ReturnsAsync(new Payload() { Id = Guid.NewGuid().ToString() });

            _eventPayloadReceiverService.ReceiveWorkflowPayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.Acknowledge(It.IsAny<Message>()), Times.Once());
        }

        [Test]
        public void TaskUpdatePayload_ValidateTaskUpdate()
        {
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _eventPayloadReceiverService.TaskUpdatePayload(message);

            _mockEventPayloadValidator.Verify(p => p.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>()), Times.Once());
            _mockEventPayloadValidator.VerifyNoOtherCalls();
        }

        [Test]
        public void TaskUpdatePayload_TaskUpdateIsNotValid_MessageSubscriberRejectsTheMessage()
        {
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

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
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

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
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _mockEventPayloadValidator.Setup(p => p.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>())).Returns(true);
            _workflowExecuterService.Setup(p => p.ProcessTaskUpdate(It.IsAny<TaskUpdateEvent>())).ReturnsAsync(false);

            // Act
            _eventPayloadReceiverService.TaskUpdatePayload(message);

            // Assert
            _mockEventPayloadValidator.Verify(x => x.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>()), Times.Once);
            _mockEventPayloadValidator.VerifyNoOtherCalls();

            _mockMessageBrokerSubscriberService.Verify(p => p.RequeueWithDelay(It.IsAny<Message>()), Times.Once());
            _mockMessageBrokerSubscriberService.VerifyNoOtherCalls();
        }

        [Test]
        public void TaskUpdatePayload_ErrorIsThrown_MessageIsRejectedAndRequeued()
        {
            // Arrange
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _mockEventPayloadValidator.Setup(p => p.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>())).Returns(true);
            _workflowExecuterService.Setup(p => p.ProcessTaskUpdate(It.IsAny<TaskUpdateEvent>())).Throws<Exception>();

            // Act
            _eventPayloadReceiverService.TaskUpdatePayload(message);

            // Assert
            _mockEventPayloadValidator.Verify(x => x.ValidateTaskUpdate(It.IsAny<TaskUpdateEvent>()), Times.Once);
            _mockEventPayloadValidator.VerifyNoOtherCalls();

            _mockMessageBrokerSubscriberService.Verify(p => p.RequeueWithDelay(It.IsAny<Message>()), Times.Once());
            _mockMessageBrokerSubscriberService.VerifyNoOtherCalls();
        }

        [Test]
        public void ExportCompletePayload_ValidateExportComplete()
        {
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _eventPayloadReceiverService.ExportCompletePayload(message);

            _mockEventPayloadValidator.Verify(p => p.ValidateExportComplete(It.IsAny<ExportCompleteEvent>()), Times.Once());
            _mockEventPayloadValidator.VerifyNoOtherCalls();
        }

        [Test]
        public void ExportCompletePayload_ExportCompleteIsNotValid_MessageSubscriberRejectsTheMessage()
        {
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _mockEventPayloadValidator.Setup(p => p.ValidateExportComplete(It.IsAny<ExportCompleteEvent>())).Returns(false);

            _eventPayloadReceiverService.ExportCompletePayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.Reject(It.IsAny<Message>(), false), Times.Once());
            _mockMessageBrokerSubscriberService.VerifyNoOtherCalls();

            _mockEventPayloadValidator.Verify(x => x.ValidateExportComplete(It.IsAny<ExportCompleteEvent>()), Times.Once);
            _mockEventPayloadValidator.VerifyNoOtherCalls();
        }

        [Test]
        public void ExportCompletePayload_ExportCompleteIsValid_MessageSubscriberAcknowledgeTheMessage()
        {
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _mockEventPayloadValidator.Setup(p => p.ValidateExportComplete(It.IsAny<ExportCompleteEvent>())).Returns(true);
            _workflowExecuterService.Setup(p => p.ProcessExportComplete(It.IsAny<ExportCompleteEvent>(), It.IsAny<string>())).ReturnsAsync(true);

            _eventPayloadReceiverService.ExportCompletePayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.Acknowledge(It.IsAny<Message>()), Times.Once());
            _mockMessageBrokerSubscriberService.VerifyNoOtherCalls();

            _mockEventPayloadValidator.Verify(x => x.ValidateExportComplete(It.IsAny<ExportCompleteEvent>()), Times.Once);
            _mockEventPayloadValidator.VerifyNoOtherCalls();
        }

        [Test]
        public void ExportCompletePayload_FailsToProcessExportComplete_MessageIsRejectedAndRequeued()
        {
            // Arrange
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _mockEventPayloadValidator.Setup(p => p.ValidateExportComplete(It.IsAny<ExportCompleteEvent>())).Returns(true);
            _workflowExecuterService.Setup(p => p.ProcessExportComplete(It.IsAny<ExportCompleteEvent>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act
            _eventPayloadReceiverService.ExportCompletePayload(message);

            // Assert
            _mockEventPayloadValidator.Verify(x => x.ValidateExportComplete(It.IsAny<ExportCompleteEvent>()), Times.Once);
            _mockEventPayloadValidator.VerifyNoOtherCalls();

            _mockMessageBrokerSubscriberService.Verify(p => p.RequeueWithDelay(It.IsAny<Message>()), Times.Once());
            _mockMessageBrokerSubscriberService.VerifyNoOtherCalls();
        }

        [Test]
        public void ExportCompletePayload_ErrorIsThrown_MessageIsRejectedAndRequeued()
        {
            // Arrange
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _mockEventPayloadValidator.Setup(p => p.ValidateExportComplete(It.IsAny<ExportCompleteEvent>())).Returns(true);
            _workflowExecuterService.Setup(p => p.ProcessExportComplete(It.IsAny<ExportCompleteEvent>(), It.IsAny<string>())).Throws<Exception>();

            // Act
            _eventPayloadReceiverService.ExportCompletePayload(message);

            // Assert
            _mockEventPayloadValidator.Verify(x => x.ValidateExportComplete(It.IsAny<ExportCompleteEvent>()), Times.Once);
            _mockEventPayloadValidator.VerifyNoOtherCalls();

            _mockMessageBrokerSubscriberService.Verify(p => p.RequeueWithDelay(It.IsAny<Message>()), Times.Once());
            _mockMessageBrokerSubscriberService.VerifyNoOtherCalls();
        }

        [Test]
        public void ReceiveWorkflowPayload_With_WorkflowId_And_TaskID()
        {

            var exportRequestMessage = new WorkflowRequestEvent
            {
                CorrelationId = Guid.NewGuid().ToString(),
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                TaskId = "exporttask"
            };
            var jsonMessage = new JsonMessage<WorkflowRequestEvent>(exportRequestMessage, MessageBrokerConfiguration.WorkflowManagerApplicationId, exportRequestMessage.CorrelationId);
            var message = new MessageReceivedEventArgs(jsonMessage.ToMessage(), CancellationToken.None);

            _mockEventPayloadValidator.Setup(p => p.ValidateWorkflowRequest(It.IsAny<WorkflowRequestEvent>())).Returns(true);

            _workflowExecuterService.Setup(p => p.ProcessPayload(It.IsAny<WorkflowRequestEvent>(), It.IsAny<Payload>())).ReturnsAsync(true);

            _eventPayloadReceiverService.ReceiveWorkflowPayload(message);

            _payloadService.Verify(p => p.CreateAsync(It.IsAny<WorkflowRequestEvent>()), Times.Never());
        }


        [Test]
        public void ArtifactReceivedPayload_ValidateWorkFlowRequest()
        {
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });
            _eventPayloadReceiverService.ArtifactReceivePayload(message);

            _mockEventPayloadValidator.Verify(p => p.ValidateArtifactReceived(It.IsAny<ArtifactsReceivedEvent>()), Times.Once());
        }


        [Test]
        public void ArtifactReceivedPayload_WorkFlowRequestIsNotValid_MessageSubscriberRejectsTheMessage()
        {
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _mockEventPayloadValidator.Setup(p => p.ValidateArtifactReceived(It.IsAny<ArtifactsReceivedEvent>())).Returns(false);

            _eventPayloadReceiverService.ArtifactReceivePayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.Reject(It.IsAny<Message>(), false), Times.Once());
        }

        [Test]
        public void ArtifactReceivedPayload_WorkFlowRequestIsValid_MessageSubscriberAcknowledgeTheMessage()
        {
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _mockEventPayloadValidator.Setup(p => p.ValidateArtifactReceived(It.IsAny<ArtifactsReceivedEvent>())).Returns(true);

            _workflowExecuterService.Setup(p => p.ProcessArtifactReceivedAsync(It.IsAny<ArtifactsReceivedEvent>())).ReturnsAsync(true);

            _eventPayloadReceiverService.ArtifactReceivePayload(message);

            _mockMessageBrokerSubscriberService.Verify(p => p.Acknowledge(It.IsAny<Message>()), Times.Once());
        }

        [Test]
        public void ArtifactReceivedPayload_FailsToProcessArtifactReceived_MessageIsRejectedAndRequeued()
        {
            // Arrange
            var message = CreateMessageReceivedEventArgs(new string[] { "destination" });

            _mockEventPayloadValidator.Setup(p => p.ValidateArtifactReceived(It.IsAny<ArtifactsReceivedEvent>())).Returns(true);
            _workflowExecuterService.Setup(p => p.ProcessArtifactReceivedAsync(It.IsAny<ArtifactsReceivedEvent>())).ReturnsAsync(false);

            // Act
            _eventPayloadReceiverService.ArtifactReceivePayload(message);

            // Assert
            _mockEventPayloadValidator.Verify(x => x.ValidateArtifactReceived(It.IsAny<ArtifactsReceivedEvent>()), Times.Once);
            _mockEventPayloadValidator.VerifyNoOtherCalls();

            _mockMessageBrokerSubscriberService.Verify(p => p.RequeueWithDelay(It.IsAny<Message>()), Times.Once());
            _mockMessageBrokerSubscriberService.VerifyNoOtherCalls();
        }

        private static MessageReceivedEventArgs CreateMessageReceivedEventArgs(string[] destinations)
        {
            var exportRequestMessage = new ExportRequestEvent
            {
                ExportTaskId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Destinations = destinations,
                Files = new[] { "file1" },
                MessageId = Guid.NewGuid().ToString(),
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                PayloadId = Guid.NewGuid().ToString()
            };
            var jsonMessage = new JsonMessage<ExportRequestEvent>(exportRequestMessage, MessageBrokerConfiguration.WorkflowManagerApplicationId, exportRequestMessage.CorrelationId, exportRequestMessage.DeliveryTag);

            return new MessageReceivedEventArgs(jsonMessage.ToMessage(), CancellationToken.None);
        }
    }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
}
