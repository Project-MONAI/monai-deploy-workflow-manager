Feature: ExecutionStats

Task Manager will keep a log of the execution stats for Tasks

@ExecutionStats
Scenario: Execution Stats table is populated when after consuming a TaskDispatchEvent
	Given A Task Dispatch event is published Task_Dispatch_Accepted
    Then the Execution Stats table is populated correctly for a TaskDispatchEvent

@ExecutionStats
Scenario Outline: Execution Stats table is updated after consuming a TaskCallbackEvent
	Given Execution Stats table is populated with <values>
    When A Task Callback event is published Task_Dispatch_Accepted
    Then the Execution Stats table is populated correctly for a TaskCallbackEvent
    Examples:
    | values |
    | test   |

@ExecutionStats
Scenario Outline: Summary of Execution Stats are returned
	Given Execution Stats table is populated with <values>
    And I have a TaskManager endpoint /statsoverview
	When I send a GET request
	Then I will get a 200 response
    And I can see expected summary execution stats are returned
    Examples:
    | values |
    | test   |

@ExecutionStats
Scenario Outline: Execution Stats for a Task are returned
	Given Execution Stats table is populated with <values>
    And I have a TaskManager endpoint /stats
	When I send a GET request
	Then I will get a 200 response
    And I can see expected execution stats are returned
    Examples:
    | values |
    | test   |
