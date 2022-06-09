Feature: WorkflowAPI

API to interact with clinician workflows

@WorkflowAPI
Scenario: Get all clinical workflows from API - Single workflow
    Given I have an endpoint /workflows
    And I have a clinical workflow Basic_Workflow_1
    When I send a GET request
    Then I will get a 200 response
    And I can see 1 workflow is returned

@WorkflowAPI
Scenario: Get all clinical workflows from API - Multiple workflows
    Given I have an endpoint /workflows
    And I have a clinical workflow Basic_Workflow_2
    And I have a clinical workflow Basic_Workflow_3
    When I send a GET request
    Then I will get a 200 response
    And I can see 2 workflows are returned

@WorkflowAPI
Scenario: Get all clinical workflows from API - No workflows
    Given I have an endpoint /workflows
    When I send a GET request
    Then I will get a 200 response
    And I can see 0 workflows are returned

@WorkflowAPI
Scenario: Update workflow
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3
    And I have a body Basic_Workflow_Update_1
    When I send a PUT request
    Then I will get a 201 response
    And the ID c86a437d-d026-4bdf-b1df-c7a6372b89e3 is returned
    And a new revision is created

@WorkflowAPI
Scenario Outline: Update workflow - Invalid Input
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint <endpoint>
    And I have a body <put_body>
    When I send a PUT request
    Then I will get a 400 response
    And I will recieve the correct error message
    Examples:
    | endpoint                                        | put_body                                |
    | /workflows/1                                    | Basic_Workflow_Update_1                 |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_Name_Length     |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_Desc_Length     |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_AETitle_Length  |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_DataOrg         |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_ExportDest      |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_TaskDesc_Length |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_TaskType_Length |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_TaskArgs        |

@WorkflowAPI
Scenario: Update workflow - Non-existant workflow
    Given I have a clinical workflow Basic_Workflow_1
    And  I have an endpoint /workflows/52b87b54-a728-4796-9a79-d30867da2a6e
    And I have a body Basic_Workflow_Update_1
    When I send a PUT request
    Then I will get a 404 response
    And I will recieve the correct error message



