// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using MongoDB.Driver;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Services
{
    public class WorkflowMetadataRepository : IWorkflowMetadataRepository
    {
        private readonly IMongoCollection<WorkflowMetaData> _workflowMetadataCollection;
        private readonly ILogger<WorkflowMetadataRepository> _logger;

        public WorkflowMetadataRepository(IMongoClient client,
                                          IOptions<WorkloadManagerDatabaseSettings> bookStoreDatabaseSettings,
                                          ILogger<WorkflowMetadataRepository> logger)
        {
            var mongoDatabase = client.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
            _workflowMetadataCollection = mongoDatabase.GetCollection<WorkflowMetaData>(bookStoreDatabaseSettings.Value.WorkflowInstanceCollectionName);
            _logger = logger;
        }

        public async Task<bool> UpdateForTaskAsync(string workflowInstanceId, List<TaskMetadata> metadata)
        {
            Guard.Against.NullOrEmpty(workflowInstanceId);
            Guard.Against.NullOrEmpty(metadata);

            try
            {
                var result = await _workflowMetadataCollection.FindOneAndUpdateAsync(
                    i => i.WorkflowInstanceId == workflowInstanceId,
                    Builders<WorkflowMetaData>.Update.Set(w => w.Metadata, metadata));
                return true;
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(UpdateForTaskAsync), e);
                return false;
            }
        }
    }
}
