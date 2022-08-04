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

Feature: TaskStatusUpdate

Update task status in the workflow instance from a Task Update Event

@TaskUpdate
Scenario Outline: Publish a valid Task Update event which updates the Task status
    Given I have a clinical workflow Task_Status_Update_Workflow
    And I have a Workflow Instance WFI_Task_Status_Update with no artifacts
    When I publish a Task Update Message Task_Status_Update with status <taskUpdateStatus>
    Then I can see the status of the Task is updated
    Examples:
    | taskUpdateStatus |
    | Accepted         |
    | Succeeded        |
    | Failed           |
    | Canceled         |

@TaskUpdate
Scenario Outline: Publish a successful Task Update event which updates the Task status and copies the metadata
    Given I have a clinical workflow Task_Status_Update_Workflow
    And I have a Workflow Instance WFI_Task_Status_Update with no artifacts
    When I publish a Task Update Message <updateMessage> with status Succeeded
    Then I can see the status of the Task is updated
    And I can see the Metadata is copied to the workflow instance
    Examples:
    | updateMessage                  |
    | Task_Status_Update             |
    | Task_Status_Update_Meta_String |
    | Task_Status_Update_Meta_Bool   |
    | Task_Status_Update_Meta_Int    |

@TaskUpdate
Scenario: Publish a valid Task Update event that where WorkflowInstance does not contain TaskId
    Given I have a clinical workflow Task_Status_Update_Workflow
    And I have a Workflow Instance WFI_Task_Status_Update with no artifacts
    When I publish a Task Update Message Task_Status_Update_TaskId_Not_Found with status Succeeded 
    Then I can see the status of the Task is not updated

@TaskUpdate
Scenario Outline: Publish an invalid Task Update event which does not update the task status
    Given I have a clinical workflow Task_Status_Update_Workflow
    And I have a Workflow Instance WFI_Task_Status_Update with no artifacts
    When I publish a Task Update Message <taskUpdateMessage> with status Succeeded 
    Then I can see the status of the Task is not updated
    Examples:
    | taskUpdateMessage                        |
    | Task_Status_Update_Missing_TaskId        |
    | Task_Status_Update_Missing_ExecutionId   |
    | Task_Status_Update_Missing_CorrelationId |
    | Task_Status_Update_Missing_Status        |

@TaskUpdate
Scenario Outline: Publish an valid Task Update event with a status that is invalid for current status
    Given I have a Workflow Instance <existingWFI> with no artifacts
    When I publish a Task Update Message <taskUpdateMessage> with status <taskUpdateStatus>
    Then I can see the status of the Task is not updated
    Examples:
    | existingWFI               | taskUpdateMessage                                | taskUpdateStatus |
    | WFI_Task_Status_Succeeded | Task_Status_Update_Status_Invalid_When_Succeeded | Accepted         |
    | WFI_Task_Status_Failed    | Task_Status_Update_Status_Invalid_When_Failed    | Accepted         |
    | WFI_Task_Status_Canceled  | Task_Status_Update_Status_Invalid_When_Canceled  | Accepted         |

@TaskExport
Scenario: Export task with single destination is in progress, export message is sent 
    Given I have a clinical workflow Workflow_Revision_for_export_single_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_single_dest_1 with no artifacts
    When I publish a Task Update Message Task_status_update_for_export_single_dest_1 with artifacts output_metadata in minio
    Then 1 Export Request message is published

@TaskExport
Scenario: Export task with mutliple destinations is in progress, export message is sent 
    Given I have a clinical workflow Workflow_Revision_for_export_multi_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_multi_dest_1 with no artifacts
    When I publish a Task Update Message Task_status_update_for_export_multi_dest_1 with artifacts output_metadata in minio
    Then 1 Export Request message is published

@TaskExport
Scenario: Export task with single destination and no artifact is in progress, export message is not sent
    Given I have a clinical workflow Workflow_Revision_for_export_single_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_single_dest_1 with no artifacts
    When I publish a Task Update Message Task_status_update_for_export_single_dest_1 with status Succeeded 
    Then An Export Request message is not published

@TaskExport
Scenario: Export request complete message is sent as Succeeded, next task dispatched
    Given I have a clinical workflow Workflow_Revision_for_export_single_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_single_dest_1 with no artifacts
    And I have a payload saved in mongo [string]
    When I publish a Task Update Message Task_status_update_for_export_single_dest_1 with status Succeeded
    And I publish an Export Request message Export_request_for_export_single_dest_1 with status Succeeded
    Then The export request in the worfkflow instance Workflow_Instance_for_export_single_dest_1 is updated to Succeeded

@TaskExport
Scenario: Export request complete message is sent as Failed, workflow is Failed
    Given I have a clinical workflow Workflow_Revision_for_export_single_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_single_dest_1 with no artifacts
    And I have a payload saved in mongo [string]
    When I publish a Task Update Message Task_status_update_for_export_single_dest_1 with status Succeeded
    And I publish an Export Request message Export_request_for_export_single_dest_1 with status Failed
    Then The export request in the worfkflow instance Workflow_Instance_for_export_single_dest_1 is updated to Failed
    And Workflow Instance status is Failed

@TaskExport
Scenario: Export request complete message is sent as Partial Failed, workflow is Failed
    Given I have a clinical workflow Workflow_Revision_for_export_single_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_single_dest_1 with no artifacts
    And I have a payload saved in mongo [string]
    When I publish a Task Update Message Task_status_update_for_export_single_dest_1 with status Succeeded
    And I publish an Export Request message Export_request_for_export_single_dest_1 with status Partial Failed
    Then The export request in the worfkflow instance Workflow_Instance_for_export_single_dest_1 is updated to Failed
    And Workflow Instance status is Failed
