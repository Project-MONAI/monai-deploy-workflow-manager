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
using Monai.Deploy.TaskManager.API;

namespace Monai.Deploy.TaskManager.TestPlugin.Repositories
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
            Guard.Against.Null(serviceScopeFactory, nameof(serviceScopeFactory));

            _scope = serviceScopeFactory.CreateScope();

            Validate();
        }

        private void Validate()
        {
            Guard.Against.Null(DispatchEvent, nameof(DispatchEvent));
            Guard.Against.Null(CallbackEvent, nameof(CallbackEvent));

            Guard.Against.NullOrWhiteSpace(DispatchEvent.WorkflowInstanceId, nameof(DispatchEvent.WorkflowInstanceId));
            Guard.Against.NullOrWhiteSpace(DispatchEvent.ExecutionId, nameof(DispatchEvent.ExecutionId));
            Guard.Against.NullOrWhiteSpace(DispatchEvent.PayloadId, nameof(DispatchEvent.PayloadId));
        }

        public override async Task<Dictionary<string, object>> RetrieveMetadata(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => new Dictionary<string, object>()).ConfigureAwait(false);
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
