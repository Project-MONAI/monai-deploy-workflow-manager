Feature: TaskStatusUpdate

Update task status in the workflow instance from a Task Update Event

@TaskUpdate
Scenario Outline: Publish a valid Task Update event which updates the Task status
    Given I have a Workflow Instance <existingWFI>
    When I publish a Task Update Message <taskUpdateMessage> with status <taskUpdateStatus>
    Then I can see the status of the Task is updated
    Examples:
    | existingWFI              | taskUpdateMessage          | taskUpdateStatus |
    | WFI_Task_Status_Update_1 | Task_Status_Update_Valid_1 | Accepted         |
    | WFI_Task_Status_Update_2 | Task_Status_Update_Valid_2 | Succeeded        |
    | WFI_Task_Status_Update_3 | Task_Status_Update_Valid_3 | Failed           |
    | WFI_Task_Status_Update_4 | Task_Status_Update_Valid_4 | Canceled         |

@TaskUpdate
Scenario Outline: Publish a valid Task Update event that does not match a workflow instance which does not update the task status
    Given I have a Workflow Instance <existingWFI>
    When I publish a Task Update Message <taskUpdateMessage> with status Succeeded 
    Then I can see the status of the Task is not updated
    Examples:
    | existingWFI              | taskUpdateMessage                               |
    | WFI_Task_Status_Update_2 | Task_Status_Update_WorkflowInstanceId_Not_Found |
    | WFI_Task_Status_Update_4 | Task_Status_Update_TaskId_Not_Found             |

@TaskUpdate
Scenario Outline: Publish an invalid Task Update event which does not update the task status
    Given I have a Workflow Instance <existingWFI>
    When I publish a Task Update Message <taskUpdateMessage> with status Succeeded 
    Then I can see the status of the Task is not updated
    Examples:
    | existingWFI              | taskUpdateMessage                        |
    | WFI_Task_Status_Update_1 | Task_Status_Update_Missing_WorkflowId    |
    | WFI_Task_Status_Update_2 | Task_Status_Update_Missing_TaskId        |
    | WFI_Task_Status_Update_3 | Task_Status_Update_Missing_ExecutionId   |
    | WFI_Task_Status_Update_4 | Task_Status_Update_Missing_CorrelationId |
    | WFI_Task_Status_Update_1 | Task_Status_Update_Missing_Status        |

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
