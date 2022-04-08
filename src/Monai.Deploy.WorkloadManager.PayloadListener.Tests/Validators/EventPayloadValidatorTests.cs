using System;
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

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowNameIsNull_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString());
            workFlow.Name = null;
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowNameIsWhiteSpace_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString());
            workFlow.Name = " ";
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowNameIsMoreThan15Char_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString());
            workFlow.Name = "abcdefghijklmnop";
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowDescriptionIsNull_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString());
            workFlow.Description = null;
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowDescriptionIsWhiteSpace_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString());
            workFlow.Description = " ";
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowDescriptionIsMoreThan200Char_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString());
            workFlow.Description = "absgstegstegstegstegstegstegsteabsgstegstegstegstegstegstegsteabsgstegstegstegstegstegstegsteabsgstegstegste" +
                "                   gstegstegstegsteabsgstegstegstegstegstegstegsteabsgstegstegstegstegstegstegsteabsgstegsteglka";
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageInformaticsGatewayIsNull_ThrowsArgumentNullException()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();
            message.Workflows = new List<string> { CreateWorkFlowString(false) };
            Assert.Throws<ArgumentNullException>(() =>
            {
                _eventPayloadValidator.ValidateWorkflowRequest(message);
            });
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowIGAeiTitlleIsNull_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString(true, null));
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowIGAeiTitleIsWhiteSpace_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString(true, " "));
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowIGAeiTitleIsMoreThan15Char_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString(true, "abcdefghijklmnop"));
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowIGDataOriginIsNull_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString(true, "abcdefghijklmnop"));
            workFlow.InformaticsGateway.DataOrigins = null;
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowIGDataOriginLengthIsZero_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            message.Workflows = new List<string>() { CreateWorkFlowString() };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowIGExportDestinationsIsNull_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString(true, "abcdefghijklmnop"));
            workFlow.InformaticsGateway.ExportDestinations = null;
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowIGExportDestinationsIsZero_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            message.Workflows = new List<string>() { CreateWorkFlowString() };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowTasksIsNull_ThrowsArgumentNullException()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString(true, "abcdefghijklmnop"));
            workFlow.Tasks = null;
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            Assert.Throws<ArgumentNullException>(() =>
            {
                _eventPayloadValidator.ValidateWorkflowRequest(message);
            });
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowTasksIsZero_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            message.Workflows = new List<string>() { CreateWorkFlowString() };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowTasksIdIsNull_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString(true, "aeiTitle", true));
            workFlow.Tasks[0].Id = null;
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowTasksIdIsWhiteSpace_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString(true, "aeiTitle", true));
            workFlow.Tasks[0].Id = " ";
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowTasksIdLengthIsMoreThan15Char_ReturnsFalse()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString(true, "aeiTitle", true));
            workFlow.Tasks[0].Id = "abcdefghijklmnop";
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

            var result = _eventPayloadValidator.ValidateWorkflowRequest(message);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateWorkflowRequest_WorkflowRequestMessageIsValid_ReturnsTrue3()
        {
            var message = CreateWorkflowRequestMessageWithNoWorkFlow();

            message.Workflows = new List<string>() { CreateWorkFlowString(true, "aeiTitle", true) };

            var workFlow = JsonConvert.DeserializeObject<Workflow>(CreateWorkFlowString(true, "aeiTitle", true));
            workFlow.InformaticsGateway.DataOrigins = new string[] { "dataOrigin" };
            workFlow.InformaticsGateway.ExportDestinations = new string[] { "exportDestinations" };
            message.Workflows = new List<string>() { JsonConvert.SerializeObject(workFlow) };

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

        private static string CreateWorkFlowString(bool addInformaticsGateway = true, string aeiTitle = "aeiTitle", bool addTask = false)
        {
            var workFlow = new Workflow
            {
                Name = "workFlow",
                Version = "1",
                Description = "This is desc",
                InformaticsGateway = addInformaticsGateway ? GetInformaticsGateway(aeiTitle) : null,
                Tasks = addTask ? new TaskObject[] { AddTaskToWorkflow() } : new TaskObject[0]
            };
            return JsonConvert.SerializeObject(workFlow);
        }
        private static Contracts.Models.InformaticsGateway GetInformaticsGateway(string aeiTitle)
        {
            return new Contracts.Models.InformaticsGateway()
            {
                AeTitle = aeiTitle,
                DataOrigins = new string[0],
                ExportDestinations = new string[0],
            };
        }

        private static TaskObject AddTaskToWorkflow()
        {

            return new TaskObject
            {
                Id = "11",
                Description = "Description",
                Type = "ss",
                Args = new object{ },
                TaskDestinations = new TaskDestination[0],
                ExportDestinations = new TaskDestination[0],
                Artifacts = new ArtifactMap()
            };

        }
    }
}
