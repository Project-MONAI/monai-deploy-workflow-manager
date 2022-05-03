using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Services;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.WorkflowExecuter.Tests.Services
{
    public class WorkflowExecuterServiceTests
    {
        private IWorkflowExecuterService WorkflowExecuterService { get; set; }

        private readonly Mock<IWorkflowRepository> _workflowRepository;
        private readonly Mock<IWorkflowInstanceRepository> _workflowInstanceRepository;
        private readonly Mock<IMessageBrokerPublisherService> _messageBrokerPublisherService;
        private readonly Mock<IStorageService> _storageService;
        private readonly IOptions<WorkflowManagerOptions> _configuration;
        private readonly IOptions<StorageServiceConfiguration> _storageConfiguration;

        public WorkflowExecuterServiceTests()
        {
            _workflowRepository = new Mock<IWorkflowRepository>();
            _workflowInstanceRepository = new Mock<IWorkflowInstanceRepository>();
            _messageBrokerPublisherService = new Mock<IMessageBrokerPublisherService>();
            _storageService = new Mock<IStorageService>();
            _configuration = Options.Create(new WorkflowManagerOptions());
            _storageConfiguration = Options.Create(new StorageServiceConfiguration());

            WorkflowExecuterService = new WorkflowExecuterService(_configuration, _storageConfiguration, _workflowRepository.Object, _workflowInstanceRepository.Object, _messageBrokerPublisherService.Object, _storageService.Object);
        }

        [Fact]
        public void ProcessPayload_ValidAeTitleWorkflowRequest_ReturnesTrue()
        {
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                CalledAeTitle = "aetitle",
                CallingAeTitle = "aetitle",
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            var workflows = new List<Workflow>
            {
                new Workflow
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid(),
                    Revision = 1,
                    WorkflowSpec = new WorkflowSpec
                    {
                        Name = "Workflowname",
                        Description = "Workflowdesc",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetWorkflowsByAeTitleAsync(workflowRequest.CalledAeTitle)).ReturnsAsync(workflows);
        }
    }
}
