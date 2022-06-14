Feature: WorkflowRequest

Publishing a workflow request is consumed by the Workflow Manager.

@WorkflowRequest
Scenario Outline: Publish a valid workflow request which creates a single workflow instance
    Given I have a clinical workflow <workflow>
    When I publish a Workflow Request Message <workflowRequestMessage>
    Then I can see 1 Workflow Instance is created
    And 1 Task Dispatch event is published
    Examples:
    | workflow              | workflowRequestMessage     |
    #| Basic_Workflow_1      | Basic_AeTitle_WF_Request   |
    | Basic_Workflow_1      | Basic_Id_WF_Request        |

@WorkflowRequest
Scenario Outline: Publish a valid workflow request which creates multiple workflow instances
    Given I have a clinical workflow <workflow_1>
    And I have a clinical workflow <workflow_2>
    When I publish a Workflow Request Message <workflowRequestMessage>
    Then I can see 2 Workflow Instances are created
    And 2 Task Dispatch events are published
    Examples:
    | workflow_1       | workflow_2         | workflowRequestMessage    |
    | Same_AeTitle_1   | Same_AeTitle_2     | Same_AeTitle              |
    | Basic_Workflow_1 | Basic_Workflow_2   | Basic_Multi_Id_WF_Request |

@WorkflowRequest
Scenario: Publish a valid workflow request with mismatched AE title and workflow ID
    Given I have a clinical workflow Basic_Workflow_1
    And I have a clinical workflow Basic_Workflow_3
    When I publish a Workflow Request Message Mismatch_Id_AeTitle_WF_Request
    Then I can see 1 Workflow Instance is created

@WorkflowRequest
Scenario: Publish a valid workflow request triggering a workflow with multiple revisions
    Given I have a clinical workflow Basic_Workflow_Multiple_Revisions_1
    And I have a clinical workflow Basic_Workflow_Multiple_Revisions_2
    When I publish a Workflow Request Message <workflowRequestMessage> 
    Then I can see 1 Workflow Instances are created
    Examples:
    | workflowRequestMessage               |
    | AeTitle_Multi_Revision_WF_Request    |
    | WorkflowID_Multi_Revision_WF_Request |

@WorkflowRequest
Scenario: Publish an invalid workflow request which does not create a workflow instance
    Given I have a clinical workflow Basic_Workflow_3
    When I publish a Workflow Request Message <workflowRequestMessage>
    Then I can see 0 Workflow Instances are created
    Examples:
    | workflowRequestMessage                    |
    | Missing_PayloadID_Invalid_WF_Request      |
    | Missing_WorkflowID_Invalid_WF_Request     |
    | Missing_Bucket_Invalid_WF_Request         |
    | Missing_CorrelationID_Invalid_WF_Request  |
    | Missing_CallingAETitle_Invalid_WF_Request |
    | Missing_CalledAETitle_Invalid_WF_Request  |

@WorkflowRequest
Scenario: Publish a valid workflow request with an exiting Workflow Instance with a Task which is not dispatched
    Given I have a clinical workflow Multi_Request_Workflow_Created
    And I have a Workflow Instance Existing_WFI_Created
    When I publish a Workflow Request Message Multi_WF_Created
    Then I can see an additional Workflow Instance is not created
    And 1 Task Dispatch event is published

@WorkflowRequest
Scenario: Publish a valid workflow request with an exiting Workflow Instance with a Task which is dispatched
    Given I have a clinical workflow Multi_Request_Workflow_Dispatched
    And I have a Workflow Instance Existing_WFI_Dispatched
    When I publish a Workflow Request Message Multi_WF_Dispatched
    Then I can see an additional Workflow Instance is not created
    And A Task Dispatch event is not published
