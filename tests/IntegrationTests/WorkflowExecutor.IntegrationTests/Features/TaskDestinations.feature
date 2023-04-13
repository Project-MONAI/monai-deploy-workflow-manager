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

@IntergrationTests
Feature: TaskDestinations

New task is dispatched after a Task update message is received.

@TaskUpdate
Scenario: Publish a valid Task Update event which triggers a single new task
    Given I have a clinical workflow Multi_Task_Workflow_3
    And I have a Workflow Instance WFI_Multi_Task_3 with no artifacts
    When I publish a Task Update Message Task_Update_To_Dispatch_Single_Task with status Succeeded
    Then 1 Task Dispatch event is published
    And Workflow Instance is updated with the new Task
    And I can see the status of the Tasks are updated
    And Workflow Instance status is Created

@TaskUpdate
Scenario: Publish a valid Task Update event which triggers multiple new tasks
    Given I have a clinical workflow Multi_Task_Workflow_2
    And I have a Workflow Instance WFI_Multi_Task_2 with no artifacts
    When I publish a Task Update Message Task_Update_Dispatches_Multi_Tasks with status Succeeded
    Then 2 Task Dispatch events are published
    And Workflow Instance is updated with the new Tasks
    And I can see the status of the Tasks are updated
    And Workflow Instance status is Created

@TaskUpdate
Scenario Outline: Publish a valid Task Update event where the Task status is not created
    Given I have a clinical workflow Multi_Task_Workflow_1
    And I have a Workflow Instance <workflowInstance> with no artifacts
    When I publish a Task Update Message Task_Update_Dispatches_Single_Task with status Succeeded
    Then A Task Dispatch event is not published
    Examples:
    | workflowInstance          |
    | WFI_Multi_Task_Dispatched |
    | WFI_Multi_Task_Accepted   |
    | WFI_Multi_Task_Succeeded  |

@TaskUpdate
Scenario: Publish a valid Task Update event as failed which does not trigger a new task and updates the workflow status to Failed
    Given I have a clinical workflow Multi_Task_Workflow_1
    And I have a Workflow Instance WFI_Multi_Task_1 with no artifacts
    When I publish a Task Update Message Task_Update_Dispatches_Single_Task with status Failed
    Then A Task Dispatch event is not published
    And Workflow Instance status is Failed

@TaskUpdate
Scenario: Publish a valid Task Update event as partial fail which does not trigger a new task
    Given I have a clinical workflow Multi_Task_Workflow_Clinical_Review_1
    And I have a Workflow Instance WFI_Clinical_Review_1 with no artifacts
    When I publish a Task Update Message Task_Update_Dispatches_Clinical_Review_False with status PartialFail
    Then A Task Dispatch event is not published
    And Clinical Review Metadata is added to workflow instance
    And Workflow Instance status is Succeeded

@TaskUpdate
Scenario: Publish a valid Task Update event as succeeded which triggers a new task
    Given I have a clinical workflow Multi_Task_Workflow_Clinical_Review_1
    And I have a Workflow Instance WFI_Clinical_Review_1 with no artifacts
    When I publish a Task Update Message Task_Update_Dispatches_Clinical_Review_True with status Succeeded
    Then 1 Task Dispatch events are published
    And Clinical Review Metadata is added to workflow instance
    And Workflow Instance status is Created

@TaskDestinationConditions
Scenario: Task destination with condition true, WFI is updated with Task and task dispatch message is published
    Given I have a clinical workflow <workflow>
    And I have a Workflow Instance <workflowInstance> with no artifacts
    And I have a payload saved in mongo <payload>
    When I publish a Task Update Message <taskUpdateMessage> with status Succeeded
    Then 1 Task Dispatch event is published
    Examples:
    | workflow                                                            | workflowInstance                                  | taskUpdateMessage                                         | payload              |
    | Multi_Task_Workflow_Destination_Single_Condition_True               | WFI_Task_Destination_Condition_True               | Task_Update_Task_Destination_Condition_True               | Payload_Full_Patient |
    | Multi_Task_Workflow_Destination_Single_Metadata_Condition_True      | WFI_Task_Destination_Metadata_Condition_True      | Task_Update_Task_Destination_Metadata_Condition_True      | Payload_Full_Patient |
    #| Multi_Task_Workflow_Destination_Single_Metadata_Null_Condition_True | WFI_Task_Destination_Metadata_Null_Condition_True | Task_Update_Task_Destination_Metadata_Null_Condition_True | Payload_Null         |  # This test requires extra work, currently empty payload do not return null so it returns an empty string and may as well be like the above test https://github.com/Project-MONAI/monai-deploy-workflow-manager/issues/320

@TaskDestinationConditions
Scenario: Task destination with condition false, WFI is not updated with Task and task dispatch message is not published
    Given I have a clinical workflow <workflow>
    And I have a Workflow Instance <workflowInstance> with no artifacts
    And I have a payload saved in mongo <payload>
    When I publish a Task Update Message <taskUpdateMessage> with status Succeeded
    Then A Task Dispatch event is not published
    Examples:
    | workflow                                                        | workflowInstance                              | taskUpdateMessage                                     | payload              |
    | Multi_Task_Workflow_Destination_Single_Condition_False          | WFI_Task_Destination_Condition_False          | Task_Update_Task_Destination_Condition_False          | Payload_Full_Patient |
    | Multi_Task_Workflow_Destination_Single_Metadata_Condition_False | WFI_Task_Destination_Metadata_Condition_False | Task_Update_Task_Destination_Metadata_Condition_False | Payload_Full_Patient |
    #| Multi_Task_Workflow_Destination_Single_Metadata_Condition_False | WFI_Task_Destination_Metadata_Condition_False | Task_Update_Task_Destination_Metadata_Condition_False | Payload_Null         | # This test requires extra work, currently empty payload do not return null so it returns an empty string and may as well be like the above test https://github.com/Project-MONAI/monai-deploy-workflow-manager/issues/320

@TaskDestinationConditions
Scenario: Multiple task destinations with condition true, multiple task dispatch messages sent
    Given I have a clinical workflow Multi_Task_Workflow_Multiple_Destination_Single_Condition_True
    And I have a Workflow Instance WFI_Task_Multiple_Destination_Condition_True with no artifacts
    When I publish a Task Update Message Task_Update_Task_Multiple_Destination_Condition_True with status Succeeded
    Then 3 Task Dispatch events are published

@TaskDestinationConditions
Scenario: Multiple task destinations based on Dicom data with upper and lower cases condition, mutiple task dispatch messages sen
    Given I have a clinical workflow Workflow_Revision_For_Case_Sensitivity
    And I have a Workflow Instance Workflow_Instance_For_Case_Sensitivity with artifacts full_patient_metadata in minio
    When I publish a Task Update Message Task_Update_Task_Multiple_Destination_Condition_Case_Sensitivity with status Succeeded
    Then 1 Task Dispatch events are published

@TaskDestinationConditions
Scenario: Multiple task destinations with condition false, no task dispatch messages sent
    Given I have a clinical workflow Multi_Task_Workflow_Multiple_Destination_Single_Condition_False
    And I have a Workflow Instance WFI_Task_Multiple_Destination_Condition_False with no artifacts
    When I publish a Task Update Message Task_Update_Task_Multiple_Destination_Condition_False with status Succeeded
    Then A Task Dispatch event is not published

@TaskDestinationConditions
Scenario: Multiple task destinations one with condition true and one with false, 1 task dispatch message published for task which is true
    Given I have a clinical workflow Multi_Task_Workflow_Destination_Multiple_Condition_True_And_False
    And I have a Workflow Instance WFI_Task_Destination_Condition_True_And_False with no artifacts
    When I publish a Task Update Message Task_Update_Task_Destination_Condition_True_And_False with status Succeeded
    Then 1 Task Dispatch event is published
    And The Task Dispatch event is for Task Id b9964b10-acb4-4050-a610-374fdbe2100d

@TaskDestinationConditions
Scenario: Workflow instance status remains created when any task status is either dispatch or accepted
    Given I have a clinical workflow Multi_Task_Workflow_Destination_Single_Condition_True
    And I have a Workflow Instance WFI_Task_Destination_Condition_True with no artifacts
    When I publish a Task Update Message Task_Update_Task_Destination_Condition_True with status Succeeded
    Then 1 Task Dispatch event is published
    And Workflow Instance status is Created

@TaskDestinationConditions
Scenario: Single task destinations with multiple conditions true, single task dispatch message sent
    Given I have a clinical workflow Multi_Task_Workflow_Destination_Single_Multi_Condition_True
    And I have a Workflow Instance WFI_Task_Destination_Multi_Condition_True with no artifacts
    When I publish a Task Update Message Task_Update_Task_Destination_Multi_Condition_True with status Succeeded
    Then 1 Task Dispatch event is published
    And Workflow Instance status is Created

@TaskDestinationConditions
Scenario: Single task destinations with one condition true and one false, workflow instance status is succeeded
    Given I have a clinical workflow Multi_Task_Workflow_Destination_Single_Multi_Condition_False
    And I have a Workflow Instance WFI_Task_Destination_Multi_Condition_False with no artifacts
    When I publish a Task Update Message Task_Update_Task_Destination_Multi_Condition_False with status Succeeded
    Then A Task Dispatch event is not published
    And Workflow Instance status is Succeeded

@TaskDestinationConditions
Scenario: Workflow instance status is succeeded when a condition is invalid
    Given I have a clinical workflow Multi_Task_Workflow_Task_Destination_Invalid_Condition
    And I have a Workflow Instance WFI_Task_Destination_Invalid_Condition with no artifacts
    When I publish a Task Update Message Task_Update_Task_Destination_Invalid_Condition with status Succeeded
    Then A Task Dispatch event is not published
    And Workflow Instance status is Succeeded

@TaskDestinationConditions
Scenario: Task destination based on Dicom data conditional is successful, and task is completed and marked as succeeded
    Given I have a clinical workflow Workflow_Revision_for_bucket_minio
    And I have a Workflow Instance Workflow_instance_for_bucket_minio with artifacts patient_1_lordge in minio
    When I publish a Task Update Message Task_status_update_for_bucket_minio with artifacts patient_1_lordge in minio
    Then I can see the status of the Task pizza is Succeeded
