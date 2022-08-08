Feature: Tasks Api

API to retrieve task status.

@Task_Api
Scenario Outline: Get details of a Task with valid payload
    Given I have an endpoint /tasks
    And I have a Task Request body <taskRequest>
    And I have a Workflow Instance WorkflowInstance_TaskApi_1 with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see an individual task is returned
    Examples:
    | taskRequest          |
    | Valid_Task_Details_1 |
    | Valid_Task_Details_2 |

@Task_Api
Scenario Outline: Get details of a Task with invalid payload
    Given I have an endpoint /tasks
    And I have a Task Request body <taskRequest>
    And I have a Workflow Instance WorkflowInstance_TaskApi_1 with no artifacts
    When I send a GET request
    Then I will get a 400 response
    Examples:
    | taskRequest                        |
    | Invalid_WorkflowID_Task_Details_1  |
    | Invalid_ExecutionID_Task_Details_2 |
    | Invalid_TaskID_Task_Details_3      |

@Task_Api
Scenario Outline: Get details of a Task with non-existent id payload
    Given I have an endpoint /tasks
    And I have a Task Request body <taskRequest>
    And I have a Workflow Instance WorkflowInstance_TaskApi_1 with no artifacts
    When I send a GET request
    Then I will get a 404 response
    Examples:
    | taskRequest                             |
    | Non_Existent_WorkflowID_Task_Details_1  |
    | Non_Existent_ExecutionID_Task_Details_2 |
    | Non_Existent_TaskID_Task_Details_3      |

@Task_Api
Scenario: Get details of all Tasks
    Given I have an endpoint /tasks/running?pageNumber=1&pageSize=10
    And I have a Workflow Instance WorkflowInstance_TaskApi_1 with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see task payload is returned
