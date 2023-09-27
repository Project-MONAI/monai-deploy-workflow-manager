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
Feature: WorkflowRequest

Publishing a workflow request is consumed by the Workflow Manager.

@WorkflowRequest
Scenario Outline: Publish a valid workflow request which creates a single workflow instance
    Given I have a clinical workflow <workflow>
    When I publish a Workflow Request Message <workflowRequestMessage> with no artifacts
    Then I can see 1 Workflow Instance is created
    And 1 Task Dispatch event is published
    Examples:
    | workflow              | workflowRequestMessage     |
    | Basic_Workflow_1      | Basic_AeTitle_WF_Request   |
    | Basic_Workflow_1      | Basic_Id_WF_Request        |
    | Basic_Workflow_3      | Basic_AeTitle_WF_Request   |

@WorkflowRequest
Scenario Outline: Publish a valid workflow request which creates multiple workflow instances
    Given I have a clinical workflow <workflow_1>
    And I have a clinical workflow <workflow_2>
    When I publish a Workflow Request Message <workflowRequestMessage> with no artifacts
    Then I can see 2 Workflow Instances are created
    And 2 Task Dispatch events are published
    Examples:
    | workflow_1       | workflow_2       | workflowRequestMessage    |
    | Same_AeTitle_1   | Same_AeTitle_2   | Same_AeTitle              |
    | Basic_Workflow_1 | Basic_Workflow_2 | Basic_Multi_Id_WF_Request |
    | Basic_Workflow_1 | Basic_Workflow_3 | Basic_AeTitle_WF_Request  |

@WorkflowRequest
Scenario Outline: Publish a workflow request which triggers a worflow based on called_aet and calling_aet
    Given I have a clinical workflow <workflow>
    When I publish a Workflow Request Message <workflowRequestMessage> with no artifacts
    Then I can see 1 Workflow Instance is created
    And 1 Task Dispatch event is published
    Examples:
    | workflow                              | workflowRequestMessage            |
    | Workflow_Called_AET                   | Called_AET_AIDE_Calling_AET_TEST  |
    | Workflow_Called_AET_Calling_AET       | Called_AET_AIDE_Calling_AET_PACS1 |
    | Workflow_Called_AET_Multi_Calling_AET | Called_AET_AIDE_Calling_AET_PACS1 |
    | Workflow_Called_AET_Multi_Calling_AET | Called_AET_AIDE_Calling_AET_PACS2 |

@WorkflowRequest
Scenario Outline: Publish a workflow request which doesnt trigger a worflow based calling_aet
    Given I have a clinical workflow <workflow>
    When I publish a Workflow Request Message <workflowRequestMessage> with no artifacts
    Then I can see no Workflow Instances are created
    Examples:
    | workflow                              | workflowRequestMessage           |
    | Workflow_Called_AET_Calling_AET       | Called_AET_AIDE_Calling_AET_TEST |
    | Workflow_Called_AET_Multi_Calling_AET | Called_AET_AIDE_Calling_AET_TEST |

@WorkflowRequest
Scenario: Publish a valid workflow request with mismatched AE title and workflow ID
    Given I have a clinical workflow Basic_Workflow_1
    And I have a clinical workflow Basic_Workflow_3
    When I publish a Workflow Request Message Mismatch_Id_AeTitle_WF_Request with no artifacts
    Then I can see 1 Workflow Instance is created

@WorkflowRequest
Scenario: Publish a valid workflow request triggering a workflow with multiple revisions
    Given I have a clinical workflow Basic_Workflow_Multiple_Revisions_1
    And I have a clinical workflow Basic_Workflow_Multiple_Revisions_2
    When I publish a Workflow Request Message <workflowRequestMessage> with no artifacts
    Then I can see 1 Workflow Instances are created
    Examples:
    | workflowRequestMessage               |
    | AeTitle_Multi_Revision_WF_Request    |
    | WorkflowID_Multi_Revision_WF_Request |

@WorkflowRequest
Scenario: Publish an invalid workflow request which does not create a workflow instance
    Given I have a clinical workflow Basic_Workflow_3
    When I publish a Workflow Request Message <workflowRequestMessage> with no artifacts
    Then I can see no Workflow Instances are created
    Examples:
    | workflowRequestMessage                    |
    | Missing_PayloadID_Invalid_WF_Request      |
    | Missing_Bucket_Invalid_WF_Request         |
    | Missing_CorrelationID_Invalid_WF_Request  |
    | Missing_CallingAETitle_Invalid_WF_Request |
    | Missing_CalledAETitle_Invalid_WF_Request  |
    | No_Matching_AE_Title                      |

@WorkflowRequest
Scenario: Publish an workflow request with ae title that matches old version and does not create a workflow instance
    Given I have a clinical workflow Basic_Workflow_Multiple_Revisions_Different_AE_1
    And I have a clinical workflow Basic_Workflow_Multiple_Revisions_Different_AE_2
    When I publish a Workflow Request Message AE_Title_From_Old_Version with no artifacts
    Then I can see no Workflow Instances are created

@WorkflowRequest
Scenario: Publish a valid workflow request with an existing Workflow Instance with a Task which is not dispatched
    Given I have a clinical workflow Multi_Request_Workflow_Created
    And I have a Workflow Instance Existing_WFI_Created with no artifacts
    When I publish a Workflow Request Message Multi_WF_Created with no artifacts
    Then I can see an additional Workflow Instance is not created
    And A Task Dispatch event is not published

@WorkflowRequest
Scenario: Publish a valid workflow request with an existing Workflow Instance with a Task which is dispatched
    Given I have a clinical workflow Multi_Request_Workflow_Dispatched
    And I have a Workflow Instance Existing_WFI_Dispatched with no artifacts
    When I publish a Workflow Request Message Multi_WF_Dispatched with no artifacts
    Then I can see an additional Workflow Instance is not created
    And A Task Dispatch event is not published

@DeleteWorkflows
Scenario: Delete a workflow with 1 revision and the workflow cannot trigger any new workflow instances
    Given I have a clinical workflow Basic_Workflow_1_Deleted
    And I publish a Workflow Request Message No_Matching_AE_Title with artifacts full_patient_metadata in minio
    Then No workflow instances will be created

@DeleteWorkflows
Scenario: Delete a workflow with 2 revisions and the workflow cannot trigger any new workflow instances
    Given I have a clinical workflow Basic_Workflow_Multiple_Revisions_1_Deleted
    And I have a clinical workflow Basic_Workflow_Multiple_Revisions_2_Deleted
    And I publish a Workflow Request Message AeTitle_Multi_Revision_WF_Request with artifacts full_patient_metadata in minio
    Then No workflow instances will be created
