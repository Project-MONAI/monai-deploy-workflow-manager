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

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.API;

namespace Monai.Deploy.WorkflowManager.TaskManager.TestPlugin
{
    public class TestPlugin : TaskPluginBase, IAsyncDisposable
    {
        private const string TaskManagerApplicationId = "4c9072a1-35f5-4d85-847d-dafca22244a8";
        private readonly IServiceScope _scope;
        private readonly ILogger<TestPlugin> _logger;
        private readonly IMessageBrokerPublisherService? _messageBrokerPublisherService;

        private string _executeTaskStatus = String.Empty;
        private string _getStatusStatus = String.Empty;

        public TestPlugin(
            IServiceScopeFactory serviceScopeFactory,
            IMessageBrokerPublisherService messageBrokerPublisherService,
            ILogger<TestPlugin> logger,
            TaskDispatchEvent taskDispatchEvent)
            : base(taskDispatchEvent)
        {
            Guard.Against.Null(serviceScopeFactory, nameof(serviceScopeFactory));

            _scope = serviceScopeFactory.CreateScope();

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageBrokerPublisherService = messageBrokerPublisherService ?? throw new ArgumentNullException(nameof(messageBrokerPublisherService));

            ValidateEventAndInit();
            Initialize();
        }

        private void Initialize()
        {
            if (Event.TaskPluginArguments.ContainsKey(Keys.ExecuteTaskStatus))
            {
                _executeTaskStatus = Event.TaskPluginArguments[Keys.ExecuteTaskStatus];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.GetStatusStatus))
            {
                _getStatusStatus = Event.TaskPluginArguments[Keys.GetStatusStatus];
            }
        }

        private void ValidateEventAndInit()
        {
            if (Event.TaskPluginArguments is null || Event.TaskPluginArguments.Count == 0)
            {
                throw new InvalidTaskException($"Required parameters for test plugin are missing: {string.Join(',', Keys.RequiredParameters)}");
            }

            foreach (var key in Keys.RequiredParameters)
            {
                if (!Event.TaskPluginArguments.ContainsKey(key))
                {
                    throw new InvalidTaskException($"Required parameters for test plugin are missing: {key}");
                }
            }
        }

        public override async Task<ExecutionStatus> ExecuteTask(CancellationToken cancellationToken = default)
        {
            if (_executeTaskStatus.ToLower() == "succeeded")
            {
                return new ExecutionStatus { Status = TaskExecutionStatus.Succeeded, FailureReason = FailureReason.None };
            }
            else if (_executeTaskStatus.ToLower() == "failed")
            {
                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError };
            }
            else if (_executeTaskStatus.ToLower() == "cancelled")
            {
                return new ExecutionStatus { Status = TaskExecutionStatus.Canceled, FailureReason = FailureReason.None };
            }

            return new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None };
        }

        public override async Task<ExecutionStatus> GetStatus(string identity, CancellationToken cancellationToken = default)
        {
            if (_getStatusStatus.ToLower() == "accepted")
            {
                return new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None };
            }
            else if (_getStatusStatus.ToLower() == "failed")
            {
                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError };
            }
            else if (_getStatusStatus.ToLower() == "cancelled")
            {
                return new ExecutionStatus { Status = TaskExecutionStatus.Canceled, FailureReason = FailureReason.None };
            }

            return new ExecutionStatus { Status = TaskExecutionStatus.Succeeded, FailureReason = FailureReason.None };
        }
        ~TestPlugin() => Dispose(disposing: false);

        protected override void Dispose(bool disposing)
        {
            if (!DisposedValue && disposing)
            {
                _scope.Dispose();
            }

            base.Dispose(disposing);
        }

        public async ValueTask DisposeAsync()
        {
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }
    }
}
