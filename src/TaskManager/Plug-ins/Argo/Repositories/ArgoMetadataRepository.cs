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

using System.Text;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Logging;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.Repositories
{
    public sealed class ArgoMetadataRepository : MetadataRepositoryBase, IAsyncDisposable
    {
        private readonly IStorageService _storageService;
        private readonly IServiceScope _scope;
        private readonly ILogger<ArgoMetadataRepository> _logger;

        public ArgoMetadataRepository(
            IServiceScopeFactory serviceScopeFactory,
            IStorageService storageService,
            ILogger<ArgoMetadataRepository> logger,
            TaskDispatchEvent taskDispatchEvent,
            TaskCallbackEvent taskCallbackEvent)
            : base(taskDispatchEvent, taskCallbackEvent)
        {
            Guard.Against.Null(serviceScopeFactory, nameof(serviceScopeFactory));

            _scope = serviceScopeFactory.CreateScope();

            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
            var path = $"{DispatchEvent.PayloadId}/workflows/{DispatchEvent.WorkflowInstanceId}/{DispatchEvent.ExecutionId}/metadata.json";

            var jsonStr = await RetrieveJsonFromFile(DispatchEvent.IntermediateStorage.Bucket, path);

            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return new Dictionary<string, object>();
            }

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
        }

        private async Task<string> RetrieveJsonFromFile(string bucketName, string path)
        {
            var jsonStr = string.Empty;

            try
            {
                var stream = await _storageService.GetObjectAsync(bucketName, path);

                jsonStr = Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());

            }
            catch (Exception)
            {
                _logger.MetadataFileNotFound(bucketName, path);
            }

            return jsonStr;
        }

        ~ArgoMetadataRepository() => Dispose(disposing: false);

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
