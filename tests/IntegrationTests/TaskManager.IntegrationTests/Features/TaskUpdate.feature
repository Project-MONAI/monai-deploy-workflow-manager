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
Feature: TaskUpdate

Integration tests for testing TaskUpdateEvents from TaskManager

@Ignore
@TaskDispatch_TaskUpdate
Scenario: TaskUpdateEvent is published with status Accepted after receiving a valid TaskDispatchEvent
	When A Task Dispatch event is published Task_Dispatch_Accepted
    Then A Task Update event with status Accepted is published with Task Dispatch details

@Ignore
@TaskDispatch_TaskUpdate
Scenario Outline: TaskUpdateEvent is published with status Failed after receiving an invalid TaskDispatchEvent
    When A Task Dispatch event is published <TaskDispatchEvent>
    Then A Task Update event with status Failed is published with Task Dispatch details
    Examples:
    | TaskDispatchEvent                                  |
    | Task_Dispatch_Invalid_Input_Missing                |
    | Task_Dispatch_Invalid_ExecutionId_Missing          |
    | Task_Dispatch_Invalid_PayloadId_Missing            |
    | Task_Dispatch_Invalid_TaskId_Missing               |
    | Task_Dispatch_Invalid_TaskPluginType_NotSupported  |

@Ignore
@TaskCallback_TaskUpdate
Scenario Outline: TaskUpdateEvent is published with correct status upon receiving a valid TaskCallbackEvent
    And I have Task Dispatch Info saved in Mongo Task_Dispatch_Basic_Clinical_Review
    When A Task Callback event is published <taskCallbackEvent>
    Then A Task Update event with status <status> is published with Task Callback details
    Examples:
    | taskCallbackEvent          | status      |
    | Task_Callback_Succeeded    | Succeeded   |
    | Task_Callback_Partial_Fail | PartialFail |

@TaskDispatch_Persistance
Scenario: TaskDispatchEvent with different permutations is published and matching TaskDispatchEvent is saved in Mongo
    When A Task Dispatch event is published <taskDispatchMessage>
    Then The Task Dispatch event is saved in mongo
    Examples:
    | taskDispatchMessage                 |
    | Task_Dispatch_Basic_Clinical_Review |
    | Task_Dispatch_Invalid               |
