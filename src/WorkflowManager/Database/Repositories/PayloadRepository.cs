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
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Database.Options;
using Monai.Deploy.WorkflowManager.Common.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Common.Database.Repositories
{
    public class PayloadRepository : RepositoryBase, IPayloadRepository
    {
        private readonly IMongoCollection<Payload> _payloadCollection;
        private readonly ILogger<PayloadRepository> _logger;

        public PayloadRepository(
            IMongoClient client,
            IOptions<WorkloadManagerDatabaseSettings> databaseSettings,
            ILogger<PayloadRepository> logger)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var mongoDatabase = client.GetDatabase(databaseSettings.Value.DatabaseName);
            _payloadCollection = mongoDatabase.GetCollection<Payload>("Payloads");
            EnsureIndex().GetAwaiter().GetResult();
        }

        private async Task EnsureIndex()
        {
            var indexName = "PayloadDeletedIndex";

            var model = new CreateIndexModel<Payload>(
                    Builders<Payload>.IndexKeys.Ascending(s => s.PayloadDeleted),
                        new CreateIndexOptions { Name = indexName }
                    );


            var asyncCursor = (await _payloadCollection.Indexes.ListAsync());
            var bsonDocuments = (await asyncCursor.ToListAsync());
            var indexes = bsonDocuments.Select(_ => _.GetElement("name").Value.ToString()).ToList();

            // If index not present create it else skip.
            if (!indexes.Exists(i => i is not null && i.Equals(indexName)))
            {
                await _payloadCollection.Indexes.CreateOneAsync(model);
            }
        }

        public Task<long> CountAsync(FilterDefinition<Payload> filter)
        {
            return CountAsync(_payloadCollection, filter);
        }

        public async Task<bool> CreateAsync(Payload payload)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));

            try
            {
                await _payloadCollection.InsertOneAsync(payload);

                return true;
            }
            catch (Exception e)
            {
                _logger.DbPayloadCreationError(e);

                return false;
            }
        }

        public async Task<IList<Payload>> GetAllAsync(int? skip = null, int? limit = null, string? patientId = null, string? patientName = null, string? accessionId = null)
        {
            var builder = Builders<Payload>.Filter;
            var filter = builder.Empty;
            if (!string.IsNullOrEmpty(patientId)) filter
                    &= builder.Regex(p => p.PatientDetails.PatientId, new BsonRegularExpression($"/{patientId}/i"));

            if (!string.IsNullOrEmpty(patientName)) filter
                    &= builder.Regex(p => p.PatientDetails.PatientName, new BsonRegularExpression($"/{patientName}/i"));

            if (!string.IsNullOrWhiteSpace(accessionId)) filter
                    &= builder.Regex(p => p.AccessionId, new BsonRegularExpression($"/{accessionId}/i"));


            return await GetAllAsync(_payloadCollection,
                                      filter,
                                      Builders<Payload>.Sort.Descending(x => x.Timestamp),
                                      skip,
                                      limit);
        }

        public async Task<Payload> GetByIdAsync(string payloadId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(payloadId, nameof(payloadId));

            var payload = await _payloadCollection
                .Find(x => x.PayloadId == payloadId)
                .FirstOrDefaultAsync();

            return payload;
        }

        public async Task<bool> UpdateAsyncWorkflowIds(Payload payload)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));

            try
            {
                var filter = Builders<Payload>.Filter.Eq(p => p.PayloadId, payload.PayloadId);
                await _payloadCollection.ReplaceOneAsync(filter, payload);

                await _payloadCollection.FindOneAndUpdateAsync(
                    i => i.Id == payload.Id,
                    Builders<Payload>.Update
                        .Set(p => p.TriggeredWorkflowNames, payload.TriggeredWorkflowNames)
                        .Set(p => p.WorkflowInstanceIds, payload.WorkflowInstanceIds));

                return true;
            }
            catch (Exception ex)
            {
                _logger.DbUpdatePayloadError(payload.PayloadId, ex);
                return false;
            }
        }

        public async Task<IList<Payload>> GetPayloadsToDelete(DateTime now)
        {
            try
            {
                var filter = (Builders<Payload>.Filter.Eq(p => p.PayloadDeleted, PayloadDeleted.No) |
                             Builders<Payload>.Filter.Eq(p => p.PayloadDeleted, PayloadDeleted.Failed)) &
                             Builders<Payload>.Filter.Lt(p => p.Expires, now);

                return await (await _payloadCollection.FindAsync(filter)).ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.DbGetPayloadsToDeleteError(ex);
                return new List<Payload>();
            }
        }

        public async Task MarkDeletedState(IList<string> Ids, PayloadDeleted status)
        {
            try
            {
                var filter = Builders<Payload>.Filter.In(p => p.PayloadId, Ids);
                var update = Builders<Payload>.Update.Set(p => p.PayloadDeleted, status);
                await _payloadCollection.UpdateManyAsync(filter, update);
            }
            catch (Exception ex)
            {
                _logger.DbGetPayloadsToDeleteError(ex);
            }
        }
    }
}
