// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManger.Common.Tests.Services
{
    public class WorkflowServiceTests
    {
        private IWorkflowService WorkflowService { get; set; }

        private readonly Mock<IWorkflowRepository> _workflowRepository;

        public WorkflowServiceTests()
        {
            _workflowRepository = new Mock<IWorkflowRepository>();

            WorkflowService = new WorkflowService(_workflowRepository.Object);
        }

        [Fact]
        public async Task WorkflowService_NullWorkflow_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => WorkflowService.UpdateAsync(null, null));
        }

        [Fact]
        public async Task WorkflowService_NoExistingWorkflow_ReturnsNull()
        {
            var result = await WorkflowService.UpdateAsync(new Workflow(), Guid.NewGuid().ToString());

            Assert.Null(result);
        }

        [Fact]
        public async Task WorkflowService_WorkflowExists_ReturnsWorkflowId()
        {
            var workflowRevision = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname1",
                    Description = "Workflowdesc1",
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
            };

            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowRevision.WorkflowId)).ReturnsAsync(workflowRevision);
            _workflowRepository.Setup(w => w.UpdateAsync(It.IsAny<Workflow>(), workflowRevision)).ReturnsAsync(workflowRevision.WorkflowId);

            var result = await WorkflowService.UpdateAsync(new Workflow(), workflowRevision.WorkflowId);

            Assert.Equal(workflowRevision.WorkflowId, result);
        }
    }
}
