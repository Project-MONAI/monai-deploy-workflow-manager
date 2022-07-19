Feature: TaskDestinations

New task is dispatched after a Task update message is received.

@TaskUpdate
Scenario: Publish a valid Task Update event which triggers a single new task
    Given I have a clinical workflow Multi_Task_Workflow_3
    And I have a Workflow Instance WFI_Multi_Task_3
    When I publish a Task Update Message Task_Update_To_Dispatch_Single_Task with status Succeeded
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
Scenario Outline: Publish a valid Task Update event where the Task status is not created
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
Scenario: Publish a valid Task Update event as failed which does not trigger a new task and updates the workflow status to Failed
    Given I have a clinical workflow Multi_Task_Workflow_1
    And I have a Workflow Instance WFI_Multi_Task_1
    When I publish a Task Update Message Task_Update_Dispatches_Single_Task with status Failed
    Then A Task Dispatch event is not published
    And Workflow Instance status is Failed

@TaskDestinationConditions
Scenario: Task destination with condition true, WFI is updated with Task and task dispatch message is published
    Given I have a clinical workflow Multi_Task_Workflow_Destination_Single_Condition_True
    And I have a Workflow Instance WFI_Task_Destination_Condition_True
    When I publish a Task Update Message Task_Update_Task_Destination_Condition_True with status Succeeded
    Then 1 Task Dispatch event is published

@TaskDestinationConditions
Scenario: Task destination with condition false, WFI is not updated with Task and task dispatch message is not published
    Given I have a clinical workflow Multi_Task_Workflow_Destination_Single_Condition_False
    And I have a Workflow Instance WFI_Task_Destination_Condition_False
    When I publish a Task Update Message Task_Update_Task_Destination_Condition_False with status Succeeded
    Then A Task Dispatch event is not published

@TaskDestinationConditions
Scenario: Multiple task destinations with condition true, multiple task dispatch messages sent
    Given I have a clinical workflow Multi_Task_Workflow_Multiple_Destination_Single_Condition_True
    And I have a Workflow Instance WFI_Task_Multiple_Destination_Condition_True
    When I publish a Task Update Message Task_Update_Task_Multiple_Destination_Condition_True with status Succeeded
    Then 3 Task Dispatch events are published

@TaskDestinationConditions
Scenario: Multiple task destinations with condition false, no task dispatch messages sent
    Given I have a clinical workflow Multi_Task_Workflow_Multiple_Destination_Single_Condition_False
    And I have a Workflow Instance WFI_Task_Multiple_Destination_Condition_False
    When I publish a Task Update Message Task_Update_Task_Multiple_Destination_Condition_False with status Succeeded
    Then A Task Dispatch event is not published

@TaskDestinationConditions
Scenario: Multiple task destinations one with condition true and one with false, 1 task dispatch message published for task which is true
    Given I have a clinical workflow Multi_Task_Workflow_Destination_Multiple_Condition_True_And_False
    And I have a Workflow Instance WFI_Task_Destination_Condition_True_And_False
    When I publish a Task Update Message Task_Update_Task_Destination_Condition_True_And_False with status Succeeded
    Then 1 Task Dispatch event is published
    And The Task Dispatch event is for Task Id b9964b10-acb4-4050-a610-374fdbe2100d

@TaskDestinationConditions
Scenario: Workflow instance status remains created when any task status is either dispatch or accepted
    Given I have a clinical workflow Multi_Task_Workflow_Destination_Single_Condition_True
    And I have a Workflow Instance WFI_Task_Destination_Condition_True
    When I publish a Task Update Message Task_Update_Task_Destination_Condition_True with status Succeeded
    Then 1 Task Dispatch event is published
    And Workflow Instance status is Created

@TaskDestinationConditions
Scenario: Workflow instance status is failed when a condition is invalid
    Given I have a clinical workflow Multi_Task_Workflow_Task_Destination_Invalid_Condition
    And I have a Workflow Instance WFI_Task_Destination_Invalid_Condition
    When I publish a Task Update Message Task_Update_Task_Destination_Invalid_Condition with status Succeeded
    Then A Task Dispatch event is not published
    And Workflow Instance status is Succeeded

@TaskDestinationConditions
Scenario: Task destination based on Dicom data conditional is successful, and task is completed and marked as succeeded
    Given I have a clinical workflow Workflow_Revision_for_bucket_minio
    And I have a Workflow Instance Workflow_instance_for_bucket_minio
    And I have a bucket in MinIO bucket1
    And I have a payload patient_1_lordge in the bucket bucket1 with payload id 5450c3a9-2b19-45b0-8b17-fb10f89d1b2d 
    When I publish a Task Update Message Task_status_update_for_bucket_minio with status Succeeded 
    Then I can see the status of the Task is Succeeded
