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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
                        Type = "clinical-review",
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
                        Type = "clinical-review",
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

            var (errors, successfulPaths) = await this.WorkflowValidator.ValidateWorkflow(workflow);

            Assert.True(errors.Count > 0);

            Assert.Equal(21, errors.Count);

            var successPath = "rootTask => taskSucessdesc1 => taskSucessdesc2";
            Assert.Contains(successPath, successfulPaths);

            var expectedConvergenceError = "Detected task convergence on path: rootTask => taskdesc1 => taskdesc2 => test-argo-task => test-clinical-review => test-clinical-review-2 => example-task => ∞";
            Assert.Contains(expectedConvergenceError, errors);

            var unreferencedTaskError = "Found Task(s) without any task destinations to it: taskdesc3,task_de.sc3?";
            Assert.Contains(unreferencedTaskError, errors);

            var loopingTasksError = "Detected task convergence on path: rootTask => taskLoopdesc1 => taskLoopdesc2 => taskLoopdesc3 => taskLoopdesc4 => ∞";
            Assert.Contains(loopingTasksError, errors);

            var missingDestinationError = "Missing destination DoesNotExistDestination in task taskLoopdesc4";
            Assert.Contains(missingDestinationError, errors);

            var invalidTaskId = "TaskId: task_de.sc3? Contains Invalid Characters.";
            Assert.Contains(invalidTaskId, errors);

            var duplicateOutputArtifactName = "Task: \"rootTask\" has multiple output names with the same value.\n";
            Assert.Contains(duplicateOutputArtifactName, errors);

            var duplicateWorkflowName = $"A Workflow with the name: {workflow.Name} already exists.";
            Assert.Contains(duplicateWorkflowName, errors);

            var missingArgoArgs = "Required parameter to execute Argo workflow is missing: queue_name, workflow_name, reviewed_task_id";
            Assert.Contains(missingArgoArgs, errors);

            var incorrectClinicalReviewValueFormat = $"Invalid Value property on input artifact Invalid Value Format in task: test-clinical-review. Incorrect format.";
            Assert.Contains(incorrectClinicalReviewValueFormat, errors);

            var selfReferencingClinicalReviewValue = $"Invalid Value property on input artifact Self Referencing Task Id in task: test-clinical-review. Self referencing task ID.";
            Assert.Contains(selfReferencingClinicalReviewValue, errors);

            var nonExistingClinicalReviewValueId = $"Invalid input artifact 'No Matching Task Id' in task 'test-clinical-review': No matching task for ID 'a-random-string'";
            Assert.Contains(nonExistingClinicalReviewValueId, errors);

            WorkflowValidator.Reset();
        }

        [Fact]
        public async Task ValidateWorkflow_ValidatesEmptyWorkflow_ReturnsErrorsAndHasCorrectValidationResultsAsync()
        {
            for (var i = 0; i < 15; i++)
            {
                var workflow = new Workflow();

                _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                    .ReturnsAsync(new WorkflowRevision());

                var (errors, _) = await this.WorkflowValidator.ValidateWorkflow(workflow);

                Assert.True(errors.Count > 0);

                Assert.Equal(8, errors.Count);

                var error1 = "'' is not a valid Workflow Description (source: Unnamed workflow).";
                Assert.Contains(error1, errors);

                var error2 = "'informaticsGateway' cannot be null (source: Unnamed workflow).";
                Assert.Contains(error2, errors);

                var error3 = "'' is not a valid AE Title (source: informaticsGateway).";
                Assert.Contains(error3, errors);

                var error4 = "'' is not a valid Informatics Gateway - exportDestinations (source: informaticsGateway).";
                Assert.Contains(error4, errors);

                var error5 = "Missing Workflow Name.";
                Assert.Contains(error5, errors);

                var error6 = "Missing Workflow Version.";
                Assert.Contains(error6, errors);

                var error7 = "Missing Workflow Tasks.";
                Assert.Contains(error7, errors);

                var error8 = "A Workflow with the name:  already exists.";
                Assert.Contains(error8, errors);

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
