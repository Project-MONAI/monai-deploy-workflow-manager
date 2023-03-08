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
Feature: PayloadCollection

Tests around the payload collection

@PatientDetails
Scenario: Payload collection will be populated with patient details after receiving a Workflow Request when patient metadata exists
    Given I have a clinical workflow Basic_Workflow_1
    When I publish a Workflow Request Message <workflowRequestMessage> with artifacts <objects> in minio
    Then A payload collection is created with patient details <patientTestData>
    And I can see <workflowInstance> Workflow Instance is created
    Examples:
    | workflowRequestMessage                     | objects                  | patientTestData | workflowInstance |
    | Basic_AeTitle_Payload_Collection_Request_1 | full_patient_metadata    | Full_Patient    | 1                |
    | Basic_Non_Existant_Request                 | partial_patient_metadata | Partial_Patient | 0                |

@PatientDetails
Scenario: Payload collection will be populated with null patient details after receiving a Workflow Request when patient metadata is blank
    Given I have a clinical workflow Basic_Workflow_1
    When I publish a Workflow Request Message Basic_AeTitle_Payload_Collection_Request_2 with artifacts null_patient_metadata in minio
    Then A payload collection is created with patient details Null_Patient
    And I can see 1 Workflow Instance is created

@PatientDetails
Scenario: Payload collection will be populated with null patient details after receiving a Workflow Request when patient metadata is missing
    Given I have a clinical workflow Basic_Workflow_1
    When I publish a Workflow Request Message Basic_AeTitle_Payload_Collection_Request_3 with artifacts no_patient_metadata in minio
    Then A payload collection is created with patient details Null_Patient
    And I can see 1 Workflow Instance is created

@PatientDetails
Scenario: Patient details are added to the Task Dispatch Event from the payload collection
    Given I have a clinical workflow Basic_Workflow_1
    When I publish a Workflow Request Message Basic_AeTitle_Payload_Collection_Request_1 with artifacts full_patient_metadata in minio
    Then A payload collection is created with 1 workflow instance id
    And A payload collection is created with patient details Full_Patient
    And Patient details Full_Patient have been added to task dispatch message

@WorkflowInstanceDetails
Scenario: Payload collection will be populated with workflow instance id after receiving a Workflow Request when workflow is matched
    Given I have a clinical workflow Basic_Workflow_1
    When I publish a Workflow Request Message Basic_AeTitle_Payload_Collection_Request_1 with artifacts full_patient_metadata in minio
    Then A payload collection is created with 1 workflow instance id

@WorkflowInstanceDetails
Scenario: Payload collection will be populated with workflow instance ids after receiving a Workflow Request when multiple workflows are matched
    Given I have a clinical workflow Basic_Workflow_1
    And I have a clinical workflow Basic_Workflow_2
    When I publish a Workflow Request Message Basic_AeTitle_Payload_Collection_Request_1 with artifacts full_patient_metadata in minio
    Then A payload collection is created with 2 workflow instance id

@WorkflowInstanceDetails
Scenario: Payload collection will not be populated with workflow instance ids after receiving a Workflow Request when no workflows are matched
    Given I publish a Workflow Request Message Basic_AeTitle_Payload_Collection_Request_1 with artifacts full_patient_metadata in minio
    Then A payload collection is created with 0 workflow instance id

