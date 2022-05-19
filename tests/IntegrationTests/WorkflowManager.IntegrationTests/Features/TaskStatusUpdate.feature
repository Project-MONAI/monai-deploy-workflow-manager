Feature: TaskStatusUpdate

Update task status in the workflow instance from a Task Update Event

@TaskUpdate
Scenario Outline: Publish a valid Task Update event which updates the Task status
    Given I have a clinical workflow Task_Status_Update_Workflow
    And I have a Workflow Instance WFI_Task_Status_Update
    When I publish a Task Update Message Task_Status_Update with status <taskUpdateStatus>
    Then I can see the status of the Task is updated
    Examples:
    | taskUpdateStatus |
    | Accepted         |
    | Succeeded        |
    | Failed           |
    | Canceled         |

@TaskUpdate
Scenario: Publish a valid Task Update event that where WorkflowInstance does not contain TaskId
    Given I have a clinical workflow Task_Status_Update_Workflow
    And I have a Workflow Instance WFI_Task_Status_Update
    When I publish a Task Update Message Task_Status_Update_TaskId_Not_Found with status Succeeded 
    Then I can see the status of the Task is not updated

@TaskUpdate
Scenario Outline: Publish an invalid Task Update event which does not update the task status
    Given I have a clinical workflow Task_Status_Update_Workflow
    And I have a Workflow Instance WFI_Task_Status_Update
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
    Given I have a Workflow Instance <existingWFI>
    When I publish a Task Update Message <taskUpdateMessage> with status <taskUpdateStatus>
    Then I can see the status of the Task is not updated
    Examples:
    | existingWFI               | taskUpdateMessage                                | taskUpdateStatus |
    | WFI_Task_Status_Succeeded | Task_Status_Update_Status_Invalid_When_Succeeded | Accepted         |
    | WFI_Task_Status_Failed    | Task_Status_Update_Status_Invalid_When_Failed    | Accepted         |
    | WFI_Task_Status_Canceled  | Task_Status_Update_Status_Invalid_When_Canceled  | Accepted         |
