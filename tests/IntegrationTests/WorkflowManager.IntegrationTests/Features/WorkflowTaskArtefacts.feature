Feature: WorkflowTaskArtefact

Artefacts can get passed into and between tasks

@WorkflowTaskArtefacts
Scenario Outline: Bucket exists in MinIO, publish workflow request which uses input artefacts
    Given I have a bucket in MinIO dicom
    And I have a clinical workflow Single_Task_Context_Input
    When I publish a Workflow Request Message Context_Input_AE
    Then I can see 1 Workflow Instance is created 
    And 1 Task Dispatch event is published

@WorkflowTaskArtefacts
Scenario Outline: Bucket does not exist in MinIO, publish workflow request which uses non existant bucket
    Given I have a clinical workflow Single_Task_Context_Input
    When I publish a Workflow Request Message Context_Input_AE
    Then The workflow instance fails

@WorkflowTaskArtefacts
Scenario Outline: Create artefact in MinIO, publish task update message with artefact as output
    Given I have a bucket in MinIO outputArtefact
    And I have a clinical workflow Multi_Task_Output_Artifact
    And I have a Workflow Instance Single_Task_Completed
    When I publish a task update message output_artefact
    When I publish a task update message <Task_Update_Message>
    Then I can see 1 Workflow Instance is created 
    And 1 Task Dispatch event is published
    Examples:
    | Task_Update_Message  |
    | output_artefact_file |
    | output_artefact_dir  |

@WorkflowTaskArtefacts
Scenario Outline: Bucket exists in MinIO, send task dispatch with non existant file path
    Given I have a bucket in MinIO dicom
    And I have a clinical workflow Single_Task_Context_Input
    When I publish a Workflow Request Message Context_Input_AE
    When I publish a task update message output_artefact
    Then The workflow instance fails

@WorkflowTaskArtefacts
Scenario Outline: Bucket exists in MinIO, send task dispatch with non existant artefact
    Given I have a bucket in MinIO dicom
    And I have a clinical workflow Single_Task_Context_Input
    And I have a Workflow Instance non_existant_artefact
    When I publish a Workflow Request Message Context_Input_AE
    Then The workflow instance fails
