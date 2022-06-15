// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Contracts.Responses;
using Monai.Deploy.WorkflowManager.Controllers;
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

        [Fact]
        public async Task UpdateAsync_InvalidWorkflow_ReturnsBadRequest()
        {
            var newWorkflow = new Workflow
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
                        Description = "taskdesc",
                        Args = new Dictionary<string, string>
                        {
                            { "test", "test" }
                        }
                    }
                }
            };

            var workflowRevision = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname",
                    Description = "Workflowdesc",
                    Version = "2",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle",
                        DataOrigins = new[] { "test" },
                        ExportDestinations = new[] { "test" }

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

            var result = await WorkflowsController.UpdateAsync(newWorkflow, workflowRevision.WorkflowId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_WorkflowsDoesNotExist_ReturnsNotFound()
        {
            var newWorkflow = new Workflow
            {
                Name = "Workflowname",
                Description = "Workflowdesc",
                Version = "1",
                InformaticsGateway = new InformaticsGateway
                {
                    AeTitle = "aetitle",
                    DataOrigins = new[] { "test" },
                    ExportDestinations = new[] { "test" }
                },
                Tasks = new TaskObject[]
                {
                    new TaskObject {
                        Id = Guid.NewGuid().ToString(),
                        Type = "type",
                        Description = "taskdesc",
                        Args = new Dictionary<string, string>
                        {
                            { "test", "test" }
                        }
                    }
                }
            };

            var workflowRevision = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname",
                    Description = "Workflowdesc",
                    Version = "2",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle",
                        DataOrigins = new[] { "test" },
                        ExportDestinations = new[] { "test" }

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

            var result = await WorkflowsController.UpdateAsync(newWorkflow, workflowRevision.WorkflowId);

            var objectResult = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_WorkflowsExist_ReturnsWorkflowId()
        {
            var newWorkflow = new Workflow
            {
                Name = "Workflowname",
                Description = "Workflowdesc",
                Version = "1",
                InformaticsGateway = new InformaticsGateway
                {
                    AeTitle = "aetitle",
                    DataOrigins = new[] { "test" },
                    ExportDestinations = new[] { "test" }
                },
                Tasks = new TaskObject[]
                {
                    new TaskObject {
                        Id = Guid.NewGuid().ToString(),
                        Type = "type",
                        Description = "taskdesc",
                        Args = new Dictionary<string, string>
                        {
                            { "test", "test" }
                        }
                    }
                }
            };

            var workflowRevision = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname",
                    Description = "Workflowdesc",
                    Version = "2",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle",
                        DataOrigins = new[] { "test" },
                        ExportDestinations = new[] { "test" }

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

            var response = new CreateWorkflowResponse(workflowRevision.WorkflowId);

            _workflowService.Setup(w => w.UpdateAsync(newWorkflow, workflowRevision.WorkflowId)).ReturnsAsync(workflowRevision.WorkflowId);

            var result = await WorkflowsController.UpdateAsync(newWorkflow, workflowRevision.WorkflowId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(201, objectResult.StatusCode);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task DeleteAsync_WorkflowsExist_SoftDeltesWorkflow()
        {
            var workflowRevisionId = Guid.NewGuid().ToString();
            var newWorkflow = new Workflow
            {
                Name = "Workflowname",
                Description = "Workflowdesc",
                Version = "1",
                InformaticsGateway = new InformaticsGateway
                {
                    AeTitle = "aetitle",
                    DataOrigins = new[] { "test" },
                    ExportDestinations = new[] { "test" }
                },
                Tasks = new TaskObject[]
                {
                    new TaskObject {
                        Id = Guid.NewGuid().ToString(),
                        Type = "type",
                        Description = "taskdesc",
                        Args = new Dictionary<string, string>
                        {
                            { "test", "test" }
                        }
                    }
                }
            };

            var workflowRevision = new WorkflowRevision
            {
                Id = workflowRevisionId,
                WorkflowId = Guid.NewGuid().ToString(),
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname",
                    Description = "Workflowdesc",
                    Version = "2",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle",
                        DataOrigins = new[] { "test" },
                        ExportDestinations = new[] { "test" }

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

            var response = new CreateWorkflowResponse(workflowRevision.WorkflowId);

            var dateNow = DateTime.UtcNow;

            _workflowService.Setup(w => w.DeleteWorkflowAsync(workflowRevision)).ReturnsAsync(dateNow);
            _workflowService.Setup(w => w.GetAsync(workflowRevisionId)).ReturnsAsync(workflowRevision);

            var result = await WorkflowsController.DeleteAsync(workflowRevisionId);

            var objectResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(200, objectResult.StatusCode);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task DeleteAsync_WorkflowsDoesntExist_SoftDeltesWorkflow()
        {
            var wrongGuid = Guid.NewGuid().ToString();
            var workflowRevisionId = Guid.NewGuid().ToString();
            var newWorkflow = new Workflow
            {
                Name = "Workflowname",
                Description = "Workflowdesc",
                Version = "1",
                InformaticsGateway = new InformaticsGateway
                {
                    AeTitle = "aetitle",
                    DataOrigins = new[] { "test" },
                    ExportDestinations = new[] { "test" }
                },
                Tasks = new TaskObject[]
                {
                    new TaskObject {
                        Id = Guid.NewGuid().ToString(),
                        Type = "type",
                        Description = "taskdesc",
                        Args = new Dictionary<string, string>
                        {
                            { "test", "test" }
                        }
                    }
                }
            };

            var workflowRevision = new WorkflowRevision
            {
                Id = workflowRevisionId,
                WorkflowId = Guid.NewGuid().ToString(),
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname",
                    Description = "Workflowdesc",
                    Version = "2",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle",
                        DataOrigins = new[] { "test" },
                        ExportDestinations = new[] { "test" }

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
            var dateNow = DateTime.UtcNow;

            _workflowService.Setup(w => w.DeleteWorkflowAsync(workflowRevision)).ReturnsAsync(dateNow);
            _workflowService.Setup(w => w.GetAsync(workflowRevisionId)).ReturnsAsync(workflowRevision);

            var result = await WorkflowsController.DeleteAsync(wrongGuid);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(result.As<ObjectResult>().Value.As<ProblemDetails>().Detail, "Failed to validate id, workflow not found");

            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsync_WorkflowsServiceThrowsException_Should500Error()
        {
            var wrongGuid = Guid.NewGuid().ToString();
            var workflowRevisionId = Guid.NewGuid().ToString();
            var newWorkflow = new Workflow
            {
                Name = "Workflowname",
                Description = "Workflowdesc",
                Version = "1",
                InformaticsGateway = new InformaticsGateway
                {
                    AeTitle = "aetitle",
                    DataOrigins = new[] { "test" },
                    ExportDestinations = new[] { "test" }
                },
                Tasks = new TaskObject[]
                {
                    new TaskObject {
                        Id = Guid.NewGuid().ToString(),
                        Type = "type",
                        Description = "taskdesc",
                        Args = new Dictionary<string, string>
                        {
                            { "test", "test" }
                        }
                    }
                }
            };

            var workflowRevision = new WorkflowRevision
            {
                Id = workflowRevisionId,
                WorkflowId = Guid.NewGuid().ToString(),
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname",
                    Description = "Workflowdesc",
                    Version = "2",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle",
                        DataOrigins = new[] { "test" },
                        ExportDestinations = new[] { "test" }

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
            var dateNow = DateTime.UtcNow;

            _workflowService.Setup(w => w.GetAsync(It.IsAny<string>()))
                .Throws(new ApplicationException());

            var result = await WorkflowsController.DeleteAsync(wrongGuid);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(result.As<ObjectResult>().Value.As<ProblemDetails>().Detail, "Unexpected error occured: Error in the application.");

            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
