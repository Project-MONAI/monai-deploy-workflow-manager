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
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.PayloadListener.Validators;
using Moq;
using NUnit.Framework;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Tests.Validators
{
    public class EventPayloadValidatorTests
    {
        private IEventPayloadValidator? _eventPayloadValidator;
        private Mock<ILogger<EventPayloadValidator>>? _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<EventPayloadValidator>>();
            _eventPayloadValidator = new EventPayloadValidator(_mockLogger.Object);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessage_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _eventPayloadValidator!.ValidateWorkflowRequest(null);
            });
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCallingAETitleIsMoreThan15Charchaters_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = "abcdefghijklmnop";
            var result = _eventPayloadValidator!.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCallingAETitleIsNull_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = string.Empty;
            var result = _eventPayloadValidator!.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCallingAETitleIsWhiteSpace_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = " ";
            var result = _eventPayloadValidator!.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCallingAETitleIsEmptyString_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = String.Empty;
            var result = _eventPayloadValidator!.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCalledAETitleIsMoreThan15Charchaters_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = "abcdefghijklmnop";
            var result = _eventPayloadValidator!.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCalledAETitleIsNull_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = string.Empty;
            var result = _eventPayloadValidator!.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCalledAETitleIsWhiteSpace_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = " ";
            var result = _eventPayloadValidator!.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCalledAETitleIsEmptyString_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = " ";
            var result = _eventPayloadValidator!.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithNullWorkflow_ThrowsArgumentNullException()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.Workflows = new List<string> { "" };

            var result = _eventPayloadValidator!.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageIsValid_ReturnsTrue()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.Workflows = new List<string> { "123", "234", "345", "456" };

            var result = _eventPayloadValidator!.ValidateWorkflowRequest(message);

            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateTaskUpdate_TaskUpdateEventIsValid_ReturnsTrue()
        {
            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                TaskId = Guid.NewGuid().ToString(),
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Succeeded,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = new Dictionary<string, object>(),
                CorrelationId = Guid.NewGuid().ToString()
            };

            var result = _eventPayloadValidator!.ValidateTaskUpdate(updateEvent);

            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateTaskUpdate_TaskUpdateEventIsInvalid_ReturnsFalse()
        {
            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = "   ",
                TaskId = Guid.NewGuid().ToString(),
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Succeeded,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = new Dictionary<string, object>(),
                CorrelationId = Guid.NewGuid().ToString()
            };

            var result = _eventPayloadValidator!.ValidateTaskUpdate(updateEvent);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateTaskUpdate_TaskUpdateEventIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _eventPayloadValidator!.ValidateTaskUpdate(null);
            });
        }

        [Test]
        public void ValidateExportComplete_ExportCompleteEventIsValid_ReturnsTrue()
        {
            var exportEvent = new ExportCompleteEvent
            {
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                ExportTaskId = Guid.NewGuid().ToString(),
                Status = ExportStatus.Success,
                Message = "This is a message"
            };

            var result = _eventPayloadValidator!.ValidateExportComplete(exportEvent);

            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateExportComplete_ExportCompleteEventIsInvalid_ReturnsFalse()
        {
            var exportEvent = new ExportCompleteEvent
            {
                WorkflowInstanceId = "     ",
                ExportTaskId = Guid.NewGuid().ToString(),
                Status = ExportStatus.Success,
                Message = "This is a message"
            };

            var result = _eventPayloadValidator!.ValidateExportComplete(exportEvent);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateExportComplete_ExportCompleteEventIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _eventPayloadValidator!.ValidateExportComplete(null);
            });
        }

        private static WorkflowRequestEvent CreateWorkflowRequestMessageWithNoWorkFlow()
        {
            return new WorkflowRequestEvent
            {
                Bucket = "Bucket",
                PayloadId = Guid.NewGuid(),
                Workflows = new List<string>(),
                FileCount = 2,
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                CalledAeTitle = "AeTitle",
                CallingAeTitle = "CallingAeTitle",
            };
        }
    }
}
