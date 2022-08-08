/*
 * Copyright 2021-2022 MONAI Consortium
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

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using Ardalis.GuardClauses;
using System.Linq;

namespace Monai.Deploy.WorkflowManager.Database.Repositories
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
            _workflowCollection = mongoDatabase.GetCollection<WorkflowRevision>(databaseSettings.Value.WorkflowCollectionName);
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
            Guard.Against.NullOrWhiteSpace(workflowId, nameof(workflowId));

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

        public async Task<WorkflowRevision> GetByAeTitleAsync(string aeTitle)
        {
            Guard.Against.NullOrWhiteSpace(aeTitle, nameof(aeTitle));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var workflow = await _workflowCollection
                .Find(x => x.Workflow.InformaticsGateway.AeTitle == aeTitle && x.Deleted == null)
                .Sort(Builders<WorkflowRevision>.Sort.Descending("Revision"))
                .FirstOrDefaultAsync();

            return workflow;
        }

        public async Task<IList<WorkflowRevision>> GetWorkflowsByAeTitleAsync(string aeTitle)
        {
            Guard.Against.NullOrWhiteSpace(aeTitle, nameof(aeTitle));

            var workflows = new List<WorkflowRevision>();


            workflows = await _workflowCollection
                .Find(x => x.Workflow.InformaticsGateway.AeTitle == aeTitle)
                .Sort(Builders<WorkflowRevision>.Sort.Descending("Revision"))
                .ToListAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            workflows = workflows.GroupBy(w => w.WorkflowId).Select(g => g.First()).ToList();

            return workflows;
        }

        public async Task<string> CreateAsync(Workflow workflow)
        {
            Guard.Against.Null(workflow, nameof(workflow));

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
            Guard.Against.Null(workflow, nameof(workflow));
            Guard.Against.Null(existingWorkflow, nameof(existingWorkflow));

            var workflowRevision = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = existingWorkflow.WorkflowId,
                Revision = ++existingWorkflow.Revision,
                Workflow = workflow
            };

            await _workflowCollection.InsertOneAsync(workflowRevision);

            return workflowRevision.WorkflowId;
        }

        public async Task<DateTime> SoftDeleteWorkflow(WorkflowRevision workflow)
        {
            Guard.Against.Null(workflow);

            var deletedTimeStamp = DateTime.UtcNow;

            await _workflowCollection.UpdateManyAsync(
                wr => wr.WorkflowId == workflow.WorkflowId,
                Builders<WorkflowRevision>.Update.Set(rec => rec.Deleted, deletedTimeStamp));

            return deletedTimeStamp;
        }

        public Task<long> CountAsync() => CountAsync(_workflowCollection, null);

        public async Task<IList<WorkflowRevision>> GetAllAsync(int? skip = null, int? limit = null)
            => await GetAllAsync(_workflowCollection,
                                      x => x.Deleted == null,
                                      Builders<WorkflowRevision>.Sort.Descending(x => x.Id),
                                      skip,
                                      limit);

    }
}
