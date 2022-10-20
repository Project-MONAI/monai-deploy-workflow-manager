using System;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Validators;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Test.Validators
{
    public class WorkflowValidatorTests
    {
        public WorkflowValidator WorkflowValidator { get; set; }

        private readonly Mock<IWorkflowService> _workflowService;

        public WorkflowValidatorTests()
        {
            _workflowService = new Mock<IWorkflowService>();
            this.WorkflowValidator = new WorkflowValidator(_workflowService.Object);
        }

        [Fact]
        public async void ValidateWorkflow_ValidatesAWorkflow_ReturnsErrorsAndHasCorrectValidationResultsAsync()
        {
            var workflow = new Workflow
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
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new WorkflowRevision());

            for (var i = 0; i < 15; i++)
            {
                var workflowValidationResult = await this.WorkflowValidator.ValidateWorkflow(workflow);

                Assert.True(workflowValidationResult.Errors.Count > 0);

                Assert.Equal(16, workflowValidationResult.Errors.Count);

                var successPath = "rootTask => taskSucessdesc1 => taskSucessdesc2";
                Assert.Contains(successPath, workflowValidationResult.SuccessfulPaths);

                var expectedConvergenceError = "Detected task convergence on path: rootTask => taskdesc1 => taskdesc2 => ∞";
                Assert.Contains(expectedConvergenceError, workflowValidationResult.Errors);

                var unreferencedTaskError = "Found Task(s) without any task destinations to it: taskdesc3,task_de.sc3?";
                Assert.Contains(unreferencedTaskError, workflowValidationResult.Errors);

                var loopingTasksError = "Detected task convergence on path: rootTask => taskLoopdesc1 => taskLoopdesc2 => taskLoopdesc3 => taskLoopdesc4 => ∞";
                Assert.Contains(loopingTasksError, workflowValidationResult.Errors);

                var missingDestinationError = "Missing destination DoesNotExistDestination in task taskLoopdesc4";
                Assert.Contains(missingDestinationError, workflowValidationResult.Errors);

                var invalidTaskId = "TaskId: task_de.sc3? Contains Invalid Characters.";
                Assert.Contains(invalidTaskId, workflowValidationResult.Errors);

                var duplicateOutputArtifactName = "Task: \"rootTask\" has multiple output names with the same value.\n";
                Assert.Contains(duplicateOutputArtifactName, workflowValidationResult.Errors);

                var duplicateWorkflowName = $"A Workflow with the name: {workflow.Name} already exists.";
                Assert.Contains(duplicateWorkflowName, workflowValidationResult.Errors);

                WorkflowValidator.Reset();
            }
        }

        [Fact]
        public async void ValidateWorkflow_ValidatesEmptyWorkflow_ReturnsErrorsAndHasCorrectValidationResultsAsync()
        {
            for (var i = 0; i < 15; i++)
            {
                var workflow = new Workflow();

                _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                    .ReturnsAsync(new WorkflowRevision());

                var results = await this.WorkflowValidator.ValidateWorkflow(workflow);

                Assert.True(results.Errors.Count > 0);

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

        [Fact]
        public async void ValidateWorkflow_ValidateWorkflow_ReturnsNoErrors()
        {
            var workflow = new Workflow
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
                    new TaskObject
                    {
                        Id = "rootTask",
                        Type = "type",
                        Description = "TestDesc",
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "taskdesc1"
                            },
                            new TaskDestination
                            {
                                Name = "taskSucessdesc1"
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
                                }
                            }
                        }
                    },
                    #region SuccessfulTasksPath
                    new TaskObject
                    {
                        Id = "taskSucessdesc1",
                        Type = "type",
                        Description = "TestDesc",
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "taskSucessdesc2"
                            }
                        }
                    },
                    new TaskObject
                    {
                        Id = "taskSucessdesc2",
                        Type = "type",
                        Description = "TestDesc",
                    },
                    #endregion
                    new TaskObject
                    {
                        Id = "taskdesc1",
                        Type = "type",
                        Description = "TestDesc",
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "taskdesc2"
                            }
                        }
                    },
                    new TaskObject
                    {
                        Id = "taskdesc2",
                        Type = "type",
                        Description = "TestDesc",
                        TaskDestinations = new TaskDestination[] { }
                    }
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            for (var i = 0; i < 15; i++)
            {
                var results = await this.WorkflowValidator.ValidateWorkflow(workflow);

                Assert.True(results.Errors.Count == 0);

                WorkflowValidator.Reset();
            }
        }
    }
}
