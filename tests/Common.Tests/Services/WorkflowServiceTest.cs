// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Moq;
using Xunit;

namespace Common.Tests
{
    public class WorkflowServiceTest
    {
        private IWorkflowService WorkflowService { get; set; }

        private readonly Mock<IWorkflowRepository> _workflowRepository;

        public WorkflowServiceTest()
        {
            _workflowRepository = new Mock<IWorkflowRepository>();

            WorkflowService = new WorkflowService(_workflowRepository.Object);
        }

        [Fact]
        public void CreateAsync_NullWorkflow_ThrowsException() => Assert.ThrowsAsync<ArgumentNullException>(() => WorkflowService.CreateAsync(null));

        [Fact]
        public async Task CreateAsync_ValidWorkflow_ReturnsWorkflowId()
        {
            var workflow = new Workflow
            {
                Description = "workflowdescription",
                Version = "workflowVersion",
                Name = "workflowname",
                InformaticsGateway = new InformaticsGateway
                {
                    AeTitle = "aetitle",
                    ExportDestinations = { },
                    DataOrigins = new[] { "PACS" }
                },
                Tasks = new TaskObject[]
                {
                    new TaskObject
                    {
                        Description = "taskdescription",
                        Type = "argo",
                        Args = {}
                    }
                }

            };
            var id = Guid.NewGuid().ToString();

            _workflowRepository.Setup(w => w.CreateAsync(workflow)).ReturnsAsync(id);

            var result = await WorkflowService.CreateAsync(workflow);

            Assert.Equal(id, result);
        }

        [Fact]
        public void GetAsync_NullId_ThrowsException() => Assert.ThrowsAsync<ArgumentNullException>(() => WorkflowService.GetAsync(null));

        [Fact]
        public async Task GetAsync_ValidId_ReturnsWorkflowRevision()
        {
            var workflowRevision = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                Revision = 1,
                Workflow = new Workflow
                {
                    Description = "workflowdescription",
                    Version = "workflowVersion",
                    Name = "workflowname",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle",
                        ExportDestinations = { },
                        DataOrigins = new[] { "PACS" }
                    },
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Description = "taskdescription",
                            Type = "argo",
                            Args = {}
                        }
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowRevision.Id)).ReturnsAsync(workflowRevision);

            var result = await WorkflowService.GetAsync(workflowRevision.Id);

            result.Should().BeEquivalentTo(workflowRevision);
        }
    }
}
