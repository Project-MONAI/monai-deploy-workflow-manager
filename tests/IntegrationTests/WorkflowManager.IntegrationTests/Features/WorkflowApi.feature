Feature: WorkflowApi

API to interact with WorkflowRevisions collection

@GetWorkflows
Scenario Outline: Get all clinical workflows from API - Single workflow
    Given I have an endpoint /workflows
    And I have a clinical workflow Basic_Workflow_1
    When I send a GET request
    Then I will get a 200 response
    And I can see 1 workflow is returned

@GetWorkflows
Scenario Outline: Get all clinical workflows from API - Multiple workflows
    Given I have an endpoint /workflows
    And I have a clinical workflow Basic_Workflow_2
    And I have a clinical workflow Basic_Workflow_3
    When I send a GET request
    Then I will get a 200 response
    And I can see 2 workflows are returned

@GetWorkflows
Scenario Outline: Get all clinical workflows from API - No workflows
    Given I have an endpoint /workflows
    When I send a GET request
    Then I will get a 200 response
    And I can see 0 workflows are returned

@UpdateWorkflows
Scenario: Update workflow with valid details
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3
    And I have a body Basic_Workflow_Update_1
    When I send a PUT request
    Then I will get a 201 response
    And the Id c86a437d-d026-4bdf-b1df-c7a6372b89e3 is returned in the response body
    And multiple workflow revisions now exist with correct details
    
@UpdateWorkflows
Scenario Outline: Update workflow with invalid details
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint <endpoint>
    And I have a body <put_body>
    When I send a PUT request
    Then I will get a 400 response
    And I will recieve the error message <message>
    Examples:
    | endpoint                                        | put_body                                | message                                                 |
    | /workflows/1                                    | Basic_Workflow_Update_1                 | Failed to validate id, not a valid guid                 |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_Name_Length     | is not a valid Workflow Name                            |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_Desc_Length     | is not a valid Workflow Description                     |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_AETitle_Length  | is not a valid AE Title                                 |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_DataOrg         | is not a valid Informatics Gateway - dataOrigins        |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_ExportDest      | is not a valid Informatics Gateway - exportDestinations |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_TaskDesc_Length | is not a valid taskDescription                          |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_TaskType_Length | is not a valid taskType                                 |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Update_TaskArgs        | is not a valid args                                     |

@UpdateWorkflows
Scenario: Update workflow where workflow ID does not exist
    Given I have a clinical workflow Basic_Workflow_1
    And  I have an endpoint /workflows/52b87b54-a728-4796-9a79-d30867da2a6e
    And I have a body Basic_Workflow_Update_1
    When I send a PUT request
    Then I will get a 404 response
    And I will recieve the error message Failed to find workflow with Id: 52b87b54-a728-4796-9a79-d30867da2a6e

@DeleteWorkflows
Scenario: Delete a workflow
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3
    When I send a DELETE request
    Then I will get a 200 response
    And all revisions of the workflow are marked as deleted

@DeleteWorkflows
Scenario: Delete workflow with invalid details
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3
    When I send a DELETE request
    Then I will get a 400 response
    And I will recieve the error message Failed to validate id, not a valid guid  

@DeleteWorkflows
Scenario: Delete workflow where workflow ID does not exist
    Given I have a clinical workflow Basic_Workflow_1
    And  I have an endpoint /workflows/52b87b54-a728-4796-9a79-d30867da2a6e
    When I send a DELETE request
    Then I will get a 404 response
    And I will recieve the error message Failed to find workflow with Id: 52b87b54-a728-4796-9a79-d30867da2a6e

@DeleteWorkflows
Scenario: Delete a workflow and recieve 404 when trying to GET by ID
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3
    And I send a DELETE request
    When I send a GET request
    Then I will get a 404 response
    And I will recieve the error message Failed to find workflow with Id: c86a437d-d026-4bdf-b1df-c7a6372b89e3

@DeleteWorkflows
Scenario: Delete a workflow and recieve 404 when trying to UPDATE by ID
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3
    And I send a DELETE request
    When I send a PUT request
    And I have a body Basic_Workflow_Update_1
    Then I will get a 404 response
    And I will recieve the error message Failed to find workflow with Id: c86a437d-d026-4bdf-b1df-c7a6372b89e3

@DeleteWorkflows
Scenario: Delete a workflow and recieve 404 when trying to GET all
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3
    And I send a DELETE request
    And I have an endpoint /workflows
    When I send a GET request
    Then the deleted workflow is not returned 
