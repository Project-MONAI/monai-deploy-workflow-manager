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
    And I will receive the error message Failed to validate ids, not a valid guid
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
    And I will receive the error message Failed to validate ids, workflow or task not found
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
    And I can see 3 tasks are returned

@Task_Api
Scenario: Get details of all Tasks From Multiple Workflows
    Given I have an endpoint /tasks/running?pageNumber=1&pageSize=10
    And I have a Workflow Instance WorkflowInstance_TaskApi_1 with no artifacts
    And I have a Workflow Instance WorkflowInstance_TaskApi_2 with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see 6 tasks are returned

@Task_Api
Scenario: Get details of all Tasks From Multiple Workflows returns no tasks
    Given I have an endpoint /tasks/running?pageNumber=1&pageSize=10
    And I have a Workflow Instance WorkflowInstance_TaskApi_3 with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see 0 tasks are returned
