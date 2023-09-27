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
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.TaskManager.API;

namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Repositories
{
    public sealed class AideClinicalReviewMetadataRepository : MetadataRepositoryBase
    {
        private readonly IServiceScope _scope;
        private readonly ILogger<AideClinicalReviewMetadataRepository> _logger;

        private readonly TaskDispatchEvent _taskDispatchEvent;

        public AideClinicalReviewMetadataRepository(
            IServiceScopeFactory serviceScopeFactory,
            IStorageService storageService,
            ILogger<AideClinicalReviewMetadataRepository> logger,
            TaskDispatchEvent taskDispatchEvent,
            TaskCallbackEvent taskCallbackEvent)
            : base(taskDispatchEvent, taskCallbackEvent)
        {
            Guard.Against.Null(serviceScopeFactory, nameof(serviceScopeFactory));

            _scope = serviceScopeFactory.CreateScope();

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taskDispatchEvent = taskDispatchEvent;

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

        public override Task<Dictionary<string, object>> RetrieveMetadata(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_taskDispatchEvent.Metadata);
        }
    }
}
