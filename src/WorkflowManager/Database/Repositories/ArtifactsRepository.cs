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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Database.Options;
using MongoDB.Driver;
using Artifact = Monai.Deploy.Messaging.Common.Artifact;

namespace Monai.Deploy.WorkflowManager.Common.Database.Repositories
{
    public class ArtifactReceivedDetails : Artifact
    {
        /// <summary>
        /// Gets or Sets LastReceived.
        /// </summary>
        public DateTime? Received { get; set; } = null;
    }

    public class ArtifactReceivedItems
    {
        /// <summary>
        /// Gets or Sets the Id.
        /// </summary>
        public double Id { get; set; }

        /// <summary>
        /// Gets or Sets WorkflowInstanceId.
        /// </summary>
        public string WorkflowInstanceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or Sets TaskId.
        /// </summary>
        public string TaskId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or Sets Artifacts.
        /// </summary>
        public List<ArtifactReceivedDetails> Artifacts { get; set; } = new();
    }

    public class ArtifactsRepository : IArtifactsRepository
    {
        private readonly ILogger<ArtifactsRepository> _logger;
        private readonly IMongoCollection<ArtifactReceivedItems> _artifactReceivedItemsCollection;

        public ArtifactsRepository(
            IMongoClient client,
            IOptions<WorkloadManagerDatabaseSettings> bookStoreDatabaseSettings,
            ILogger<ArtifactsRepository> logger)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var mongoDatabase = client.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
            _artifactReceivedItemsCollection = mongoDatabase.GetCollection<ArtifactReceivedItems>("ArtifactReceivedItems");
        }

        public async Task<List<ArtifactReceivedItems>> GetAllAsync(string workflowInstance, string taskId)
        {
            var result = await _artifactReceivedItemsCollection.FindAsync(a => a.WorkflowInstanceId == workflowInstance && a.TaskId == taskId).ConfigureAwait(false);
            return await result.ToListAsync().ConfigureAwait(false);
        }

        public Task AddItemAsync(ArtifactReceivedItems item)
        {
            return _artifactReceivedItemsCollection.InsertOneAsync(item);
        }

        public Task AddItemAsync(string workflowInstanceId, string taskId, List<Artifact> artifactsOutputs)
        {
            var artifacts = artifactsOutputs.Select(a => new ArtifactReceivedDetails()
            {
                Path = a.Path,
                Type = a.Type,
                Received = DateTime.UtcNow
            });

            var item = new ArtifactReceivedItems()
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = taskId,
                Artifacts = artifacts.ToList()
            };

            return _artifactReceivedItemsCollection.InsertOneAsync(item);
        }

        public async Task AddOrUpdateItemAsync(string workflowInstanceId, string taskId,
            IEnumerable<Artifact> artifactsOutputs)
        {
            var artifacts = artifactsOutputs.Select(a => new ArtifactReceivedDetails()
            {
                Path = a.Path,
                Type = a.Type,
                Received = DateTime.UtcNow
            });

            var item = new ArtifactReceivedItems()
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = taskId,
                Artifacts = artifacts.ToList()
            };

            var result = await _artifactReceivedItemsCollection
                .FindAsync(a => a.WorkflowInstanceId == workflowInstanceId && a.TaskId == taskId).ConfigureAwait(false);
            var existing = await result.FirstOrDefaultAsync().ConfigureAwait(false);

            if (existing == null)
            {
                await _artifactReceivedItemsCollection.InsertOneAsync(item).ConfigureAwait(false);
            }
            else
            {
                var update = Builders<ArtifactReceivedItems>.Update.Set(a => a.Artifacts, item.Artifacts);
                await _artifactReceivedItemsCollection
                    .UpdateOneAsync(a => a.WorkflowInstanceId == workflowInstanceId && a.TaskId == taskId, update)
                    .ConfigureAwait(false);
            }
        }
    }
}
