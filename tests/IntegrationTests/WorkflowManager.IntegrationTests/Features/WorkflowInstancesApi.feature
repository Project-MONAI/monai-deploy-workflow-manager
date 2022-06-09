Feature: WorkflowInstancesApi

Workflow instances get requests

@WorkflowInstanceApi
Scenario: Get all workflows instances - multiple
	Given I have an endpoint /workflowinstances
    And I have a Workflow Instance Existing_WFI_Created
    And I have a Workflow Instance Existing_WFI_Dispatched
    And I have a Workflow Instance WFI_Multi_Task_Dispatched
    When I send a GET request
    Then I will get a 200 response
    And I can see expected workflow instances are returned

@WorkflowInstanceApi
Scenario: Get all workflows instances - single
	Given I have an endpoint /workflowinstances
    And I have a Workflow Instance Existing_WFI_Created
    When I send a GET request
    Then I will get a 200 response
    And I can see expected workflow instances are returned

@WorkflowInstanceApi
Scenario: Get all workflows instances - empty
	Given I have an endpoint /workflowinstances
    When I send a GET request
    Then I will get a 200 response
    And I can see expected workflow instances are returned

@WorkflowInstanceApi
Scenario: Get all workflows instances by Id
	Given I have an endpoint /workflowinstances/bff4cfd0-3af3-4e2b-9f3c-de2a6f2b9569
    And I have a Workflow Instance WFI_Static_1
    And I have a Workflow Instance WFI_Static_2
    When I send a GET request
    Then I will get a 200 response
    And I can see expected workflow instance is returned

@WorkflowInstanceApi
Scenario: Get all workflows instances by Id. Id Not Found
	Given I have an endpoint /workflowinstances/bff4cfd0-3af3-4e2b-9f3c-de2a6f2b9575
    And I have a Workflow Instance WFI_Static_1
    When I send a GET request
    Then I will get a 404 response

@WorkflowInstanceApi
Scenario: Get all workflows instances by Id. Id Bad Request
	Given I have an endpoint /workflowinstances/absfsushs
    And I have a Workflow Instance WFI_Static_1
    When I send a GET request
    Then I will get a 400 response
