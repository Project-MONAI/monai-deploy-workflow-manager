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
Feature: PayloadApi

API to interact with Payloads collection

@GetPayloads
Scenario: Get all payloads from API - Single payload
    Given I have an endpoint /payload
    And I have a payload saved in mongo Payload_Full_Patient
    When I send a GET request
    Then I will get a 200 response
    And I can see expected Payloads are returned

@GetPayloads
Scenario: Get all payloads from API - multiple payloads
    Given I have an endpoint /payload
    And I have a payload saved in mongo Payload_Full_Patient
    And I have a payload saved in mongo Payload_Partial_Patient
    And I have a payload saved in mongo Payload_Null_Patient
    When I send a GET request
    Then I will get a 200 response
    And I can see expected Payloads are returned

@GetPayloads
Scenario: Get all payloads from API - no payloads
    Given I have an endpoint /payload
    When I send a GET request
    Then I will get a 200 response
    And I can see no Payloads are returned

@GetPayloads
Scenario: Get all payloads from API - Workflow Instances Created - PayloadStatus InProgress
    Given I have an endpoint /payload
    And I have a payload saved in mongo Payload_PayloadStatus_1
    And I have a Workflow Instance Workflow_Instance_For_PayloadData_Payload_PayloadStatus_1_1 with no artifacts
    And I have a Workflow Instance Workflow_Instance_For_PayloadData_Payload_PayloadStatus_1_2 with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see expected Payloads are returned with PayloadStatus InProgress

@GetPayloads
Scenario: Get all payloads from API - Workflow Instances Succeeded and Failed - PayloadStatus Complete
    Given I have an endpoint /payload
    And I have a payload saved in mongo Payload_PayloadStatus_2
    And I have a Workflow Instance Workflow_Instance_For_PayloadData_Payload_PayloadStatus_2_1 with no artifacts
    And I have a Workflow Instance Workflow_Instance_For_PayloadData_Payload_PayloadStatus_2_2 with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see expected Payloads are returned with PayloadStatus Complete

@GetPayloads
Scenario: Get all payloads from API - Workflow Instances Succeeded, Failed, and Created - PayloadStatus InProgress
    Given I have an endpoint /payload
    And I have a payload saved in mongo Payload_PayloadStatus_3
    And I have a Workflow Instance Workflow_Instance_For_PayloadData_Payload_PayloadStatus_3_1 with no artifacts
    And I have a Workflow Instance Workflow_Instance_For_PayloadData_Payload_PayloadStatus_3_2 with no artifacts
    And I have a Workflow Instance Workflow_Instance_For_PayloadData_Payload_PayloadStatus_3_3 with no artifacts
    When I send a GET request
    Then I will get a 200 response
    And I can see expected Payloads are returned with PayloadStatus InProgress

@PayloadSearch
Scenario Outline: Get all payloads from API - Test search query parameters
    Given I have an endpoint /payload/<search_query>
    And I have 10 Payloads
    When I send a GET request
    Then I will get a 200 response
    And Search is working correctly for the <search_count> payloads
    Examples:
    | search_query                                                           | search_count |
    | ?patientName=Steve%20Jobs                                              | 1            |
    | ?patientName=Steve                                                     | 1            |
    | ?patientId=dae4a6d1-573d-4a3f-978f-ed056f628de6                        | 1            |
    | ?patientId=dae4a6d1-573d-4a3f-978f-ed056                               | 1            |
    | ?patientName=Jane%20Doe&patientId=dae4a6d1-573d-4a3f-978f-ed056f628de6 | 1            |
    | ?patientName=Jane&patientId=dae4a6d1-573d-4a3f-978f-ed056f628de6       | 1            |
    | ?patientId=09da8f2c-c0ae-4de6-9b66-28a2bed6c2f6                        | 3            |


@PayloadPagination
Scenario Outline: Get all payloads from API - Test pagination
    Given I have an endpoint /payload/<pagination_query>
    And I have <amount> Payloads
    When I send a GET request
    Then I will get a 200 response
    And Pagination is working correctly for the <pagination_count> payloads
    Examples:
    | pagination_query           | amount | pagination_count |
    | ?pageSize=1                | 11     | 11               |
    | ?pageNumber=10             | 11     | 11               |
    | ?pageNumber=1&pageSize=10  | 11     | 11               |
    | ?pageSize=10&pageNumber=2  | 10     | 10               |
    | ?pageNumber=2&pageSize=7   | 4      | 4                |
    | ?pageNumber=3&pageSize=10  | 7      | 7                |
    | ?pageNumber=1&pageSize=3   | 10     | 10               |
    |                            | 11     | 11               |
    |                            | 3      | 3                |
    | ?pageNumber=3&pageSize=10  | 0      | 0                |
    | ?pageNumber=1              | 0      | 0                |
    |                            | 0      | 0                |
    | ?pageNumber=1&pageSize=100 | 11     | 11               |

@PayloadPagination
Scenario Outline: Invalid pagination returns 400
    Given I have an endpoint /payload/<pagination_query>
    And I have 10 Payloads
    When I send a GET request
    Then I will get a 400 response
    And I will receive the error message <error_message>
    Examples:
    | pagination_query                           | error_message                                                                                                           |
    | ?pageSize=NotANumber                       | The value 'NotANumber' is not valid for PageSize.                                                                       |
    | ?pageNumber=NotANumber                     | The value 'NotANumber' is not valid for PageNumber.                                                                     |
    | ?pageNumber=NotANumber&pageSize=NotANumber | The value 'NotANumber' is not valid for PageSize."],"PageNumber":["The value 'NotANumber' is not valid for PageNumber. |
    | ?pageSize=10000000000000&pageNumber=2      | The value '10000000000000' is not valid for PageSize.                                                                   |
    | ?pageNumber=10000000000000&pageSize=1      | The value '10000000000000' is not valid for PageNumber.                                                                 |

@GetPayloadById
Scenario Outline: Get payload by Id returns 200
    Given I have an endpoint /payload/c5c3636b-81dd-44a9-8c4b-71adec7d47b2
    And I have a payload saved in mongo Payload_Full_Patient
    And I have a payload saved in mongo Payload_Partial_Patient
    When I send a GET request
    Then I will get a 200 response
    And I can see expected Payload is returned

@GetPayloadById
Scenario Outline: Get payload by Id returns 404
    Given I have an endpoint /payload/c5c3636b-81dd-44a9-8c4b-71adec7d47c3
    And I have a payload saved in mongo Payload_Full_Patient
    And I have a payload saved in mongo Payload_Partial_Patient
    When I send a GET request
    Then I will get a 404 response
    And I will receive the error message Failed to find payload with payload id: c5c3636b-81dd-44a9-8c4b-71adec7d47c3


@GetPayloadById
Scenario Outline: Get payload by Id returns 400
    Given I have an endpoint /payload/hagaaaa
    And I have a payload saved in mongo Payload_Full_Patient
    When I send a GET request
    Then I will get a 400 response
    And I will receive the error message Failed to validate id, not a valid guid

@DeletePayloadById
Scenario Outline: Delete payload by Id returns 400 with invalid payload ID
    Given I have an endpoint /payload/invalid-payload-id
    When I send a DELETE request
    Then I will get a 400 response
    And I will receive the error message Failed to validate id, not a valid guid

@DeletePayloadById
Scenario Outline: Delete payload by ID returns 404 when no payload exists
    Given I have an endpoint /payload/c5c3635b-81dd-44a9-8c3b-71adec7d47c6
    When I send a DELETE request
    Then I will get a 404 response
    And I will receive the error message Payload with ID: c5c3635b-81dd-44a9-8c3b-71adec7d47c6 not found

@DeletePayloadById
Scenario Outline: Delete payload by ID returns 400 when PayloadDeleted is already InProgress
    Given I have an endpoint /payload/c5c3635b-81dd-44a9-8c3b-71adec7d47c6
    And I have a payload saved in mongo Payload_PayloadDeleted_InProgress
    When I send a DELETE request
    Then I will get a 400 response
    And I will receive the error message Deletion of files for payload ID: c5c3635b-81dd-44a9-8c3b-71adec7d47c6 already in progress or already deleted

@DeletePayloadById
Scenario Outline: Delete payload by ID returns 202
    Given I have an endpoint /payload/d5c3633b-41de-44a9-8c3a-71adec3d47c1
    And I have a payload saved in mongo Payload_PayloadDeleted_No
    When I send a DELETE request
    Then I will get a 202 response
