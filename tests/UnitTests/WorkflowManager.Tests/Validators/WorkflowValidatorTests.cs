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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Services.InformaticsGateway;
using Monai.Deploy.WorkflowManager.Common.Validators;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.Test.Validators
{
    public class WorkflowValidatorTests
    {
        private readonly Mock<IWorkflowService> _workflowService;
        private readonly Mock<IInformaticsGatewayService> _informaticsGatewayService;
        private readonly WorkflowValidator _workflowValidator;
        private readonly Mock<ILogger<WorkflowValidator>> _logger;

        private readonly IOptions<WorkflowManagerOptions> _options;

        public WorkflowValidatorTests()
        {
            _logger = new Mock<ILogger<WorkflowValidator>>();

            _workflowService = new Mock<IWorkflowService>();
            _informaticsGatewayService = new Mock<IInformaticsGatewayService>();
            _options = Options.Create(
                new WorkflowManagerOptions
                {
                    DicomTagsDisallowed = "PatientName,PatientID,IssuerOfPatientID,TypeOfPatientID,IssuerOfPatientIDQualifiersSequence,SourcePatientGroupIdentificationSequence,GroupOfPatientsIdentificationSequence,SubjectRelativePositionInImage,PatientBirthDate,PatientBirthTime,PatientBirthDateInAlternativeCalendar,PatientDeathDateInAlternativeCalendar,PatientAlternativeCalendar,PatientSex,PatientInsurancePlanCodeSequence,PatientPrimaryLanguageCodeSequence,PatientPrimaryLanguageModifierCodeSequence,QualityControlSubject,QualityControlSubjectTypeCodeSequence,StrainDescription,StrainNomenclature,StrainStockNumber,StrainSourceRegistryCodeSequence,StrainStockSequence,StrainSource,StrainAdditionalInformation,StrainCodeSequence,GeneticModificationsSequence,GeneticModificationsDescription,GeneticModificationsNomenclature,GeneticModificationsCodeSequence,OtherPatientIDsRETIRED,OtherPatientNames,OtherPatientIDsSequence,PatientBirthName,PatientAge,PatientSize,PatientSizeCodeSequence,PatientBodyMassIndex,MeasuredAPDimension,MeasuredLateralDimension,PatientWeight,PatientAddress,InsurancePlanIdentificationRETIRED,PatientMotherBirthName,MilitaryRank,BranchOfService,MedicalRecordLocatorRETIRED,ReferencedPatientPhotoSequence,MedicalAlerts,Allergies,CountryOfResidence,RegionOfResidence,PatientTelephoneNumbers,PatientTelecomInformation,EthnicGroup,Occupation,SmokingStatus,AdditionalPatientHistory,PregnancyStatus,LastMenstrualDate,PatientReligiousPreference,PatientSpeciesDescription,PatientSpeciesCodeSequence,PatientSexNeutered,AnatomicalOrientationType,PatientBreedDescription,PatientBreedCodeSequence,BreedRegistrationSequence,BreedRegistrationNumber,BreedRegistryCodeSequence,ResponsiblePerson,ResponsiblePersonRole,ResponsibleOrganization,PatientComments,ExaminedBodyThickness"
                }
            );

            _workflowValidator = new WorkflowValidator(_workflowService.Object, _informaticsGatewayService.Object, _logger.Object, _options);
            _logger = new Mock<ILogger<WorkflowValidator>>();
        }

        [Fact]
        public async Task ValidateWorkflow_ValidatesAWorkflow_ReturnsErrorsAndHasCorrectValidationResultsAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var workflow = new Workflow
            {
                Name = "Workflowname1",
                Description = "Workflowdesc1",
                Version = "1",
                InformaticsGateway = new InformaticsGateway
                {
                    AeTitle = "aetitle",
                    ExportDestinations = new string[] { "oneDestination", "twoDestination", "threeDestination" },
                    DataOrigins = new string[] { "invalid_origin" }
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
                            Output = new OutputArtifact[]
                            {
                                new OutputArtifact
                                {
                                    Name = "non_unique_artifact",
                                    Mandatory = true,
                                    Value = "Example Value"
                                },
                                new OutputArtifact
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
                            { "cpu", "0.1" },
                            { "memory", "0.1" },
                            { "gpu_required", "2" }
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
                        Args = {
                            { "reviewed_task_id", "taskLoopdesc4" }
                        },
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
                                },
                                new Artifact
                                {
                                    Name = "Non Argo Artifact",
                                    Value = "{{ context.value.rootTask.artifact.non_unique_artifact }}"
                                },
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
                        Id = "test-clinical-review-3",
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
                                },
                                new Artifact
                                {
                                    Name = "Non Argo Artifact",
                                    Value = "{{ context.value.rootTask.artifact.non_unique_artifact }}"
                                },
                            }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
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
                            },
                            new TaskDestination
                            {
                                Name = "test-clinical-review-3"
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
                            },
                            new TaskDestination
                            {
                                Name = "invalid-key-argo-task"
                            },
                            new TaskDestination
                            {
                                Name = "EmailTask_MissingEmailAndRolesArgs"
                            }
                        }
                    },

                    #endregion SelfReferencingTasks

                    new TaskObject
                    {
                        Id = "invalid-key-argo-task",
                        Type = "argo",
                        Description = "Invalid Key Argo Task",
                        Args = {
                            { "invalid_key", "value" },
                            { "workflow_template_name" ,"spot"},
                            { "cpu", "1" },
                            { "memory", "1" },
                            { "gpu_required", "1" }
                        }
                    },

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
                    },

                    // email tasks
                    new TaskObject
                    {
                        Id = "EmailTask_MissingEmailAndRolesArgs",
                        Type = "email",
                        Args =
                        {
                            { "metadata_values", "Status" }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "EmailTask_MissingMetadataArg"
                            }
                        },
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]
                            {
                                new Artifact
                                {
                                    Name = "ExampleArtifact",
                                    Value = "{{ context.value.EmailTask_InvalidMetadataValues.artifact.test }}"
                                },
                            }
                        }
                    },
                    new TaskObject
                    {
                        Id = "EmailTask_MissingMetadataArg",
                        Type = "email",
                        Args =
                        {
                            { "recipient_emails", "test@email.com" },
                            { "recipient_roles", "admin" }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "EmailTask_NullEmailArg"
                            }
                        },
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]
                            {
                                new Artifact
                                {
                                    Name = "ExampleArtifact",
                                    Value = "{{ context.value.EmailTask_InvalidMetadataValues.artifact.test }}"
                                },
                            }
                        }
                    },
                    new TaskObject
                    {
                        Id = "EmailTask_NullEmailArg",
                        Type = "email",
                        Args =
                        {
                            { "recipient_emails", null },
                            { "metadata_values", "Status" }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "EmailTask_NullRolesArg"
                            }
                        },
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]
                            {
                                new Artifact
                                {
                                    Name = "ExampleArtifact",
                                    Value = "{{ context.value.EmailTask_InvalidMetadataValues.artifact.test }}"
                                },
                            }
                        }
                    },
                    new TaskObject
                    {
                        Id = "EmailTask_NullRolesArg",
                        Type = "email",
                        Args =
                        {
                            { "recipient_roles", null },
                            { "metadata_values", "Status" }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "EmailTask_NullMetadataArg"
                            }
                        },
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]
                            {
                                new Artifact
                                {
                                    Name = "ExampleArtifact",
                                    Value = "{{ context.value.EmailTask_InvalidMetadataValues.artifact.test }}"
                                },
                            }
                        }
                    },
                    new TaskObject
                    {
                        Id = "EmailTask_NullMetadataArg",
                        Type = "email",
                        Args =
                        {
                            { "recipient_emails", "test@email.com" },
                            { "recipient_roles", "admin" },
                            { "metadata_values", null }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "EmailTask_InvalidEmailsArg"
                            }
                        },
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]
                            {
                                new Artifact
                                {
                                    Name = "ExampleArtifact",
                                    Value = "{{ context.value.EmailTask_InvalidMetadataValues.artifact.test }}"
                                },
                            }
                        }
                    },
                    new TaskObject
                    {
                        Id = "EmailTask_InvalidEmailsArg",
                        Type = "email",
                        Args =
                        {
                            { "recipient_emails", "test.email.com.,invalid.email," },
                            { "metadata_values", "Status" }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "EmailTask_DisallowedMetadataValues"
                            }
                        },
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]
                            {
                                new Artifact
                                {
                                    Name = "ExampleArtifact",
                                    Value = "{{ context.value.EmailTask_InvalidMetadataValues.artifact.test }}"
                                },
                            }
                        }
                    },
                    new TaskObject
                    {
                        Id = "EmailTask_DisallowedMetadataValues",
                        Type = "email",
                        Args =
                        {
                            { "recipient_emails", "test@email.com" },
                            { "metadata_values", "PatientID,PatientName" }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination
                            {
                                Name = "EmailTask_InvalidMetadataValues"
                            }
                        },
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]
                            {
                                new Artifact
                                {
                                    Name = "ExampleArtifact",
                                    Value = "{{ context.value.EmailTask_InvalidMetadataValues.artifact.test }}"
                                },
                            }
                        }
                    },
                    new TaskObject
                    {
                        Id = "EmailTask_InvalidMetadataValues",
                        Type = "email",
                        Args =
                        {
                            { "recipient_emails", "test@email.com" },
                            { "metadata_values", "InvalidTag1,InvalidTag2" }
                        },
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]
                            {
                                new Artifact
                                {
                                    Name = "ExampleArtifact",
                                    Value = "{{ context.value.EmailTask_InvalidEmailsArg.artifact.test }}"
                                },
                            }
                        }
                    },
                }
            };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new WorkflowRevision());
            _informaticsGatewayService.Setup(w => w.OriginExists(It.IsAny<string>()))
                .ReturnsAsync(false);

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.True(errors.Count > 0);

            Assert.Equal(56, errors.Count);

            var convergingTasksDestinations = "Converging Tasks Destinations in tasks: (test-clinical-review-2, example-task) on task: example-task";
            Assert.Contains(convergingTasksDestinations, errors);

            var convergingTasksDestinations2 = "Converging Tasks Destinations in tasks: (taskLoopdesc4, taskLoopdesc1) on task: taskLoopdesc2";
            Assert.Contains(convergingTasksDestinations2, errors);

            var unreferencedTaskError = "Found Task(s) without any task destinations to it: taskdesc3, task_de.sc3?";
            Assert.Contains(unreferencedTaskError, errors);

            var missingDestinationError = "Task: 'taskLoopdesc4' export_destination: 'DoesNotExistDestination' must be registered in the informatics_gateway object.";
            Assert.Contains(missingDestinationError, errors);

            var invalidTaskId = "TaskId: 'task_de.sc3?' Contains Invalid Characters.";
            Assert.Contains(invalidTaskId, errors);

            var duplicateOutputArtifactName = "Task: 'rootTask' has multiple output names with the same value.";
            Assert.Contains(duplicateOutputArtifactName, errors);

            var duplicateWorkflowName = $"Workflow with name 'Workflowname1' already exists.";
            Assert.Contains(duplicateWorkflowName, errors);

            var missingClinicalReviewArgs1 = "Task: 'test-clinical-review' application_name must be specified.";
            Assert.Contains(missingClinicalReviewArgs1, errors);

            var missingClinicalReviewArgs2 = "Task: 'test-clinical-review-3' reviewed_task_id must be specified.";
            Assert.Contains(missingClinicalReviewArgs2, errors);

            var missingClinicalReviewArgs3 = "Task: 'test-clinical-review' application_version must be specified.";
            Assert.Contains(missingClinicalReviewArgs3, errors);

            var missingClinicalReviewArgs4 = "Task: 'test-clinical-review' mode is incorrectly specified, please specify 'QA', 'Research' or 'Clinical'";
            Assert.Contains(missingClinicalReviewArgs4, errors);

            var missingArgoArgs = "Task: 'test-argo-task' workflow_template_name must be specified, this corresponds to an Argo template name.";
            Assert.Contains(missingArgoArgs, errors);

            var invalidArgoArg1 = "Task: 'test-argo-task' value '0.1' provided for argument 'cpu' is not valid. The value needs to be a whole number greater than 0.";
            Assert.Contains(invalidArgoArg1, errors);

            var invalidArgoArg3 = "Task: 'test-argo-task' value '2' provided for argument 'gpu_required' is not valid. The value needs to be 'true' or 'false'.";
            Assert.Contains(invalidArgoArg3, errors);

            var incorrectClinicalReviewValueFormat = $"Invalid Value property on input artifact 'Invalid Value Format' in task: 'test-clinical-review'. Incorrect format.";
            Assert.Contains(incorrectClinicalReviewValueFormat, errors);

            var selfReferencingClinicalReviewValue = $"Invalid Value property on input artifact 'Self Referencing Task Id' in task: 'test-clinical-review'. Self referencing task ID.";
            Assert.Contains(selfReferencingClinicalReviewValue, errors);

            var nonExistingClinicalReviewValueId = $"Invalid input artifact 'No Matching Task Id' in task 'test-clinical-review': No matching task for ID 'a-random-string'";
            Assert.Contains(nonExistingClinicalReviewValueId, errors);

            var missingArtifactsClinicalReview = $"Task: 'test-clinical-review-2' must have Input Artifacts specified.";
            Assert.Contains(missingArtifactsClinicalReview, errors);

            var nonReviewedTask = "Invalid input artifact 'Non Argo Artifact' in task 'test-clinical-review': Task cannot reference a non-reviewed task artifacts 'rootTask'";
            Assert.Contains(nonReviewedTask, errors);

            var invalidSourceName = "Data origin invalid_origin does not exists. Please review sources configuration management.";
            Assert.Contains(invalidSourceName, errors);

            var invalidArgoKey = $"Task: 'invalid-key-argo-task' args has invalid keys: invalid_key. Please only specify keys from the following list: {string.Join(", ", ArgoParameters.VaildParameters)}.";
            Assert.Contains(invalidArgoKey, errors);

            var emailMissingEmailAndRolesArgs = "No recipients arguments specified for task EmailTask_MissingEmailAndRolesArgs. Email tasks must specify at least one of the following properties: recipient_emails, recipient_roles";
            Assert.Contains(emailMissingEmailAndRolesArgs, errors);

            var emailMissingMetadataArg = "Argument 'metadata_values' for task EmailTask_MissingMetadataArg must be specified";
            Assert.Contains(emailMissingMetadataArg, errors);

            var emailNullEmailsArg = "Argument 'recipient_emails' for task EmailTask_NullEmailArg must be a comma seperated list of email addresses.";
            Assert.Contains(emailNullEmailsArg, errors);

            var emailNullRolesArg = "Argument 'recipient_roles' for task EmailTask_NullRolesArg must be a comma seperated list of roles.";
            Assert.Contains(emailNullRolesArg, errors);

            var emailNullMetadataArg = "Argument 'metadata_values' for task EmailTask_NullMetadataArg must be a comma seperated list of DICOM metadata tag names.";
            Assert.Contains(emailNullMetadataArg, errors);

            var emailInvalidEmailArg = $"Argument 'recipient_emails' for task: EmailTask_InvalidEmailsArg contains email addresses that do not conform to the standard email format:{Environment.NewLine}test.email.com.{Environment.NewLine}invalid.email";
            Assert.Contains(emailInvalidEmailArg, errors);

            var emailDisallowedMetadataValuesArg = $"Argument 'metadata_values' for task EmailTask_DisallowedMetadataValues contains the following values that are not permitted:{Environment.NewLine}PatientID{Environment.NewLine}PatientName";
            Assert.Contains(emailDisallowedMetadataValuesArg, errors);

            var emailInvalidMetadataValuesArg = $"Argument 'metadata_values' for task EmailTask_InvalidMetadataValues has the following invalid DICOM tags:{Environment.NewLine}InvalidTag1{Environment.NewLine}InvalidTag2";
            Assert.Contains(emailInvalidMetadataValuesArg, errors);
        }

        [Fact]
        public async Task ValidateWorkflow_ValidatesEmptyWorkflow_ReturnsErrorsAndHasCorrectValidationResultsAsync()
        {
            var workflow = new Workflow();

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new WorkflowRevision());

            _workflowValidator.OrignalName = "pizza";
            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

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

        //"metadata_values": "Study Instance UID, Series Instance UID"
        [Fact]
        public async Task ValidateWorkflow_ValidateWorkflow_WithPluginArgs_ReturnsNoErrors()
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
                                Name = "EmailTask"
                            }
                        },
                        ExportDestinations = new ExportDestination[]
                        {
                            new ExportDestination { Name = "oneDestination" },
                            new ExportDestination { Name = "twoDestination" },
                        },
                        Artifacts = new ArtifactMap
                        {
                            Output = new OutputArtifact[]
                            {
                                new OutputArtifact
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

                        Id = "EmailTask",
                        Type = "email",
                        Description = "Email plugin Task 1",
                        Artifacts = new ArtifactMap{ Input = new Artifact[] {
                                new Artifact
                                {
                                    Name = "ExampleArtifact",
                                    Value = "{{ context.value.EmailTask_InvalidEmailsArg.artifact.test }}"
                                },
                        } },
                        Args = new Dictionary<string, string>{
                            { "metadata_values", "Study Instance UID, Series Instance UID"},
                            { "workflow_name", "Workflow Name"},
                            { "recipient_emails", "neil.south@blah.com"}
                        },
                        TaskDestinations = new TaskDestination[] {
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
                        TaskDestinations = Array.Empty<TaskDestination>()
                    }
                    #endregion SuccessfulTasksPath
                }
            };

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.True(errors.Count == 0);

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
                            Output = new OutputArtifact[]
                            {
                                new OutputArtifact
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
                        TaskDestinations = Array.Empty<TaskDestination>()
                    }
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            for (var i = 0; i < 15; i++)
            {
                var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

                Assert.True(errors.Count == 0);
            }
        }

        [Fact]
        public async Task ValidateWorkflow_Incorrect_podPriorityClassName_ReturnsErrors()
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
                        Args = new System.Collections.Generic.Dictionary<string, string>{
                            { "priority" ,"god" },
                            { "workflow_template_name" ,"spot"}
                        },
                        Id = "rootTask",
                        Type = "argo",
                        Description = "TestDesc",
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]{
                                new Artifact
                                {
                                    Name = "non_unique_artifact",
                                    Value = "Example Value"
                                }
                            }
                        }
                    },
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.Single(errors);
        }

        [Fact]
        public async Task ValidateWorkflow_correct_podPriorityClassName_ReturnsNoErrors()
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
                        Args = new System.Collections.Generic.Dictionary<string, string>{
                            { "priority" ,"high" },
                            { "workflow_template_name" ,"spot"}
                        },
                        Id = "rootTask",
                        Type = "argo",
                        Description = "TestDesc",
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]{
                                new Artifact
                                {
                                    Name = "non_unique_artifact",
                                    Value = "Example Value"
                                }
                            }
                        }
                    },
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateWorkflow_ExportAppWithoutDestination_ReturnsErrors()
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
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]{
                                new Artifact
                                {
                                    Name = "non_unique_artifact",
                                    Value = "Example Value"
                                }
                            }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination{ Name = "externalTask" }
                        }
                    },
                    new TaskObject
                    {
                        Id = "externalTask",
                        Type = "remote_app_execution",
                        //ExportDestinations = new ExportDestination[]
                        //{
                        //    new ExportDestination { Name = "oneDestination" }
                        //},
                        Artifacts = new ArtifactMap()
                        {
                            Input = new Artifact[]
                            {
                                new Artifact { Name = "output", Value = "{{ context.executions.artifact_task_1.artifacts.output }}" },
                            },
                        }
                    }
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task ValidateWorkflow_RemoteAppTaskWithoutTypeSet_ReturnsErrors()
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
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[] {
                                new Artifact
                                {
                                    Name = "non_unique_artifact",
                                    Value = "Example Value"
                                }
                            }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination{ Name = "externalTask" }
                        }
                    },
                    new TaskObject
                    {
                        Id = "externalTask",
                        Type = "remote_app_execution",
                        //ExportDestinations = new ExportDestination[]
                        //{
                        //    new ExportDestination { Name = "oneDestination" }
                        //},
                        Artifacts = new ArtifactMap()
                        {
                            Input = new Artifact[]
                            {
                                new Artifact { Name = "output", Value = "{{ context.executions.artifact_task_1.artifacts.output }}" },
                            },
                            Output = new OutputArtifact[]
                            {
                                new OutputArtifact { Name = "report.pdf" },
                            },
                        }
                    }
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.NotEmpty(errors);
            const string expectedError = "Task: 'externalTask' has incorrect artifact output types set on artifacts with following name. report.pdf";
            errors.Contains(expectedError).Should().BeTrue();
        }

        [Fact]
        public async Task ValidateWorkflow_ExportAppWithSameDestinationTwice_ReturnsErrors()
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
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]{
                                new Artifact
                                {
                                    Name = "non_unique_artifact",
                                    Value = "Example Value"
                                }
                            }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination{ Name = "externalTask" }
                        }
                    },
                    new TaskObject
                    {
                        Id = "externalTask",
                        Type = "remote_app_execution",
                        ExportDestinations = new ExportDestination[]
                        {
                            new ExportDestination { Name = "oneDestination" },
                            new ExportDestination { Name = "oneDestination" }
                        },
                        Artifacts = new ArtifactMap()
                        {
                            Input = new Artifact[]
                            {
                                new Artifact { Name = "output", Value = "{{ context.executions.artifact_task_1.artifacts.output }}" },
                            },
                        }
                    }
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task ValidateWorkflow_ExportAppWithInputs_ReturnsErrors()
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
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]{
                                new Artifact
                                {
                                    Name = "non_unique_artifact",
                                    Value = "Example Value"
                                }
                            }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination{ Name = "externalTask" }
                        }
                    },
                    new TaskObject
                    {
                        Id = "externalTask",
                        Type = "remote_app_execution",
                        ExportDestinations = new ExportDestination[]
                        {
                            new ExportDestination { Name = "oneDestination" }
                        },
                        Artifacts = new ArtifactMap()
                        {
                        }
                    }
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.NotEmpty(errors);
        }
        [Fact]
        public async Task ValidateWorkflow_ExportAppWithOutputs_ReturnsErrors()
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
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]{
                                new Artifact
                                {
                                    Name = "non_unique_artifact",
                                    Value = "Example Value"
                                }
                            }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination{ Name = "externalTask" }
                        }
                    },
                    new TaskObject
                    {
                        Id = "externalTask",
                        Type = "remote_app_execution",
                        ExportDestinations = new ExportDestination[]
                        {
                            new ExportDestination { Name = "oneDestination" }
                        },
                        Artifacts = new ArtifactMap()
                        {
                            Input = new Artifact[]
                            {
                                new Artifact { Name = "output", Value = "{{ context.executions.artifact_task_1.artifacts.output }}" },
                            },
                        }
                    }
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.Single(errors);
        }


        [Fact]
        public async Task ValidateWorkflow_Converging_Tasks_ReturnsErrors()
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
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]{
                                new Artifact
                                {
                                    Name = "non_unique_artifact",
                                    Value = "Example Value"
                                }
                            }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination{ Name = "externalTask" }
                        }
                    },
                    new TaskObject
                    {
                        Id = "externalTask",
                        Type = "export",
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination { Name = "rootTask" }
                        },
                        Artifacts = new ArtifactMap()
                        {
                            Input = new Artifact[]
                            {
                                new Artifact { Name = "output", Value = "{{ context.executions.artifact_task_1.artifacts.output }}" },
                            },
                        }
                    },
                    new TaskObject
                    {
                        Id = "externalTask2",
                        Type = "router",
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination{ Name = "rootTask"}
                        },
                        Artifacts = new ArtifactMap()
                        {
                            Input = new Artifact[]
                            {
                                new Artifact { Name = "output", Value = "{{ context.executions.artifact_task_1.artifacts.output }}" },
                            },
                        }
                    }
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.NotEmpty(errors.Where(e => e.Contains("Converging Tasks")));
        }
        [Fact]
        public async Task ValidateWorkflow_Duplicate_TaskIds_ReturnsErrors()
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
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]{
                                new Artifact
                                {
                                    Name = "non_unique_artifact",
                                    Value = "Example Value"
                                }
                            }
                        },
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination{ Name = "externalTask" }
                        }
                    },
                    new TaskObject
                    {
                        Id = "rootTask",
                        Type = "router",
                        TaskDestinations = new TaskDestination[]
                        {
                            new TaskDestination { Name = "rootTask" }
                        },
                        Artifacts = new ArtifactMap()
                        {
                        }
                    }
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.NotEmpty(errors.Where(e => e.Contains("Found duplicate task")));
        }
        [Fact]
        public async Task ValidateWorkflow_No_InformaticsGateway_On_export_Task_ReturnsErrors()
        {
            var workflow = new Workflow
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
                    new TaskObject
                    {
                        Id = "rootTask",
                        Type = "export",
                        Description = "TestDesc",
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]{
                                new Artifact
                                {
                                    Name = "non_unique_artifact",
                                    Value = "Example Value"
                                }
                            }
                        },
                        ExportDestinations = new ExportDestination[]
                        {
                            new ExportDestination { Name = "oneDestination" }
                        },
                    },
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.NotEmpty(errors.Where(e => e.Contains("InformaticsGateway ExportDestinations destinations can not be null")));
        }
        [Fact]
        public async Task ValidateWorkflow_InformaticsGateway_No_AETitle_ReturnsErrors()
        {
            var workflow = new Workflow
            {
                Name = "Workflowname1",
                Description = "Workflowdesc1",
                Version = "1",
                InformaticsGateway = new InformaticsGateway
                {
                },
                Tasks = new TaskObject[]
                {
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.NotEmpty(errors.Where(e => e.Contains("AeTitle is required in the InformaticsGateaway section")));
        }

        [Fact]
        public async Task ValidateWorkflow_No_name_Or_Values_On_Inputs_ReturnsErrors()
        {
            var workflow = new Workflow
            {
                Name = "Workflowname1",
                Description = "Workflowdesc1",
                Version = "1",
                InformaticsGateway = new InformaticsGateway
                {
                },
                Tasks = new TaskObject[]
                {
                    new TaskObject
                    {
                        Id = "rootTask",
                        Type = "export",
                        Description = "TestDesc",
                        Artifacts = new ArtifactMap
                        {
                            Input = new Artifact[]{
                                new Artifact
                                {
                                    Name = "",
                                    Value = ""
                                }
                            }
                        },
                        ExportDestinations = new ExportDestination[]
                        {
                            new ExportDestination { Name = "oneDestination" }
                        },
                    },
                }
            };

            _workflowService.Setup(w => w.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(null, TimeSpan.FromSeconds(.1));

            var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

            Assert.NotEmpty(errors.Where(e => e.Contains("Input Artifacts must have a Name")));
            Assert.NotEmpty(errors.Where(e => e.Contains("Input Artifacts must have a Value")));
        }
    }
}
