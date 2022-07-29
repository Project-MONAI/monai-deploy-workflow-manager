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

Feature: TaskUpdate

Integration tests for testing TaskUpdateEvents from TaskManager

@TaskDispatch_TaskUpdate
Scenario: TaskUpdateEvent is published with status Accepted after receiving a valid TaskDispatchEvent
	Given I have a bucket in MinIO bucket1
	When A Task Dispatch event is published Task_Dispatch_Accepted
    Then A Task Update event with status Accepted is published with Task Dispatch details

@TaskDispatch_TaskUpdate
Scenario Outline: TaskUpdateEvent is published with status Failed after receiving an invalid TaskDispatchEvent
    Given I have a bucket in MinIO bucket1
    When A Task Dispatch event is published <TaskDispatchEvent>
    Then A Task Update event with status Failed is published with Task Dispatch details
    Examples:
    | TaskDispatchEvent                                  |
    | Task_Dispatch_Invalid_Input_Missing                |
    | Task_Dispatch_Invalid_ExecutionId_Missing          |
    | Task_Dispatch_Invalid_PayloadId_Missing            |
    | Task_Dispatch_Invalid_TaskId_Missing               |
    | Task_Dispatch_Invalid_TaskPluginType_NotSupported  |
    | Task_Dispatch_Clinical_Review_WorkflowName_Missing |
    | Task_Dispatch_Clinical_Review_QueueName_Missing    |

@TaskCallback_TaskUpdate
Scenario: TaskUpdateEvent is published with status Successful after receiving a valid TaskCallbackEvent
    Given I have a bucket in MinIO bucket1
    When A Task Dispatch event is published Task_Dispatch_Basic
    Then A Task Update event with status Accepted is published with Task Dispatch details
    And The Task Dispatch event is saved in mongo
    And A Task Callback event is published Task_Callback_Basic
    And A Task Update event with status Succeeded is published with Task Callback details
    And The Task Dispatch event is deleted in mongo
