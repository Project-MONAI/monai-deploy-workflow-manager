// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Options;
using MongoDB.Driver;
using System.Linq;

namespace Monai.Deploy.WorkflowManager.Database
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly IMongoClient _client;
        private readonly IMongoCollection<Workflow> _workflowCollection;

        public WorkflowRepository(
            IMongoClient client,
            IOptions<WorkloadManagerDatabaseSettings> bookStoreDatabaseSettings)
        {
            _client = client;
            var mongoDatabase = client.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
            _workflowCollection = mongoDatabase.GetCollection<Workflow>(bookStoreDatabaseSettings.Value.WorkflowCollectionName);
        }

        public async Task<Workflow> GetByWorkflowIdAsync(Guid workflowId)
        {
            var workflow = await _workflowCollection
                .Find(x => x.WorkflowId == workflowId)
                .Sort(Builders<Workflow>.Sort.Descending("Revision"))
                .FirstOrDefaultAsync();

            return workflow;
        }

        public async Task<IList<Workflow>> GetByWorkflowsIdsAsync(IEnumerable<string> workflowIds)
        {
            var filterDef = new FilterDefinitionBuilder<Workflow>();

            var workflowIdGuids = workflowIds.Select(Guid.Parse).ToList();

            var filter = filterDef.In(x => x.WorkflowId, workflowIdGuids);

            var workflows = await _workflowCollection
                .Find(filter).ToListAsync();

            return workflows;
        }

        public async Task<Workflow> GetByAeTitleAsync(string aeTitle)
        {
            var workflow = await _workflowCollection
                .Find(x => x.WorkflowSpec.InformaticsGateway.AeTitle == aeTitle)
                .Sort(Builders<Workflow>.Sort.Descending("Revision"))
                .FirstOrDefaultAsync();

            return workflow;
        }

        public async Task<IList<Workflow>> GetWorkflowsByAeTitleAsync(string aeTitle)
        {
            var workflows = await _workflowCollection
                .Find(x => x.WorkflowSpec.InformaticsGateway.AeTitle == aeTitle)
                .Sort(Builders<Workflow>.Sort.Descending("Revision"))
                .ToListAsync();

            return workflows;
        }
    }
}
