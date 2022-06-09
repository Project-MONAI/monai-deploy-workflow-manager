using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.Controllers;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Test.Controllers
{
    public class WorkflowsControllerTests
    {
        private WorkflowsController WorkflowsController { get; set; }

        private readonly Mock<IWorkflowService> _workflowService;
        private readonly Mock<ILogger<WorkflowsController>> _logger;

        public WorkflowsControllerTests()
        {
            _workflowService = new Mock<IWorkflowService>();
            _logger = new Mock<ILogger<WorkflowsController>>();

            WorkflowsController = new WorkflowsController(_workflowService.Object, _logger.Object);
        }

        [Fact]
        public void GetList_WorkflowsExist_ReturnsList()
        {
            var workflows = new List<WorkflowRevision>
            {
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow
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

            _workflowService.Setup(w => w.GetList()).Returns(workflows);

            var result = WorkflowsController.GetList();

            var objectResult = Assert.IsType<OkObjectResult>(result);

            objectResult.Value.Should().BeEquivalentTo(workflows);
        }

        [Fact]
        public void GetList_ServiceException_ReturnProblem()
        {
            _workflowService.Setup(w => w.GetList()).Throws(new Exception());

            var result = WorkflowsController.GetList();

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
