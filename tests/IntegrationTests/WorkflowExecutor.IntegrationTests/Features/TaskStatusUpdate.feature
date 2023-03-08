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
    | Partialfail      |

@TaskUpdate
Scenario Outline: Publish a valid Task Update event with a failed status and failure reason
    Given I have a clinical workflow Task_Status_Update_Workflow
    And I have a Workflow Instance WFI_Task_Status_Update with no artifacts
    When I publish a Task Update Message <taskUpdateEvent> with status Failed
    Then I can see the status of the Task is updated
    Examples:
    | taskUpdateEvent                         |
    | Task_Status_Update_Invalid_Message      |
    | Task_Status_Update_Runner_Not_Supported |

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
    | Task_Status_Update_Missing_ExecutionId   |
    | Task_Status_Update_Missing_CorrelationId |

@TaskUpdate
Scenario Outline: Publish an valid Task Update event with a status that is invalid for current status
    Given I have a clinical workflow <workflow>
    And I have a Workflow Instance <existingWFI> with no artifacts
    When I publish a Task Update Message <taskUpdateMessage> with status <taskUpdateStatus>
    Then I can see the status of the Task is not updated
    Examples:
    | workflow                                              | existingWFI               | taskUpdateMessage                                | taskUpdateStatus |
    | Workflow_Revision_for_publish_an_invalid_task_update  | WFI_Task_Status_Succeeded | Task_Status_Update_Status_Invalid_When_Succeeded | Accepted         |
    | Workflow_Revision_for_publish_an_invalid_task_update  | WFI_Task_Status_Failed    | Task_Status_Update_Status_Invalid_When_Failed    | Accepted         |
    | Workflow_Revision_for_publish_an_invalid_task_update  | WFI_Task_Status_Canceled  | Task_Status_Update_Status_Invalid_When_Canceled  | Accepted         |

@TaskExport
Scenario Outline: Export task with single destination is in progress, export message is sent 
    Given I have a clinical workflow <workflow>
    And I have a Workflow Instance <workflowInstance> with no artifacts
    When I publish a Task Update Message <taskUpdate> with artifacts <minioFiles> in minio
    Then 1 Export Request message is published
    Examples:
    | workflow                                   | workflowInstance                           | taskUpdate                                  | minioFiles                     |
    | Workflow_Revision_for_export_single_dest_1 | Workflow_Instance_for_export_single_dest_1 | Task_status_update_for_export_single_dest_1 | output_metadata                |
    | Workflow_Revision_for_export_folder        | Workflow_Instance_for_export_folder        | Task_status_update_for_export_folder        | output_metadata_multiple_files |

@TaskExport
Scenario: Export task with mutliple destinations is in progress, export message is sent 
    Given I have a clinical workflow Workflow_Revision_for_export_multi_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_multi_dest_1 with no artifacts
    When I publish a Task Update Message Task_status_update_for_export_multi_dest_1 with artifacts output_metadata in minio
    Then 2 Export Request messages are published

@TaskExport
Scenario: Export task with single destination and no artifact is in progress, export message is not sent
    Given I have a clinical workflow Workflow_Revision_for_export_single_dest_1
    And I have a Workflow Instance Workflow_Instance_for_export_single_dest_1 with no artifacts
    When I publish a Task Update Message Task_status_update_for_export_single_dest_1 with status Succeeded 
    Then 0 Export Request messages are published

@TaskExport
Scenario: Export request complete message is sent as Succeeded, next task dispatched
    Given I have a clinical workflow Workflow_Revision_for_export_multi_dest_2
    And I have a Workflow Instance Workflow_Instance_for_export_multi_dest_2 with artifacts output_metadata in minio
    When I publish a Export Complete Message Export_Complete_Message_for_export_multi_dest_2_Succeeded
    Then I can see the status of the Task export_task_1 is Succeeded
    And I can see the status of the Task task_3 is Dispatched

@TaskExport
Scenario: Export request complete message is sent as Partial Failed or Failed, workflow is Failed
    Given I have a clinical workflow Workflow_Revision_for_export_multi_dest_2
    And I have a Workflow Instance Workflow_Instance_for_export_multi_dest_2 with artifacts output_metadata in minio
    When I publish a Export Complete Message <exportCompleteMessage>
    Then I can see the status of the Task export_task_1 is Failed
    And Workflow Instance status is Failed
    Examples:
    | exportCompleteMessage                                         |
    | Export_Complete_Message_for_export_multi_dest_2_PartialFailed |
    | Export_Complete_Message_for_export_multi_dest_2_Failed        |
