Feature: WorkflowUpdateAPI

API to interact with clinician workflows

@WorkflowUpdateAPI
Scenario Outline: Get all clinical workflows from API - Single workflow
    Given I have an endpoint /workflows
    And I have a clinical workflow Basic_Workflow_1
    When I send a GET request
    Then I will get a 200 response
    And I can see 1 workflow is returned

@WorkflowUpdateAPI
Scenario Outline: Get all clinical workflows from API - Multiple workflows
    Given I have an endpoint /workflows
    And I have a clinical workflow Basic_Workflow_2
    And I have a clinical workflow Basic_Workflow_3
    When I send a GET request
    Then I will get a 200 response
    And I can see 2 workflows are returned

@WorkflowUpdateAPI
Scenario Outline: Get all clinical workflows from API - No workflows
    Given I have an endpoint /workflows
    When I send a GET request
    Then I will get a 200 response
    And I can see 0 workflows are returned


