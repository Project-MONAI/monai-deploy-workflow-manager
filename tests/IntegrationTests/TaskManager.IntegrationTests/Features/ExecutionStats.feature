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

@IntegrationTests
Feature: ExecutionStats

Execution stats are returned for Tasks

@ExecutionStats
Scenario: Execution Stats table is populated when after consuming a TaskDispatchEvent
	Given A Task Dispatch event is published Task_Dispatch_Accepted
	And the Execution Stats table is populated correctly for a TaskDispatchEvent

@ExecutionStats @ignore
Scenario: Execution Stats table is updated after consuming a TaskCallbackEvent
    Given A Task Dispatch event is published Task_Dispatch_Execution_Stats
	And the Execution Stats table is populated correctly for a TaskDispatchEvent
    When A Task Callback event is published Task_Callback_Execution_Stats
    Then the Execution Stats table is populated correctly for a TaskCallbackEvent

@ExecutionStats
Scenario Outline: Summary of Execution Stats are returned
    Given Execution Stats table is populated
    And I have a TaskManager endpoint /tasks/statsoverview
    And I set the start time to be UTC <startTime>
    When I send a GET request
    Then I will get a 200 response
    And I can see expected summary execution stats are returned
    Examples:
    | startTime |
    | -61       |
    | -31       |

@ExecutionStats
Scenario Outline: Execution Stats for a Task are returned
    Given Execution Stats table is populated
    And I have a TaskManager endpoint /tasks/stats
    And I set WorkflowId as <workflowId> and TaskId as <taskId>
    When I send a GET request
    Then I will get a 200 response
    And I can see expected execution stats are returned
    Examples:
    | workflowId | taskId |
    | Workflow_1 | Task_1 |
    | Workflow_1 | Task_2 |

@ExecutionStats
Scenario Outline: Execution Stats are not returned if Workflow or Task is not found
    Given Execution Stats table is populated
    And I have a TaskManager endpoint /tasks/stats
    And I set WorkflowId as <workflowId> and TaskId as <taskId>
    When I send a GET request
    Then I will get a 200 response
    And I can see expected execution stats are returned
    Examples:
    | workflowId | taskId |
    | Workflow_2 | Task_1 |
    | Workflow_1 | Task_3 |

