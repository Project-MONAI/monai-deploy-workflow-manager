Feature: TaskStatusUpdate

Update task status in the workflow instance

Scenario Outline: Publish a valid Task Update event which updates the Task status
    Given I have a Workflow Instance Existing_WFI_Created
    When I publish a Task Update Message <taskUpdateMessage> with status <taskUpdateStatus>
    Then I can see the status of the Task is updated
    Examples:
    | taskUpdateMessage        | taskUpdateStatus |
    | Task_Status_Update_Valid | Accepted         |
    | Task_Status_Update_Valid | Succeeded        |
    | Task_Status_Update_Valid | Failed           |
    | Task_Status_Update_Valid | Cancelled        |

Scenario Outline: Publish a valid Task Update event that does not match a workflow instance which does not update the task status
    Given I have a Workflow Instance Existing_WFI_Created
    When I publish a Task Update Message <taskUpdateMessage> with status Succeeded 
    Then I can see the status of the Task is not updated
    Examples:
    | taskUpdateMessage                |
    | Execution_ID_Not_Found           |
    | Execution_And_Task_ID_Dont_Match |

Scenario Outline: Publish an invalid Task Update event which does not update the task status
    Given I have a Workflow Instance Existing_WFI_Created
    When I publish a Task Update Message <taskUpdateMessage> with status Succeeded 
    Then I can see the status of the Task is not updated
    Examples:
    | taskUpdateMessage     |
    | Invalid_WorkflowID    |
    | Invalid_TaskID        |
    | Invalid_ExecutionID   |
    | Invalid_CorrelationID |
    | Missing_Status        |

Scenario Outline: Publish an valid Task Update event with a status that is non linear
    Given I have a Workflow Instance Existing_WFI_Status_Succeeded
    When I publish a Task Update Message Task_Status_Update_Valid with status Accepted
    Then I can see the status of the Task is not updated




