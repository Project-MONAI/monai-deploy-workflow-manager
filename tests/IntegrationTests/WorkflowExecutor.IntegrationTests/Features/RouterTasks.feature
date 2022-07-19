Feature: RouterTasks

Workflow supports Router Tasks

@RouterTasks
Scenario: Routing Task dispatches single plug-in task
    Given I have a clinical workflow Routing_Workflow_Single_Destination
    And I have a bucket in MinIO bucket1
    When I publish a Workflow Request Message Routing_Workflow_Request
    Then I can see 1 Workflow Instance is created
    And Task Dispatch events for TaskIds taskdest1 are published

@RouterTasks
Scenario: Routing Task dispatches multiple plug-in tasks
    Given I have a clinical workflow Routing_Workflow_Multi_Destination
    And I have a bucket in MinIO bucket1
    When I publish a Workflow Request Message Routing_Workflow_Request
    Then I can see 1 Workflow Instance is created
    And Task Dispatch events for TaskIds taskdest1, taskdest2 are published

@RouterTasks
Scenario: Routing Task dispatches single plug-in task based on conditional statements
    Given I have a clinical workflow Routing_Workflow_Single_Destination_Conditional
    And I have a bucket in MinIO bucket1
    When I publish a Workflow Request Message Routing_Workflow_Request
    Then I can see 1 Workflow Instance is created
    And Task Dispatch events for TaskIds taskdest1 are published

@RouterTasks
Scenario: Routing Task dispatches no tasks based on conditional statements
    Given I have a clinical workflow Routing_Workflow_No_Destination_Conditional
    And I have a bucket in MinIO bucket1
    When I publish a Workflow Request Message Routing_Workflow_Request
    Then I can see 1 Workflow Instance is created
    And A Task Dispatch event is not published

@RouterTasks
Scenario: Routing Task dispatches additonal plugin task and triggers another router task
    Given I have a clinical workflow Routing_Workflow_Multi_Router
    And I have a bucket in MinIO bucket1
    When I publish a Workflow Request Message Routing_Workflow_Request
    Then I can see 1 Workflow Instance is created
    And Task Dispatch events for TaskIds taskdest1, taskdest2, taskdest3 are published

@RouterTasks
Scenario: Routing Task attempts to dispatch task which does not exist in workflow
    Given I have a clinical workflow Routing_Workflow_Invalid_Destination
    And I have a bucket in MinIO bucket1
    When I publish a Workflow Request Message Routing_Workflow_Request
    Then I can see 1 Workflow Instance is created
    And A Task Dispatch event is not published
