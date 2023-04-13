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
Feature: RouterTasks

Workflow supports Router Tasks

@RouterTasks
Scenario: Routing Task dispatches single plug-in task
    Given I have a clinical workflow Routing_Workflow_Single_Destination
    When I publish a Workflow Request Message Routing_Workflow_Request with no artifacts
    Then I can see 1 Workflow Instance is created
    And Task Dispatch events for TaskIds taskdest1 are published

@RouterTasks
Scenario: Routing Task dispatches multiple plug-in tasks
    Given I have a clinical workflow Routing_Workflow_Multi_Destination
    When I publish a Workflow Request Message Routing_Workflow_Request with no artifacts
    Then I can see 1 Workflow Instance is created
    And Task Dispatch events for TaskIds taskdest1, taskdest2 are published

@RouterTasks
Scenario: Routing Task dispatches single plug-in task based on conditional statements
    Given I have a clinical workflow Routing_Workflow_Single_Destination_Conditional
    When I publish a Workflow Request Message Routing_Workflow_Request with no artifacts
    Then I can see 1 Workflow Instance is created
    And Task Dispatch events for TaskIds taskdest1 are published

@RouterTasks
Scenario: Routing Task dispatches no tasks based on conditional statements
    Given I have a clinical workflow Routing_Workflow_No_Destination_Conditional
    When I publish a Workflow Request Message Routing_Workflow_Request with no artifacts
    Then I can see 1 Workflow Instance is created
    And A Task Dispatch event is not published

@RouterTasks
Scenario: Routing Task dispatches additonal plugin task and triggers another router task
    Given I have a clinical workflow Routing_Workflow_Multi_Router
    When I publish a Workflow Request Message Routing_Workflow_Request with no artifacts
    Then I can see 1 Workflow Instance is created
    And Task Dispatch events for TaskIds taskdest1, taskdest2, taskdest3 are published

@RouterTasks
Scenario: Routing Task attempts to dispatch task which does not exist in workflow
    Given I have a clinical workflow Routing_Workflow_Invalid_Destination
    When I publish a Workflow Request Message Routing_Workflow_Request with no artifacts
    Then I can see 1 Workflow Instance is created
    And A Task Dispatch event is not published
