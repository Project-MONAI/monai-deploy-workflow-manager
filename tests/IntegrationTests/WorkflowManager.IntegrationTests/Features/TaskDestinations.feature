Feature: TaskDestinations

New task is dispatched after a Task update message is received.

@TaskUpdate
Scenario: Publish a valid Task Update event which triggers a single new task
    Given I have a clinical workflow Multi_Task_Workflow_1
    And I have a Workflow Instance WFI_Multi_Task_1
    When I publish a Task Update Message Task_Update_Dispatches_Single_Task with status Succeeded
    Then 1 Task Dispatch event is published
    And Workflow Instance is updated with the new Task
    And I can see the status of the Tasks are updated
    And Workflow Instance status is Created

@TaskUpdate
Scenario: Publish a valid Task Update event which triggers multiple new tasks
    Given I have a clinical workflow Multi_Task_Workflow_2
    And I have a Workflow Instance WFI_Multi_Task_2
    When I publish a Task Update Message Task_Update_Dispatches_Multi_Tasks with status Succeeded
    Then 2 Task Dispatch events are published
    And Workflow Instance is updated with the new Tasks
    And I can see the status of the Tasks are updated
    And Workflow Instance status is Created

@TaskUpdate
Scenario Outline: Publish a valid Task Update event where the next Task is not of status Created and see that a Task Dispatch event is not created
    Given I have a clinical workflow Multi_Task_Workflow_1
    And I have a Workflow Instance <workflowInstance>
    When I publish a Task Update Message Task_Update_Dispatches_Single_Task with status Succeeded
    Then A Task Dispatch event is not published
    Examples:
    | workflowInstance          |
    | WFI_Multi_Task_Dispatched |
    | WFI_Multi_Task_Accepted   |
    | WFI_Multi_Task_Succeeded  |

@TaskUpdate
Scenario: Publish a valid Task Update event which does not link to another task on the workflow leaves the workflow instance in Created status
    Given I have a clinical workflow Multi_Independent_Task_Workflow
    And I have a Workflow Instance WFI_Multi_Independent_Task
    When I publish a Task Update Message Task_Update_Independent_Task with status Succeeded
    Then A Task Dispatch event is not published
    And Workflow Instance status is Created

@TaskUpdate
Scenario: Publish a valid Task Update event as failed which does not trigger a new task and updates the workflow status to Failed
    Given I have a clinical workflow Multi_Task_Workflow_1
    And I have a Workflow Instance WFI_Multi_Task_1
    When I publish a Task Update Message Task_Update_Dispatches_Single_Task with status Failed
    Then A Task Dispatch event is not published
    And Workflow Instance status is Failed

@TaskUpdate
Scenario: Publish a valid Task Update which has an invalid task destination
    Given I have a clinical workflow Multi_Task_Workflow_Invalid_Task_Destination
    And I have a Workflow Instance WFI_Invalid_Task_Destination
    When I publish a Task Update Message Task_Update_Invalid_Task_Destination with status Succeeded
    Then A Task Dispatch event is not published
    And Workflow Instance status is Created


