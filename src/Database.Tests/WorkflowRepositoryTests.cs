using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Options;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Database.Tests
{
    public class WorkflowRepositoryTests
    {
        private readonly Mock<IMongoClient> _mongoClient;

        private readonly IMongoCollection<Workflow> _workflowCollection;


        private readonly IWorkflowRepository _workflowRepository;

        public WorkflowRepositoryTests()
        {
            _mongoClient = new Mock<IMongoClient>();
            var configuration = Options.Create(new WorkloadManagerDatabaseSettings { DatabaseName = "testdb", WorkflowCollectionName = "workflow" });
            _workflowRepository = new WorkflowRepository(_mongoClient.Object, configuration);
        }

        [Fact]
        public async Task GetByWorkflowIdAsync_NullResponse_ReturnsEmptyList()
        {
            var workflowId = Guid.NewGuid().ToString();

            //_workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);

            var workflows = _workflowRepository.GetByWorkflowsIdsAsync(new List<string> { workflowId });
        }
    }
}
