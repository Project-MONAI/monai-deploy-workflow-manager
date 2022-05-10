// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Options;
using MongoDB.Driver;
using System;
using Ardalis.GuardClauses;
using System.Linq;

namespace Monai.Deploy.WorkflowManager.Database
{
    public class WorkflowRepository : IWorkflowRepository
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

        public async Task<WorkflowRevision> GetByWorkflowIdAsync(string workflowId)
        {
            Guard.Against.NullOrWhiteSpace(workflowId, nameof(workflowId));

            var workflow = await _workflowCollection
                .Find(x => x.WorkflowId == workflowId)
                .Sort(Builders<WorkflowRevision>.Sort.Descending("Revision"))
                .FirstOrDefaultAsync();

            return workflow;
        }

        public async Task<IList<WorkflowRevision>> GetByWorkflowsIdsAsync(IEnumerable<string> workflowIds)
        {
            Guard.Against.NullOrEmpty(workflowIds, nameof(workflowIds));

            var workflows = new List<WorkflowRevision>();

            var filterDef = new FilterDefinitionBuilder<WorkflowRevision>();

            var filter = filterDef.In(x => x.WorkflowId, workflowIds);

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

            var workflow = await _workflowCollection
                .Find(x => x.Workflow.InformaticsGateway.AeTitle == aeTitle)
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

            workflows = workflows.GroupBy(w => w.WorkflowId).Select(g => g.First()).ToList();

            return workflows;
        }

        public async Task<string> CreateAsync(Workflow workflow)
        {
            Guard.Against.Null(workflow, nameof(workflow));

            foreach (var task in workflow.Tasks)
            {
                task.Id = Guid.NewGuid().ToString();
            }

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
    }
}
