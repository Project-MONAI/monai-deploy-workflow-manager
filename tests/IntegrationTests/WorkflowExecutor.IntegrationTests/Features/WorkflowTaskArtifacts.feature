# Copyright 2022 MONAI Consortium
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

Feature: WorkflowTaskArtifact

Integration test for Task input and output artifacts

@TaskArtifacts_WorkflowRequest
Scenario Outline: Workflow with context.input.dicom is triggered when mandatory files exist resulting in a successful Task dispatch
    Given I have a clinical workflow <workflow>
    When I publish a Workflow Request Message Artifact_AeTitle_Request_1 with artifacts full_patient_metadata in minio
    Then I can see 1 Workflow Instance is created
    And Input artifacts are mapped
    And 1 Task Dispatch event is published
    Examples:
    | workflow                                             |
    | OneTask_Context.Dicom.Input_ArtifactMandatory=Null   |
    | OneTask_Context.Dicom.Input_ArtifactMandatory=True   |
    | OneTask_Context.Dicom.Input_ArtifactMandatory=False  |

@TaskArtifacts_WorkflowRequest
Scenario Outline: Workflow is triggered when mandatory artifacts are missing resulting in a failed Task dispatch
    Given I have a clinical workflow <workflow>
    When I publish a Workflow Request Message Artifact_AeTitle_Request_1 with no artifacts
    Then Workflow Instance status is Failed
    And A Task Dispatch event is not published
    Examples:
    | workflow                                           |
    | OneTask_Context.Dicom.Input_ArtifactMandatory=Null |
    | OneTask_Context.Dicom.Input_ArtifactMandatory=True |

@TaskArtifacts_WorkflowRequest
Scenario Outline: Workflow with context.input.dicom is triggered when non-mandatory files are missing resulting in a successful Task dispatch
    Given I have a clinical workflow OneTask_Context.Dicom.Input_ArtifactMandatory=False
    When I publish a Workflow Request Message Artifact_AeTitle_Request_1 with no artifacts
    Then I can see 1 Workflow Instance is created
    And 1 Task Dispatch event is published

@TaskArtifacts_TaskUpdate
Scenario Outline: Task with context.input.dicom is triggered when mandatory files exist resulting in a successful Task dispatch
    Given I have a clinical workflow <workflow>
    And I have a Workflow Instance <workflowInstance> with artifacts full_patient_metadata in minio
    When I publish a Task Update Message <taskUpdate> with artifacts output_metadata in minio
    Then 1 Task Dispatch event is published
    And I can see 1 Workflow Instance is updated
    And Input artifacts are mapped
    Examples:
    | workflow                                            | workflowInstance                                    | taskUpdate                                          |
    | TwoTask_Context.Dicom.Input_ArtifactMandatory=Null  | TwoTask_Context.Dicom.Input_ArtifactMandatory=Null  | TwoTask_Context.Dicom.Input_ArtifactMandatory=Null  |
    | TwoTask_Context.Dicom.Input_ArtifactMandatory=True  | TwoTask_Context.Dicom.Input_ArtifactMandatory=True  | TwoTask_Context.Dicom.Input_ArtifactMandatory=True  |
    | TwoTask_Context.Dicom.Input_ArtifactMandatory=False | TwoTask_Context.Dicom.Input_ArtifactMandatory=False | TwoTask_Context.Dicom.Input_ArtifactMandatory=False |

@TaskArtifacts_TaskUpdate
Scenario Outline: Task with context.input.dicom is triggered when non mandatory files do not exist resulting in a successful Task Dispatch
    Given I have a clinical workflow <workflow>
    And I have a Workflow Instance <workflowInstance> with no artifacts
    When I publish a Task Update Message <taskUpdate> with artifacts output_metadata in minio
    Then 1 Task Dispatch event is published
    And I can see 1 Workflow Instance is updated
    And Input artifacts are mapped
    Examples:
    | workflow                                            | workflowInstance                                    | taskUpdate                                          |
    | TwoTask_Context.Dicom.Input_ArtifactMandatory=Null  | TwoTask_Context.Dicom.Input_ArtifactMandatory=Null  | TwoTask_Context.Dicom.Input_ArtifactMandatory=Null  |
    | TwoTask_Context.Dicom.Input_ArtifactMandatory=True  | TwoTask_Context.Dicom.Input_ArtifactMandatory=True  | TwoTask_Context.Dicom.Input_ArtifactMandatory=True  |

@TaskArtifacts_TaskUpdate
Scenario Outline: Task with context.input.dicom is triggered when mandatory files do not exist resulting in a failed Task Dispatch
    Given I have a clinical workflow <workflow>
    And I have a Workflow Instance <workflowInstance> with no artifacts
    When I publish a Task Update Message <taskUpdate> with artifacts output_metadata in minio
    Then A Task Dispatch event is not published
    Examples:
    | workflow                                           | workflowInstance                                   | taskUpdate                                         |
    | TwoTask_Context.Dicom.Input_ArtifactMandatory=Null | TwoTask_Context.Dicom.Input_ArtifactMandatory=Null | TwoTask_Context.Dicom.Input_ArtifactMandatory=Null |
    | TwoTask_Context.Dicom.Input_ArtifactMandatory=True | TwoTask_Context.Dicom.Input_ArtifactMandatory=True | TwoTask_Context.Dicom.Input_ArtifactMandatory=True |
