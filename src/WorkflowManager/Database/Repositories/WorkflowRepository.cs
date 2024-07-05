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
using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Database.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Monai.Deploy.WorkflowManager.Common.Database.Repositories
{
    public class WorkflowRepository : RepositoryBase, IWorkflowRepository
    {
        private readonly IMongoCollection<WorkflowRevision> _workflowCollection;

        public WorkflowRepository(
            IMongoClient client,
            IOptions<WorkloadManagerDatabaseSettings> databaseSettings)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var mongoDatabase = client.GetDatabase(databaseSettings.Value.DatabaseName);
            _workflowCollection = mongoDatabase.GetCollection<WorkflowRevision>("Workflows");
            EnsureIndex().GetAwaiter().GetResult();
        }

        private async Task EnsureIndex()
        {
            ArgumentNullException.ThrowIfNull(_workflowCollection, "WorkflowCollection");

            var asyncCursor = (await _workflowCollection.Indexes.ListAsync());
            var bsonDocuments = (await asyncCursor.ToListAsync());
            var indexes = bsonDocuments.Select(_ => _.GetElement("name").Value.ToString()).ToList();

            // If index not present create it else skip.
            if (!indexes.Any(i => i is not null && i.Equals("AeTitleIndex")))
            {
                // Create Index here

                var options = new CreateIndexOptions()
                {
                    Name = "AeTitleIndex"
                };
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var model = new CreateIndexModel<WorkflowRevision>(
                    Builders<WorkflowRevision>.IndexKeys.Ascending(s => s.Workflow.InformaticsGateway.AeTitle),
                    options
                    );
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                await _workflowCollection.Indexes.CreateOneAsync(model);
            }
        }

        public List<WorkflowRevision> GetWorkflowsList()
        {
            var workflow = _workflowCollection
                            .AsQueryable()
                              .Where(w => w.Deleted == null)
                              .OrderByDescending(e => e.Revision)
                              .GroupBy(e => e.WorkflowId)
                              .Select(g => new WorkflowRevision
                              {
                                  Id = g.First().Id,
                                  WorkflowId = g.Key,
                                  Revision = g.First().Revision,
                                  Workflow = g.First().Workflow
                              })
                              .ToList();

            return workflow;
        }

        public async Task<WorkflowRevision> GetByWorkflowIdAsync(string workflowId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowId, nameof(workflowId));

            var workflow = await _workflowCollection
                .Find(x => x.WorkflowId == workflowId && x.Deleted == null)
                .Sort(Builders<WorkflowRevision>.Sort.Descending("Revision"))
                .FirstOrDefaultAsync();

            return workflow;
        }

        public async Task<IList<WorkflowRevision>> GetByWorkflowsIdsAsync(IEnumerable<string> workflowIds)
        {
            Guard.Against.NullOrEmpty(workflowIds, nameof(workflowIds));

            var workflows = new List<WorkflowRevision>();

            var filterDef = new FilterDefinitionBuilder<WorkflowRevision>();

            var filter = filterDef.And(
                filterDef.In(x => x.WorkflowId, workflowIds),
                filterDef.Where(x => x.Deleted == null)
            );

            workflows = await _workflowCollection
                .Find(filter)
                .Sort(Builders<WorkflowRevision>.Sort.Descending("Revision"))
                .ToListAsync();

            workflows = workflows.GroupBy(w => w.WorkflowId).Select(g => g.First()).ToList();

            return workflows;
        }

        public async Task<WorkflowRevision> GetByWorkflowNameAsync(string name)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var workflow = await _workflowCollection
                .Find(x => x.Workflow.Name.ToLower() == name.ToLower() && x.Deleted == null)
                .FirstOrDefaultAsync();

            return workflow;
        }

        public async Task<WorkflowRevision> GetByAeTitleAsync(string aeTitle)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(aeTitle, nameof(aeTitle));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var workflow = await _workflowCollection
                .Find(x => x.Workflow.InformaticsGateway.AeTitle == aeTitle && x.Deleted == null)
                .Sort(Builders<WorkflowRevision>.Sort.Descending("Revision"))
                .FirstOrDefaultAsync();

            return workflow;
        }

        public async Task<IEnumerable<WorkflowRevision>> GetAllByAeTitleAsync(string aeTitle, int? skip, int? limit)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(aeTitle, nameof(aeTitle));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var workflows = await _workflowCollection
                .Find(x => x.Workflow.InformaticsGateway.AeTitle == aeTitle && x.Deleted == null)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();

            return workflows
                .GroupBy(w => w.WorkflowId)
                .Select(g => g.First())
                .ToList(); ;
        }

        public async Task<long> GetCountByAeTitleAsync(string aeTitle)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(aeTitle, nameof(aeTitle));
            return await _workflowCollection
                .CountDocumentsAsync(x => x.Workflow.InformaticsGateway.AeTitle == aeTitle && x.Deleted == null);
        }

        public async Task<IList<WorkflowRevision>> GetWorkflowsByAeTitleAsync(List<string> aeTitles)
        {
            Guard.Against.NullOrEmpty(aeTitles, nameof(aeTitles));

            var workflows = new List<WorkflowRevision>();

            foreach (var aeTitle in aeTitles)
            {
                var wfs = await _workflowCollection
                        .Find(x => x.Workflow.InformaticsGateway.AeTitle == aeTitle && x.Deleted == null)
                        .ToListAsync();

                workflows.AddRange(wfs);
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            workflows = workflows
                .Distinct()
                .OrderByDescending(w => w.Revision)
                .GroupBy(w => w.WorkflowId)
                .Select(g => g.First())
                .ToList();

            return workflows;
        }

        public async Task<IList<WorkflowRevision>> GetWorkflowsForWorkflowRequestAsync(string calledAeTitle, string callingAeTitle)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(calledAeTitle, nameof(calledAeTitle));
            ArgumentNullException.ThrowIfNullOrEmpty(callingAeTitle, nameof(callingAeTitle));

            var wfs = await _workflowCollection
                .Find(x =>
                    x.Workflow != null &&
                    x.Workflow.InformaticsGateway != null &&
                    ((x.Workflow.InformaticsGateway.AeTitle == calledAeTitle &&
                        (x.Workflow.InformaticsGateway.DataOrigins == null ||
                        x.Workflow.InformaticsGateway.DataOrigins.Length == 0)) ||
                    x.Workflow.InformaticsGateway.AeTitle == calledAeTitle &&
                        x.Workflow.InformaticsGateway.DataOrigins != null &&
                        x.Workflow.InformaticsGateway.DataOrigins.Any(d => d == callingAeTitle)) &&
                    x.Deleted == null)
                .ToListAsync();

            return wfs;
        }

        public async Task<string> CreateAsync(Workflow workflow)
        {
            ArgumentNullException.ThrowIfNull(workflow, nameof(workflow));

            var workflowRevision = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                Revision = 1,
                Workflow = workflow
            };

            await _workflowCollection.InsertOneAsync(workflowRevision);

            return workflowRevision.WorkflowId;
        }

        public async Task<string> UpdateAsync(Workflow workflow, WorkflowRevision existingWorkflow)
        {
            ArgumentNullException.ThrowIfNull(workflow, nameof(workflow));
            ArgumentNullException.ThrowIfNull(existingWorkflow, nameof(existingWorkflow));

            var workflowRevision = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = existingWorkflow.WorkflowId,
                Revision = ++existingWorkflow.Revision,
                Workflow = workflow
            };

            await SoftDeleteWorkflow(existingWorkflow);

            await _workflowCollection.InsertOneAsync(workflowRevision);

            return workflowRevision.WorkflowId;
        }

        public async Task<DateTime> SoftDeleteWorkflow(WorkflowRevision workflow)
        {
            ArgumentNullException.ThrowIfNull(workflow, nameof(workflow));

            var deletedTimeStamp = DateTime.UtcNow;

            await _workflowCollection.UpdateManyAsync(
                wr => wr.WorkflowId == workflow.WorkflowId && wr.Deleted == null,
                Builders<WorkflowRevision>.Update.Set(rec => rec.Deleted, deletedTimeStamp));

            return deletedTimeStamp;
        }

        public async Task<long> CountAsync(FilterDefinition<WorkflowRevision> filter)
        {
            var builder = Builders<WorkflowRevision>.Filter;
            filter &= builder.Eq(p => p.Deleted, null);
            return await _workflowCollection.CountDocumentsAsync(filter);
        }

        public async Task<IList<WorkflowRevision>> GetAllAsync(int? skip = null, int? limit = null)
            => await GetAllAsync(_workflowCollection,
                                      x => x.Deleted == null,
                                      Builders<WorkflowRevision>.Sort.Descending(x => x.Id),
                                      skip,
                                      limit);
    }
}
