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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Monai.Deploy.Messaging.API;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Database.Tests
{
    public class TaskExecutionStatsTests
    {
        private const string NOT_ARGO = "notArgo";
        private const string NOT_ARGO2 = "notArgo2";
        //private readonly Mock<ILogger<TaskManager>> _logger;
        //private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
        private readonly Mock<IServiceScope> _serviceScope;
        private readonly Mock<ITaskExecutionStatsRepository> _executionStatsRepo;
        //private readonly Mock<IStorageService> _storageService;
        //private readonly Mock<IStorageAdminService> _storageAdminService;
        private readonly Mock<IMessageBrokerPublisherService> _messageBrokerPublisherService;
        private readonly Mock<IMessageBrokerSubscriberService> _messageBrokerSubscriberService;
        //private readonly Mock<ITestRunnerCallback> _testRunnerCallback;
        //private readonly Mock<ITestMetadataRepositoryCallback> _testMetadataRepositoryCallback;
        //private readonly Mock<ITaskDispatchEventService> _taskDispatchEventService;
        private readonly CancellationTokenSource _cancellationTokenSource;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TaskExecutionStatsTests()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        [Fact(DisplayName = "ExecuteTask - sets Execution Stats on start")]
        public async Task ExecuteTask_SetsExecutionStatsOnStart()
        {


        }

        [Fact(DisplayName = "Task Manager - metadata used to fill in ExecutionStats")]
        public async Task TaskManager_Metadata_Used_To_Fill_In_ExecutionStats()
        {


        }

        [Fact]
        public async Task ExecuteTask_SetsExecutionStatsOnCanceled()
        {

        }

        //private static JsonMessage<TaskCallbackEvent> GenerateTaskCallbackEvent(JsonMessage<TaskDispatchEvent>? taskDispatchEventMessage = null)
        //{
        //    return new JsonMessage<TaskCallbackEvent>(
        //                    new TaskCallbackEvent
        //                    {
        //                        CorrelationId = taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.CorrelationId,
        //                        ExecutionId = taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.Body.ExecutionId,
        //                        WorkflowInstanceId = taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.Body.WorkflowInstanceId,
        //                        TaskId = taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.Body.TaskId,
        //                        Identity = Guid.NewGuid().ToString(),
        //                    },
        //                    Guid.NewGuid().ToString(),
        //                    taskDispatchEventMessage is null ? Guid.NewGuid().ToString() : taskDispatchEventMessage.CorrelationId,
        //                    "1");
        //}

    }
}
