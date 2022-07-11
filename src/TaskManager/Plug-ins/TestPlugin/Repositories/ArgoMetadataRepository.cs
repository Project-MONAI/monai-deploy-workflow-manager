// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.API;

namespace Monai.Deploy.WorkflowManager.TaskManager.TestPlugin.Repositories
{
    public sealed class TestPluginRepository : MetadataRepositoryBase, IAsyncDisposable
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
            Guard.Against.Null(DispatchEvent);
            Guard.Against.Null(CallbackEvent);

            Guard.Against.NullOrWhiteSpace(DispatchEvent.WorkflowInstanceId);
            Guard.Against.NullOrWhiteSpace(DispatchEvent.ExecutionId);
            Guard.Against.NullOrWhiteSpace(DispatchEvent.PayloadId);
        }

        public override async Task<Dictionary<string, object>> RetrieveMetadata(CancellationToken cancellationToken = default)
        {
            return new Dictionary<string, object>();
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

        public async ValueTask DisposeAsync()
        {
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }
    }
}
