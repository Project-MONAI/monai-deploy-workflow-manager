﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Moq;
using Monai.Deploy.WorkloadManager.PayloadListener.Validators;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Monai.Deploy.WorkloadManager.PayloadListener.Tests.Validators
{
    public class EventPayloadValidatorTests
    {
        private IEventPayloadValidator _eventPayloadValidator;
        private Mock<ILogger<EventPayloadValidator>> _mockLogger;

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
                _eventPayloadValidator.ValidateWorkflowRequest(null);
            });
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCallingAETitleIsMoreThan15Charchaters_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = "abcdefghijklmnop";
            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCallingAETitleIsNull_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = null;
            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCallingAETitleIsWhiteSpace_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = " ";
            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCallingAETitleIsEmptyString_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = String.Empty;
            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCalledAETitleIsMoreThan15Charchaters_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = "abcdefghijklmnop";
            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCalledAETitleIsNull_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = null;
            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCalledAETitleIsWhiteSpace_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = " ";
            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithCalledAETitleIsEmptyString_ReturnsValidatonFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.CalledAeTitle = " ";
            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageWithNullWorkflow_ThrowsArgumentNullException()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.Workflows = new List<string> { null };

            Assert.Throws<ArgumentNullException>(() =>
            {
                _eventPayloadValidator.ValidateWorkflowRequest(message);
            });
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageIsValid_ReturnsTrue()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.Workflows = new List<string> { "123", "234", "345", "456" };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsTrue(result);
        }

        private static WorkflowRequestMessage CreateWorkflowRequestMessageWithNoWorkFlow()
        {
            return new WorkflowRequestMessage
            {
                Bucket = "Bucket",
                PayloadId = Guid.NewGuid(),
                Workflows = new List<string>(),
                FileCount = 2,
                CorrelationId = "CorrelationId",
                Timestamp = DateTime.Now,
                CalledAeTitle = "AeTitle",
                CallingAeTitle = "CallingAeTitle",
            };
        }
    }
}
