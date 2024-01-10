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
    public sealed class ArgoMetadataRepository : MetadataRepositoryBase
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
            ArgumentNullException.ThrowIfNull(serviceScopeFactory, nameof(serviceScopeFactory));

            _scope = serviceScopeFactory.CreateScope();

            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
            var outputDir = $"{DispatchEvent.PayloadId}/workflows/{DispatchEvent.WorkflowInstanceId}/{DispatchEvent.ExecutionId}";

            var jsonStr = await RetrieveJsonFromFile(DispatchEvent.IntermediateStorage.Bucket, outputDir);

            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return new Dictionary<string, object>();
            }

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr) ?? new Dictionary<string, object>();
        }

        private async Task<string> RetrieveJsonFromFile(string bucketName, string outputDir)
        {
            var jsonStr = string.Empty;

            try
            {
                var filesList = await _storageService.ListObjectsAsync(bucketName, outputDir, true);

                var metadataFile = filesList?.FirstOrDefault((file) => file.Filename.ToLower() == "metadata.json");

                if (metadataFile is null)
                {
                    throw new FileNotFoundException("File could not be found");
                }

                var stream = await _storageService.GetObjectAsync(bucketName, metadataFile.FilePath);

                jsonStr = Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());
            }
            catch (Exception)
            {
                _logger.MetadataFileNotFound(bucketName, outputDir);
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
    }
}
