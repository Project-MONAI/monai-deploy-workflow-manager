# Copyright 2022 MONAI Consortium
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

@IntergrationTests
Feature: WorkflowApi

API to interact with WorkflowRevisions collection

@GetWorkflows
Scenario: Get all clinical workflows from API - Single workflow
    Given I have an endpoint /workflows
    And I have a clinical workflow Basic_Workflow_1
    When I send a GET request
    Then I will get a 200 response
    And I can see 1 workflow is returned

@GetWorkflows
Scenario: Get all clinical workflows from API - Multiple workflows
    Given I have an endpoint /workflows
    And I have a clinical workflow Basic_Workflow_2
    And I have a clinical workflow Basic_Workflow_3
    When I send a GET request
    Then I will get a 200 response
    And I can see 2 workflows are returned

@GetWorkflows
Scenario: Get all clinical workflows from API - No workflows
    Given I have an endpoint /workflows
    When I send a GET request
    Then I will get a 200 response
    And I can see 0 workflows are returned

@WorkflowPagination
Scenario Outline: Get all clinical workflows from API - Test pagination
    Given I have an endpoint /workflows/<pagination_query>
    And I have <amount> clinical workflows
    When I send a GET request
    Then I will get a 200 response
    And Pagination is working correctly for the <pagination_count> workflows
    Examples:
    | pagination_query           | amount | pagination_count |
    | ?pageSize=1                | 15     | 15               |
    | ?pageNumber=10             | 15     | 15               |
    | ?pageNumber=1&pageSize=10  | 15     | 15               |
    | ?pageSize=10&pageNumber=2  | 13     | 13               |
    | ?pageNumber=2&pageSize=7   | 4      | 4                |
    | ?pageNumber=3&pageSize=10  | 7      | 7                |
    | ?pageNumber=1&pageSize=3   | 10     | 10               |
    |                            | 15     | 15               |
    |                            | 3      | 3                |
    | ?pageNumber=3&pageSize=10  | 0      | 0                |
    | ?pageNumber=1              | 0      | 0                |
    |                            | 0      | 0                |
    | ?pageNumber=1&pageSize=100 | 15     | 15               |

@WorkflowPagination
Scenario Outline: Invalid pagination returns 400
    Given I have an endpoint /workflows/<pagination_query>
    And I have 10 clinical workflows
    When I send a GET request
    Then I will get a 400 response
    And I will recieve the error message <error_message>
    Examples:
    | pagination_query                           | error_message                                                                                                           |
    | ?pageSize=NotANumber                       | The value 'NotANumber' is not valid for PageSize.                                                                       |
    | ?pageNumber=NotANumber                     | The value 'NotANumber' is not valid for PageNumber.                                                                     |
    | ?pageNumber=NotANumber&pageSize=NotANumber | The value 'NotANumber' is not valid for PageSize."],"PageNumber":["The value 'NotANumber' is not valid for PageNumber. |
    | ?pageSize=10000000000000&pageNumber=2      | The value '10000000000000' is not valid for PageSize.                                                                   |
    | ?pageNumber=10000000000000&pageSize=1      | The value '10000000000000' is not valid for PageNumber.                                                                 |


@UpdateWorkflows
Scenario: Update workflow with valid details
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3
    And I have a body Basic_Workflow_1
    When I send a PUT request
    Then I will get a 201 response
    And the Workflow Id c86a437d-d026-4bdf-b1df-c7a6372b89e3 is returned in the response body
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
    | endpoint                                        | put_body                           | message                                                 |
    | /workflows/1                                    | Basic_Workflow_1                   | Failed to validate id, not a valid guid                 |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Name_Length       | is not a valid Workflow Name                            |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Desc_Length       | is not a valid Workflow Description                     |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_AETitle_Length    | is not a valid AE Title                                 |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_ExportDest        | is not a valid Informatics Gateway - exportDestinations |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_TaskDesc_Length   | is not a valid taskDescription                          |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_TaskType_Length   | is not a valid taskType                                 |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_TaskID_Length     | is not a valid taskId                                   |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_TaskID_Content    | Contains Invalid Characters.                            |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Unreferenced_Task | Found Task(s) without any task destinations to it       |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Loopback_Task     | Detected task convergence on path                       |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_0_Tasks           | Missing Workflow Tasks                                  |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Version_Null      | Missing Workflow Version                                |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Version_Blank     | Missing Workflow Version                                |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Invalid_Workflow_Body_Object       | 'informaticsGateway' cannot be null                     |
    | /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3 | Empty_Workflow_Body                | '' is not a valid Workflow Description                  |


@UpdateWorkflows
Scenario: Update workflow where workflow ID does not exist
    Given I have a clinical workflow Basic_Workflow_1
    And  I have an endpoint /workflows/52b87b54-a728-4796-9a79-d30867da2a6e
    And I have a body Basic_Workflow_1
    When I send a PUT request
    Then I will get a 404 response
    And I will recieve the error message Failed to find workflow with Id: 52b87b54-a728-4796-9a79-d30867da2a6e

@AddWorkflows
Scenario: Add workflow with valid details
    Given  I have an endpoint /workflows
    And I have a body Basic_Workflow_1
    When I send a POST request
    Then I will get a 201 response

@AddWorkflows
Scenario: Add workflow with valid empty details
    Given  I have an endpoint /workflows
    And I have a body Basic_Workflow_2
    When I send a POST request
    Then I will get a 201 response

@AddWorkflows
Scenario Outline: Add workflow with invalid details
    Given  I have an endpoint /workflows
    And I have a body <post_body>
    When I send a POST request
    Then I will get a 400 response
    And I will recieve the error message <message>
    Examples:
    | post_body                          | message                                                 |
    | Invalid_Workflow_Name_Length       | is not a valid Workflow Name                            |
    | Invalid_Workflow_Desc_Length       | is not a valid Workflow Description                     |
    | Invalid_Workflow_AETitle_Length    | is not a valid AE Title                                 |
    | Invalid_Workflow_ExportDest        | is not a valid Informatics Gateway - exportDestinations |
    | Invalid_Workflow_TaskDesc_Length   | is not a valid taskDescription                          |
    | Invalid_Workflow_TaskType_Length   | is not a valid taskType                                 |
    | Invalid_Workflow_TaskID_Length     | is not a valid taskId                                   |
    | Invalid_Workflow_TaskID_Content    | Contains Invalid Characters.                            |
    | Invalid_Workflow_Unreferenced_Task | Found Task(s) without any task destinations to it       |
    | Invalid_Workflow_Loopback_Task     | Detected task convergence on path                       |
    | Invalid_Workflow_0_Tasks           | Missing Workflow Tasks                                  |
    | Invalid_Workflow_Version_Null      | Missing Workflow Version                                |
    | Invalid_Workflow_Version_Blank     | Missing Workflow Version                                |
    | Invalid_Workflow_Body_Object       | 'informaticsGateway' cannot be null                     |
    | Empty_Workflow_Body                | '' is not a valid Workflow Description                  |

@DeleteWorkflows
Scenario: Delete a workflow with one revision
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3
    When I send a DELETE request
    Then I will get a 200 response
    And all revisions of the workflow are marked as deleted

@DeleteWorkflows
Scenario: Delete a workflow with multiple revisions
    Given I have a clinical workflow Basic_Workflow_multiple_revisions_1
    And I have a clinical workflow Basic_Workflow_multiple_revisions_2
    And  I have an endpoint /workflows/570611d3-ad74-43a4-ae84-539164ee8f0c
    When I send a DELETE request
    Then I will get a 200 response
    And all revisions of the workflow are marked as deleted

@DeleteWorkflows
Scenario: Delete workflow with invalid details
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint /workflows/1
    When I send a DELETE request
    Then I will get a 400 response
    And I will recieve the error message Failed to validate id, not a valid guid

@DeleteWorkflows
Scenario: Delete workflow where workflow ID does not exist
    Given I have a clinical workflow Basic_Workflow_1
    And  I have an endpoint /workflows/52b87b54-a728-4796-9a79-d30867da2a6e
    When I send a DELETE request
    Then I will get a 404 response
    And I will recieve the error message Failed to validate id, workflow not found

@DeleteWorkflows
Scenario: Delete a workflow and recieve 404 when trying to GET by ID
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3
    And I send a DELETE request
    When I send a GET request
    Then I will get a 404 response
    And I will recieve the error message Failed to validate id, workflow not found

@DeleteWorkflows
Scenario: Delete a workflow and recieve 404 when trying to UPDATE by ID
    Given I have a clinical workflow Basic_Workflow_1_static
    And  I have an endpoint /workflows/c86a437d-d026-4bdf-b1df-c7a6372b89e3
    And I send a DELETE request
    And I have a body Basic_Workflow_1
    When I send a PUT request
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
