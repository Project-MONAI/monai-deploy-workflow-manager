using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Monai.Deploy.WorkflowManager.TaskManager.Database;
using Monai.Deploy.WorkflowManager.TaskManager.Services;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.TaskManager.Tests.Services
{
    public class TaskDispatchEventServiceTests
    {
        private readonly Mock<ILogger<TaskDispatchEventService>> _logger;
        private readonly Mock<ITaskDispatchEventRepository> _taskDispatchEventRepository;

        public TaskDispatchEventServiceTests()
        {
            _logger = new Mock<ILogger<TaskDispatchEventService>>();
            _taskDispatchEventRepository = new Mock<ITaskDispatchEventRepository>();
        }

        [Fact(DisplayName = "TaskDispatchEventService - UpdateTaskPluginArgsAsync - Throws error when taskDispatchEvent is null")]
        public async void TaskDispatchEventService_UpdateTaskPluginArgsAsync_ThrowsErrorWhenTaskDispatchEventNull()
        {
            TaskDispatchEventInfo eventInfo = null;
            var pluginArgs = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

            var service = new TaskDispatchEventService(_taskDispatchEventRepository.Object, _logger.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.UpdateTaskPluginArgsAsync(eventInfo, pluginArgs));

            _taskDispatchEventRepository.Verify(x => x.UpdateTaskPluginArgsAsync(It.IsAny<TaskDispatchEventInfo>(), It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [Fact(DisplayName = "TaskDispatchEventService - UpdateTaskPluginArgsAsync - Throws error when pluginArgs is null")]
        public async void TaskDispatchEventService_UpdateTaskPluginArgsAsync_ThrowsErrorWhenPluginArgsNull()
        {
            TaskDispatchEventInfo eventInfo = GenerateTaskDispatchEventInfo();
            Dictionary<string, string> pluginArgs = null;

            var service = new TaskDispatchEventService(_taskDispatchEventRepository.Object, _logger.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.UpdateTaskPluginArgsAsync(eventInfo, pluginArgs));

            _taskDispatchEventRepository.Verify(x => x.UpdateTaskPluginArgsAsync(It.IsAny<TaskDispatchEventInfo>(), It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [Fact(DisplayName = "TaskDispatchEventService - UpdateTaskPluginArgsAsync - Successful")]
        public async void TaskDispatchEventService_UpdateTaskPluginArgsAsync_Successful()
        {
            var eventInfo = GenerateTaskDispatchEventInfo();
            var pluginArgs = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

            _taskDispatchEventRepository.Setup(x => x.UpdateTaskPluginArgsAsync(eventInfo, pluginArgs)).ReturnsAsync(new TaskDispatchEventInfo(new TaskDispatchEvent
            {
                CorrelationId = "CorrelationId",
                PayloadId = Guid.NewGuid().ToString(),
                ExecutionId = Guid.NewGuid().ToString(),
                TaskPluginType = PluginStrings.Argo,
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                TaskId = Guid.NewGuid().ToString(),
                TaskPluginArguments = pluginArgs
            }));

            var service = new TaskDispatchEventService(_taskDispatchEventRepository.Object, _logger.Object);

            var result = await service.UpdateTaskPluginArgsAsync(eventInfo, pluginArgs);

            _taskDispatchEventRepository.Verify(x => x.UpdateTaskPluginArgsAsync(It.IsAny<TaskDispatchEventInfo>(), It.IsAny<Dictionary<string, string>>()), Times.Once);
            Assert.Equal(result.Event.TaskPluginArguments, pluginArgs);
        }

        private TaskDispatchEventInfo GenerateTaskDispatchEventInfo()
        {
            return new TaskDispatchEventInfo(new TaskDispatchEvent
            {
                CorrelationId = "CorrelationId",
                PayloadId = Guid.NewGuid().ToString(),
                ExecutionId = Guid.NewGuid().ToString(),
                TaskPluginType = PluginStrings.Argo,
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                TaskId = Guid.NewGuid().ToString()
            });
        }
    }
}
