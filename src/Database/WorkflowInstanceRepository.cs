using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Database
{
    public class WorkflowInstanceRepository : IWorkflowInstanceRepository
    {
        private readonly IMongoClient _client;
        private readonly IMongoCollection<WorkflowInstance> _workflowInstanceCollection;

        public WorkflowInstanceRepository(
            IMongoClient client,
            IOptions<WorkloadManagerDatabaseSettings> bookStoreDatabaseSettings)
        {
            _client = client;
            var mongoDatabase = client.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
            _workflowInstanceCollection = mongoDatabase.GetCollection<WorkflowInstance>(bookStoreDatabaseSettings.Value.WorkflowInstanceCollectionName);
        }

        public async Task<bool> CreateAsync(IList<WorkflowInstance> workflowInstances)
        {
            using var session = await _client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                await _workflowInstanceCollection.InsertManyAsync(workflowInstances);
                await session.CommitTransactionAsync();

                return true;
            }
            catch (Exception e)
            {
                await session.AbortTransactionAsync();
                return false;
            }
        }
    }
}
