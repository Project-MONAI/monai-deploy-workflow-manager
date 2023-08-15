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
using System.Threading.Tasks;
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        public TaskDispatchEventServiceTests()
        {
            _logger = new Mock<ILogger<TaskDispatchEventService>>();
            _taskDispatchEventRepository = new Mock<ITaskDispatchEventRepository>();
        }

        [Fact(DisplayName = "TaskDispatchEventService - UpdateTaskPluginArgsAsync - Throws error when taskDispatchEvent is null")]
        public async Task TaskDispatchEventService_UpdateTaskPluginArgsAsync_ThrowsErrorWhenTaskDispatchEventNull()
        {
            TaskDispatchEventInfo eventInfo = null;
            var pluginArgs = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

            var service = new TaskDispatchEventService(_taskDispatchEventRepository.Object, _logger.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.UpdateTaskPluginArgsAsync(eventInfo, pluginArgs));

            _taskDispatchEventRepository.Verify(x => x.UpdateTaskPluginArgsAsync(It.IsAny<TaskDispatchEventInfo>(), It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [Fact(DisplayName = "TaskDispatchEventService - UpdateTaskPluginArgsAsync - Throws error when pluginArgs is null")]
        public async Task TaskDispatchEventService_UpdateTaskPluginArgsAsync_ThrowsErrorWhenPluginArgsNull()
        {
            TaskDispatchEventInfo eventInfo = GenerateTaskDispatchEventInfo();
            Dictionary<string, string> pluginArgs = null;

            var service = new TaskDispatchEventService(_taskDispatchEventRepository.Object, _logger.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.UpdateTaskPluginArgsAsync(eventInfo, pluginArgs));

            _taskDispatchEventRepository.Verify(x => x.UpdateTaskPluginArgsAsync(It.IsAny<TaskDispatchEventInfo>(), It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [Fact(DisplayName = "TaskDispatchEventService - UpdateTaskPluginArgsAsync - Successful")]
        public async Task TaskDispatchEventService_UpdateTaskPluginArgsAsync_Successful()
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
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
