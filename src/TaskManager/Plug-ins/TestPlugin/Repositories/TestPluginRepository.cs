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
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.API;

namespace Monai.Deploy.WorkflowManager.TaskManager.TestPlugin.Repositories
{
    public sealed class TestPluginRepository : MetadataRepositoryBase
    {
        private readonly IServiceScope _scope;

        public TestPluginRepository(
            IServiceScopeFactory serviceScopeFactory,
            TaskDispatchEvent taskDispatchEvent,
            TaskCallbackEvent taskCallbackEvent)
            : base(taskDispatchEvent, taskCallbackEvent)
        {
            ArgumentNullException.ThrowIfNull(serviceScopeFactory, nameof(serviceScopeFactory));

            _scope = serviceScopeFactory.CreateScope();

            Validate();
        }

        private void Validate()
        {
            ArgumentNullException.ThrowIfNull(DispatchEvent, nameof(DispatchEvent));
            ArgumentNullException.ThrowIfNull(CallbackEvent, nameof(CallbackEvent));

            ArgumentNullException.ThrowIfNullOrWhiteSpace(DispatchEvent.WorkflowInstanceId, nameof(DispatchEvent.WorkflowInstanceId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(DispatchEvent.ExecutionId, nameof(DispatchEvent.ExecutionId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(DispatchEvent.PayloadId, nameof(DispatchEvent.PayloadId));
        }

        public override async Task<Dictionary<string, object>> RetrieveMetadata(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => new Dictionary<string, object>()).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
        }

        ~TestPluginRepository() => Dispose(disposing: false);

        protected override void Dispose(bool disposing)
        {
            if (!DisposedValue && disposing)
            {
                _scope.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
