Feature: TaskStatusUpdate

Update task status in the workflow instance

Scenario Outline: Publish a valid Task Update event which updates the Task status
    Given I have a Workflow Instance Existing_WFI_Created
    When I publish a Task Update Message <taskUpdateMessage>
    Then I can see the status of the Task is updated
    Examples:
    | workflow              | workflowRequestMessage     |
    | Basic_Workflow_1      | Basic_AeTitle_WF_Request   |
    | Basic_Workflow_1      | Basic_Id_WF_Request        |
