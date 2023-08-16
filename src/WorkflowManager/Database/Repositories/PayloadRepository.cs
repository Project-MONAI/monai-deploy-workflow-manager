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
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Common.Contracts.Models;
using Monai.Deploy.Common.Database.Interfaces;
using Monai.Deploy.Common.Database.Options;
using Monai.Deploy.Common.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Monai.Deploy.Common.Database.Repositories
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
        }

        public Task<long> CountAsync() => CountAsync(_payloadCollection, null);

        public async Task<bool> CreateAsync(Payload payload)
        {
            Guard.Against.Null(payload, nameof(payload));

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

        public async Task<IList<Payload>> GetAllAsync(int? skip = null, int? limit = null, string? patientId = "", string? patientName = "")
        {
            var builder = Builders<Payload>.Filter;
            var filter = builder.Empty;
            if (!string.IsNullOrEmpty(patientId))
            {
                filter &= builder.Regex(p => p.PatientDetails.PatientId, new BsonRegularExpression($"/{patientId}/i"));
            }
            if (!string.IsNullOrEmpty(patientName))
            {
                filter &= builder.Regex(p => p.PatientDetails.PatientName, new BsonRegularExpression($"/{patientName}/i"));
            }

            return await GetAllAsync(_payloadCollection,
                                      filter,
                                      Builders<Payload>.Sort.Descending(x => x.Timestamp),
                                      skip,
                                      limit);
        }

        public async Task<Payload> GetByIdAsync(string payloadId)
        {
            Guard.Against.NullOrWhiteSpace(payloadId, nameof(payloadId));

            var payload = await _payloadCollection
                .Find(x => x.PayloadId == payloadId)
                .FirstOrDefaultAsync();

            return payload;
        }

        public async Task<bool> UpdateAsync(Payload payload)
        {
            Guard.Against.Null(payload, nameof(payload));

            try
            {
                var filter = Builders<Payload>.Filter.Eq(p => p.PayloadId, payload.PayloadId);
                await _payloadCollection.ReplaceOneAsync(filter, payload);

                return true;
            }
            catch (Exception ex)
            {
                _logger.DbUpdatePayloadError(payload.PayloadId, ex);
                return false;
            }
        }

        public async Task<bool> UpdateAssociatedWorkflowInstancesAsync(string payloadId, IEnumerable<string> workflowInstances)
        {
            Guard.Against.NullOrEmpty(workflowInstances, nameof(workflowInstances));
            Guard.Against.NullOrWhiteSpace(payloadId, nameof(payloadId));

            try
            {
                await _payloadCollection.FindOneAndUpdateAsync(
                    i => i.Id == payloadId,
                    Builders<Payload>.Update.Set(p => p.WorkflowInstanceIds, workflowInstances));

                return true;
            }
            catch (Exception ex)
            {
                _logger.DbUpdateWorkflowInstanceError(ex);
                return false;
            }
        }
    }
}
