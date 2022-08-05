Feature: Tasks Api

API to retrieve task status.

@Task_Api
Scenario Outline: Get details of a Task with valid payload
    Given I have an endpoint /tasks
    And I have a Task Request body <taskRequest>
    And I have a Workflow Instance WorkflowInstance_TaskApi_1 with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see task payload is returned
    Examples:
    | taskRequest          |
    | Valid_Task_Details_1 |
    | Valid_Task_Details_2 |

Scenario: Get details of all Tasks
    Given I have an endpoint /tasks/running?pageNumber=1&pageSize=10
    And I have a Workflow Instance WorkflowInstance_TaskApi_1 with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see task payload is returned
