/*
 * Copyright 2021-2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Contracts.Responses;
using Monai.Deploy.WorkflowManager.Controllers;
using Monai.Deploy.WorkflowManager.Services;
using Monai.Deploy.WorkflowManager.Validators;
using Monai.Deploy.WorkflowManager.Wrappers;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Test.Controllers
{
    public class WorkflowsControllerTests
    {
        private WorkflowsController WorkflowsController { get; set; }

        private readonly Mock<IWorkflowService> _workflowService;
        private readonly Mock<ILogger<WorkflowsController>> _logger;
        private readonly Mock<IUriService> _uriService;
        private readonly IOptions<WorkflowManagerOptions> _options;

        public WorkflowsControllerTests()
        {
            _options = Options.Create(new WorkflowManagerOptions());
            _workflowService = new Mock<IWorkflowService>();
            _logger = new Mock<ILogger<WorkflowsController>>();
            _uriService = new Mock<IUriService>();

            WorkflowsController = new WorkflowsController(_workflowService.Object, _logger.Object, _uriService.Object, _options);
        }

        [Fact]
        public async void GetList_WorkflowsExist_ReturnsList()
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
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                }
            };

            _workflowService.Setup(w => w.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(workflows);
            _workflowService.Setup(w => w.CountAsync()).ReturnsAsync(workflows.Count);
            _uriService.Setup(s => s.GetPageUriString(It.IsAny<Filter.PaginationFilter>(), It.IsAny<string>())).Returns(() => "unitTest");

            var result = await WorkflowsController.GetList(new Filter.PaginationFilter());

            var objectResult = Assert.IsType<OkObjectResult>(result);

            var responseValue = (PagedResponse<List<WorkflowRevision>>)objectResult.Value;
            responseValue.Data.Should().BeEquivalentTo(workflows);
            responseValue.FirstPage.Should().Be("unitTest");
            responseValue.LastPage.Should().Be("unitTest");
            responseValue.PageNumber.Should().Be(1);
            responseValue.PageSize.Should().Be(10);
            responseValue.TotalPages.Should().Be(1);
            responseValue.TotalRecords.Should().Be(1);
            responseValue.Succeeded.Should().Be(true);
            responseValue.PreviousPage.Should().Be(null);
            responseValue.NextPage.Should().Be(null);
            responseValue.Errors.Should().BeNullOrEmpty();
        }

        [Fact]
        public async void GetList_ServiceException_ReturnProblem()
        {
            _workflowService.Setup(w => w.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>())).ThrowsAsync(new Exception());

            var result = await WorkflowsController.GetList(new Filter.PaginationFilter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);

            const string expectedInstance = "/workflows";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
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

            const string expectedInstance = "/workflows";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);

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

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(404, objectResult.StatusCode);

            const string expectedInstance = "/workflows";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
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
            Assert.Equal("Failed to validate id, workflow not found", result.As<ObjectResult>().Value.As<ProblemDetails>().Detail);

            Assert.Equal(404, objectResult.StatusCode);

            const string expectedInstance = "/workflows";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task DeleteAsync_WorkflowsServiceThrowsException_Should500Error()
        {
            var workflowRevisionId = Guid.NewGuid().ToString();

            _workflowService.Setup(w => w.GetAsync(It.IsAny<string>()))
                .Throws(new ApplicationException());

            var result = await WorkflowsController.DeleteAsync(workflowRevisionId);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal("Unexpected error occurred: Error in the application.", result.As<ObjectResult>().Value.As<ProblemDetails>().Detail);

            Assert.Equal(500, objectResult.StatusCode);

            const string expectedInstance = "/workflows";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public async Task DeleteAsync_WorkflowsGivenInvalidId_ShouldBadRequest()
        {
            var invalidId = "1";

            var result = await WorkflowsController.DeleteAsync(invalidId);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal("Failed to validate id, not a valid guid", result.As<ObjectResult>().Value.As<ProblemDetails>().Detail);

            Assert.Equal(400, objectResult.StatusCode);

            const string expectedInstance = "/workflows";
            Assert.StartsWith(expectedInstance, ((ProblemDetails)objectResult.Value).Instance);
        }

        [Fact]
        public void ValidateWorkflow_ValidatesAWorkflow_ReturnsTrueAndHasCorrectValidationResultsAsync()
        {
            for (var i = 0; i < 15; i++)
            {
                var workflow =
                new WorkflowRevision
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
                            AeTitle = "aetitle",
                            ExportDestinations = new string[] { "oneDestination", "twoDestination", "threeDestination" }
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = "rootTask",
                                Type = "type",
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination
                                    {
                                        Name = "taskdesc1"
                                    },
                                    new TaskDestination
                                    {
                                        Name = "taskSucessdesc1"
                                    },
                                    new TaskDestination
                                    {
                                        Name = "taskLoopdesc1"
                                    }
                                },
                                ExportDestinations = new ExportDestination[]
                                {
                                    new ExportDestination { Name = "oneDestination" },
                                    new ExportDestination { Name = "twoDestination" },
                                },
                                Artifacts = new ArtifactMap
                                {
                                    Output = new Artifact[]
                                    {
                                        new Artifact
                                        {
                                            Name = "non_unique_artifact",
                                            Mandatory = true,
                                            Value = "Example Value"
                                        },
                                        new Artifact
                                        {
                                            Name = "non_unique_artifact",
                                            Mandatory = true,
                                            Value = "Example Value"
                                        }
                                    }
                                }
                            },
                            #region LoopingTasks
                            new TaskObject {
                                Id = "taskLoopdesc4",
                                Type = "type",
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination
                                    {
                                        Name = "taskLoopdesc2"
                                    }
                                },
                                ExportDestinations = new ExportDestination[]
                                {
                                    new ExportDestination { Name = "threeDestination" },
                                    new ExportDestination { Name = "twoDestination" },
                                    new ExportDestination { Name = "DoesNotExistDestination" },
                                }
                            },
                            new TaskObject {
                                Id = "taskLoopdesc3",
                                Type = "type",
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination
                                    {
                                        Name = "taskLoopdesc4"
                                    }
                                }
                            },
                                new TaskObject {
                                Id = "taskLoopdesc2",
                                Type = "type",
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination
                                    {
                                        Name = "taskLoopdesc3"
                                    }
                                }
                            },
                            new TaskObject {
                                Id = "taskLoopdesc1",
                                Type = "type",
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination
                                    {
                                        Name = "taskLoopdesc2"
                                    }
                                }
                            },
                            #endregion
                            #region SuccessfulTasksPath
                            new TaskObject {
                                Id = "taskSucessdesc1",
                                Type = "type",
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination
                                    {
                                        Name = "taskSucessdesc2"
                                    }
                                }
                            },
                            new TaskObject {
                                Id = "taskSucessdesc2",
                                Type = "type",
                            },
                            #endregion
                            #region SelfReferencingTasks
                            new TaskObject {
                                Id = "taskdesc1",
                                Type = "type",
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination
                                    {
                                        Name = "taskdesc2"
                                    }
                                }
                            },
                            new TaskObject {
                                Id = "taskdesc2",
                                Type = "type",
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination
                                    {
                                        Name = "taskdesc2"
                                    }
                                }
                            },
                            #endregion
                            // Unreferenced task 
                            new TaskObject {
                                Id = "taskdesc3",
                                Type = "type",
                                Description = "taskdesc",
                            },
                            new TaskObject {
                                Id = "task_de.sc3?",
                                Type = "type",
                                Description = "invalidid",
                            }
                        }
                    }
                };

                var workflowHasErrors = WorkflowValidator.ValidateWorkflow(workflow.Workflow, out var results);

                Assert.True(workflowHasErrors);

                Assert.Equal(15, results.Errors.Count);

                var successPath = "rootTask => taskSucessdesc1 => taskSucessdesc2";
                Assert.Contains(successPath, results.SuccessfulPaths);

                var expectedConvergenceError = "Detected task convergence on path: rootTask => taskdesc1 => taskdesc2 => ∞";
                Assert.Contains(expectedConvergenceError, results.Errors);

                var unreferencedTaskError = "Found Task(s) without any task destinations to it: taskdesc3,task_de.sc3?";
                Assert.Contains(unreferencedTaskError, results.Errors);

                var loopingTasksError = "Detected task convergence on path: rootTask => taskLoopdesc1 => taskLoopdesc2 => taskLoopdesc3 => taskLoopdesc4 => ∞";
                Assert.Contains(loopingTasksError, results.Errors);

                var missingDestinationError = "Missing destination DoesNotExistDestination in task taskLoopdesc4";
                Assert.Contains(missingDestinationError, results.Errors);

                var invalidTaskId = "TaskId: task_de.sc3? Contains Invalid Characters.";
                Assert.Contains(invalidTaskId, results.Errors);

                var duplicateOutputArtifactName = "Task: \"rootTask\" has multiple output names with the same value.\n";
                Assert.Contains(duplicateOutputArtifactName, results.Errors);

                WorkflowValidator.Reset();
            }
        }

        [Fact]
        public void ValidateWorkflow_ValidatesEmptyWorkflow_ReturnsTrueAndHasCorrectValidationResultsAsync()
        {
            for (var i = 0; i < 15; i++)
            {
                var workflow = new Workflow();

                var workflowHasErrors = WorkflowValidator.ValidateWorkflow(workflow, out var results);

                Assert.True(workflowHasErrors);

                Assert.Equal(7, results.Errors.Count);

                var error1 = "'' is not a valid Workflow Description (source: Unnamed workflow).";
                Assert.Contains(error1, results.Errors);

                var error2 = "'informaticsGateway' cannot be null (source: Unnamed workflow).";
                Assert.Contains(error2, results.Errors);

                var error3 = "'' is not a valid AE Title (source: informaticsGateway).";
                Assert.Contains(error3, results.Errors);

                var error4 = "'' is not a valid Informatics Gateway - exportDestinations (source: informaticsGateway).";
                Assert.Contains(error4, results.Errors);

                var error5 = "Missing Workflow Name.";
                Assert.Contains(error5, results.Errors);

                var error6 = "Missing Workflow Version.";
                Assert.Contains(error6, results.Errors);

                var error7 = "Missing Workflow Tasks.";
                Assert.Contains(error7, results.Errors);

                WorkflowValidator.Reset();
            }
        }
    }
}
