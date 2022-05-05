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

namespace Monai.Deploy.WorkflowManager.Database
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly IMongoCollection<Workflow> _workflowCollection;

        public WorkflowRepository(
            IMongoClient client,
            IOptions<WorkloadManagerDatabaseSettings> databaseSettings)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var mongoDatabase = client.GetDatabase(databaseSettings.Value.DatabaseName);
            _workflowCollection = mongoDatabase.GetCollection<Workflow>(databaseSettings.Value.WorkflowCollectionName);
        }

        public async Task<Workflow> GetByWorkflowIdAsync(string workflowId)
        {
            Guard.Against.NullOrWhiteSpace(workflowId, nameof(workflowId));

            var workflow = await _workflowCollection
                .Find(x => x.WorkflowId == workflowId)
                .Sort(Builders<Workflow>.Sort.Descending("Revision"))
                .FirstOrDefaultAsync();

            return workflow;
        }

        public async Task<IList<Workflow>> GetByWorkflowsIdsAsync(IEnumerable<string> workflowIds)
        {
            Guard.Against.NullOrEmpty(workflowIds, nameof(workflowIds));

            var filterDef = new FilterDefinitionBuilder<Workflow>();

            var filter = filterDef.In(x => x.WorkflowId, workflowIds);

            var workflows = await _workflowCollection
                .Find(filter).ToListAsync();

            return workflows ?? new List<Workflow>();
        }

        public async Task<Workflow> GetByAeTitleAsync(string aeTitle)
        {
            Guard.Against.NullOrWhiteSpace(aeTitle, nameof(aeTitle));

            var workflow = await _workflowCollection
                .Find(x => x.WorkflowSpec.InformaticsGateway.AeTitle == aeTitle)
                .Sort(Builders<Workflow>.Sort.Descending("Revision"))
                .FirstOrDefaultAsync();

            return workflow;
        }

        public async Task<IList<Workflow>> GetWorkflowsByAeTitleAsync(string aeTitle)
        {
            Guard.Against.NullOrWhiteSpace(aeTitle, nameof(aeTitle));

            var workflows = await _workflowCollection
                .Find(x => x.WorkflowSpec.InformaticsGateway.AeTitle == aeTitle)
                .Sort(Builders<Workflow>.Sort.Descending("Revision"))
                .ToListAsync();

            return workflows ?? new List<Workflow>();
        }
    }
}
