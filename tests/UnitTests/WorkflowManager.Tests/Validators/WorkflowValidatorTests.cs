/*
 * Copyright 2022 MONAI Consortium
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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Validators;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Test.Validators
{
    public class WorkflowValidatorTests
    {
        private readonly Mock<IWorkflowService> _workflowService;
        private readonly WorkflowValidator _workflowValidator;
        private readonly Mock<ILogger<WorkflowValidator>> _logger;

        public WorkflowValidatorTests()
        {
            _logger = new Mock<ILogger<WorkflowValidator>>();

            _workflowService = new Mock<IWorkflowService>();

            _workflowValidator = new WorkflowValidator(_workflowService.Object, _logger.Object);
            _logger = new Mock<ILogger<WorkflowValidator>>();
        }

        [Fact]
        public async Task ValidateWorkflow_ValidatesAWorkflow_ReturnsErrorsAndHasCorrectValidationResultsAsync()
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

                    #endregion LoopingTasks

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

                    #endregion SuccessfulTasksPath

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
                                Name = "test-argo-task"
                            },
                        }
                    },
                    new TaskObject
                    {
                        Id = "test-argo-task",
                        Type = "argo",
                        Description = "Test Argo Task",
                        Args = {
                            { "example", "value" }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "test-clinical-review"
                            }
                        }
                    },
                    new TaskObject
                    {
                        Id = "test-clinical-review",
                        Type = "aide_clinical_review",
                        Description = "Test Clinical Review Task",
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]
                            {
                                new Artifact
                                {
                                    Name = "Invalid Value Format",
                                    Value = "{{ wrong.format }}"
                                },
                                new Artifact
                                {
                                    Name = "Self Referencing Task Id",
                                    Value = "{{ context.value.test-clinical-review.artifact.test }}"
                                },
                                new Artifact
                                {
                                    Name = "No Matching Task Id",
                                    Value = "{{ context.value.a-random-string.artifact.test }}"
                                }
                            }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "test-clinical-review-2"
                            }
                        }
                    },
                    new TaskObject
                    {
                        Id = "test-clinical-review-2",
                        Type = "aide_clinical_review",
                        Description = "Test Clinical Review Task 2",
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "example-task"
                            }
                        }
                    },
                    new TaskObject
                    {
                        Id = "example-task",
                        Type = "type",
                        Description = "taskdesc",
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "example-task"
                            }
                        }
                    },

                    #endregion SelfReferencingTasks

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

            var (errors, successfulPaths) = await _workflowValidator.ValidateWorkflow(workflow);

            Assert.True(errors.Count > 0);

            Assert.Equal(33, errors.Count);

            var successPath = "rootTask => taskSucessdesc1 => taskSucessdesc2";
            Assert.Contains(successPath, successfulPaths);

            var expectedConvergenceError = "Detected task convergence on path: rootTask => taskdesc1 => taskdesc2 => test-argo-task => test-clinical-review => test-clinical-review-2 => example-task => ∞";
            Assert.Contains(expectedConvergenceError, errors);

            var unreferencedTaskError = "Found Task(s) without any task destinations to it: taskdesc3,task_de.sc3?";
            Assert.Contains(unreferencedTaskError, errors);

            var loopingTasksError = "Detected task convergence on path: rootTask => taskLoopdesc1 => taskLoopdesc2 => taskLoopdesc3 => taskLoopdesc4 => ∞";
            Assert.Contains(loopingTasksError, errors);

            var missingDestinationError = "Task: 'taskLoopdesc4' export_destination: 'DoesNotExistDestination' must be registered in the informatics_gateway object.";
            Assert.Contains(missingDestinationError, errors);

            var invalidTaskId = "TaskId: 'task_de.sc3?' Contains Invalid Characters.";
            Assert.Contains(invalidTaskId, errors);

            var duplicateOutputArtifactName = "Task: 'rootTask' has multiple output names with the same value.";
            Assert.Contains(duplicateOutputArtifactName, errors);

            var duplicateWorkflowName = $"Workflow with name 'Workflowname1' already exists, please review.";
            Assert.Contains(duplicateWorkflowName, errors);

            var missingClinicalReviewArgs1 = "Task: 'test-clinical-review' application_name must be specified.";
            Assert.Contains(missingClinicalReviewArgs1, errors);

            var missingClinicalReviewArgs2 = "Task: 'test-clinical-review' reviewed_task_id must be specified.";
            Assert.Contains(missingClinicalReviewArgs2, errors);

            var missingClinicalReviewArgs3 = "Task: 'test-clinical-review' application_version must be specified.";
            Assert.Contains(missingClinicalReviewArgs3, errors);

            var missingClinicalReviewArgs4 = "Task: 'test-clinical-review' mode is incorrectly specified, please specify 'QA', 'Research' or 'Clinical'";
            Assert.Contains(missingClinicalReviewArgs4, errors);

            var missingArgoArgs = "Task: 'test-argo-task' workflow_template_name must be specified, this corresponds to an Argo template name.";
            Assert.Contains(missingArgoArgs, errors);

            var incorrectClinicalReviewValueFormat = $"Invalid Value property on input artifact 'Invalid Value Format' in task: 'test-clinical-review'. Incorrect format.";
            Assert.Contains(incorrectClinicalReviewValueFormat, errors);

            var selfReferencingClinicalReviewValue = $"Invalid Value property on input artifact 'Self Referencing Task Id' in task: 'test-clinical-review'. Self referencing task ID.";
            Assert.Contains(selfReferencingClinicalReviewValue, errors);

            var nonExistingClinicalReviewValueId = $"Invalid input artifact 'No Matching Task Id' in task 'test-clinical-review': No matching task for ID 'a-random-string'";
            Assert.Contains(nonExistingClinicalReviewValueId, errors);

            var missingArtifactsClinicalReview = $"Task: 'test-clinical-review-2' must have Input Artifacts specified.";
            Assert.Contains(missingArtifactsClinicalReview, errors);
        }

        [Fact]
        public async Task ValidateWorkflow_ValidatesEmptyWorkflow_ReturnsErrorsAndHasCorrectValidationResultsAsync()
        {
            var workflow = new Workflow();

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new WorkflowRevision());

            _workflowValidator.OrignalName = "pizza";
            var (errors, _) = await _workflowValidator.ValidateWorkflow(workflow);

            Assert.True(errors.Count > 0);

            Assert.Equal(4, errors.Count);

            var error1 = "Missing InformaticsGateway section.";
            Assert.Contains(error1, errors);

            var error2 = "Missing Workflow Name.";
            Assert.Contains(error2, errors);

            var error3 = "Missing Workflow Version.";
            Assert.Contains(error3, errors);

            var error4 = "Workflow does not contain Tasks, please review Workflow.";
            Assert.Contains(error4, errors);
        }

        [Fact]
        public async Task ValidateWorkflow_ValidateWorkflow_ReturnsNoErrors()
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
                        Type = "router",
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
                        Type = "router",
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
                        Type = "router",
                        Description = "TestDesc",
                    },

                    #endregion SuccessfulTasksPath

                    new TaskObject
                    {
                        Id = "taskdesc1",
                        Type = "router",
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
                        Type = "router",
                        Description = "TestDesc",
                        TaskDestinations = new TaskDestination[] { }
                    }
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            for (var i = 0; i < 15; i++)
            {
                var (errors, _) = await _workflowValidator.ValidateWorkflow(workflow);

                Assert.True(errors.Count == 0);
            }
        }
    }
}
