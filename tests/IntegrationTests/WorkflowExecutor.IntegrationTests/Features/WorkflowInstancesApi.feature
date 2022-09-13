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
Feature: WorkflowInstancesApi

API to interact with WorkflowInstances collection

@GetWorkflowInstances
Scenario: Get all workflows instances - multiple
	Given I have an endpoint /workflowinstances
    And I have a Workflow Instance Existing_WFI_Created with no artifacts
    And I have a Workflow Instance Existing_WFI_Dispatched with no artifacts
    And I have a Workflow Instance WFI_Multi_Task_Dispatched with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see expected workflow instances are returned

@GetWorkflowInstances
Scenario: Get all workflows instances - single
	Given I have an endpoint /workflowinstances
    And I have a Workflow Instance Existing_WFI_Created with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see expected workflow instances are returned

@GetWorkflowInstances
Scenario: Get all workflows instances - empty
	Given I have an endpoint /workflowinstances
    When I send a GET request
    Then I will get a 200 response
    And I can see expected workflow instances are returned

@GetWorkflowInstances
Scenario: Get all triggered workflows instances for payload
    Given I have an endpoint /workflowinstances<query>
    And I have a Workflow Instance Existing_WFI_Created_Static_PayloadId with no artifacts
    And I have a Workflow Instance Existing_WFI_Dispatched_Static_PayloadId with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see 1 triggered workflow instances from payload id c2219298-44ec-44d6-b9c7-b2c3e5abaf45
    Examples:
    | query                                                                                           |
    | ?payloadId=c2219298-44ec-44d6-b9c7-b2c3e5abaf45                                                 |
    | ?payloadId=c2219298-44ec-44d6-b9c7-b2c3e5abaf45&pageNumber=1&pageSize=10                        |
    | ?payloadId=c2219298-44ec-44d6-b9c7-b2c3e5abaf45&pageNumber=1&pageSize=10&disablePagination=true |
    | ?payloadId=c2219298-44ec-44d6-b9c7-b2c3e5abaf45&disablePagination=true                          |

@WorkflowInstancePagination
Scenario Outline: Get all workflow instances from API - Test pagination
    Given I have an endpoint /workflowinstances<pagination_query>
    And I have <amount> Workflow Instances
    When I send a GET request
    Then I will get a 200 response
    And Pagination is working correctly for the <pagination_count> workflow instances
    Examples:
    | pagination_query                                                          | amount | pagination_count |
    | ?pageSize=1                                                               | 15     | 15               |
    | ?pageNumber=10                                                            | 15     | 15               |
    | ?pageNumber=1&pageSize=10                                                 | 15     | 15               |
    | ?pageSize=10&pageNumber=2                                                 | 13     | 13               |
    | ?pageNumber=2&pageSize=7                                                  | 4      | 4                |
    | ?pageNumber=3&pageSize=10                                                 | 7      | 7                |
    | ?pageNumber=1&pageSize=3                                                  | 10     | 10               |
    |                                                                           | 15     | 15               |
    |                                                                           | 3      | 3                |
    | ?pageNumber=3&pageSize=10                                                 | 0      | 0                |
    | ?pageNumber=1                                                             | 0      | 0                |
    |                                                                           | 0      | 0                |
    | ?pageNumber=1&pageSize=100                                                | 15     | 15               |
    | ?pageNumber=1&pageSize=100&payloadId=c2219298-44ec-44d6-b9c7-b2c3e5abaf45 | 15     | 1                |

@WorkflowInstancePagination
Scenario Outline: Get all workflow instances from API with provided status or PayloadId - Test pagination
    Given I have an endpoint /workflowinstances<pagination_query>
    And I have <amount> Workflow Instances
    When I send a GET request
    Then I will get a 200 response
    And Pagination is working correctly for the <pagination_count> workflow instances
    And All results have correct <expected_status> and <expected_payloadId>
    Examples:
    | pagination_query                                           | amount | pagination_count | expected_status | expected_payloadId                   |
    | ?pageSize=1                                                | 15     | 15               |                 |                                      |
    | ?pageSize=1&status=created                                 | 15     | 15               | 0               |                                      |
    | ?pageSize=1&payloadId=5450c3a9-2b19-45b0-8b17-fb10f89d1b2d | 15     | 1                |                 | 5450c3a9-2b19-45b0-8b17-fb10f89d1b2d |

@WorkflowInstancePagination
Scenario Outline: Invalid pagination returns 400
    Given I have an endpoint /workflowinstances<pagination_query>
    And I have 10 Workflow Instances
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

@WorkflowInstancePagination
Scenario Outline: Disable workflow instance pagination
	Given I have an endpoint /workflowinstances<query>
    And I have a Workflow Instance Existing_WFI_Created_Static_PayloadId with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I will recieve no pagination response
    Examples:
    | query                                                                                           |
    | ?disablePagination=true                                                                         |
    | ?pageNumber=1&pageSize=10&disablePagination=true                                                |
    | ?payloadId=c2219298-44ec-44d6-b9c7-b2c3e5abaf45&disablePagination=true                          |
    | ?payloadId=c2219298-44ec-44d6-b9c7-b2c3e5abaf45&pageNumber=1&pageSize=10&disablePagination=true |

@GetWorkflowInstances
Scenario: Get all workflows instances by Id
	Given I have an endpoint /workflowinstances/bff4cfd0-3af3-4e2b-9f3c-de2a6f2b9569
    And I have a Workflow Instance WFI_Static_1 with no artifacts
    And I have a Workflow Instance WFI_Static_2 with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see expected workflow instance is returned

@GetWorkflowInstances
Scenario: Get all workflows instances by Id. Id Not Found
	Given I have an endpoint /workflowinstances/bff4cfd0-3af3-4e2b-9f3c-de2a6f2b9575
    And I have a Workflow Instance WFI_Static_1 with no artifacts
    When I send a GET request
    Then I will get a 404 response

@GetWorkflowInstances
Scenario: Get all workflows instances by Id. Id Bad Request
	Given I have an endpoint /workflowinstances/absfsushs
    And I have a Workflow Instance WFI_Static_1 with no artifacts
    When I send a GET request
    Then I will get a 400 response
