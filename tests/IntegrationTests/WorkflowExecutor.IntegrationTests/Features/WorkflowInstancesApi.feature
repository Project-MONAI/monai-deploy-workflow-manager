Feature: WorkflowInstancesApi

API to interact with WorkflowInstances collection

@GetWorkflowInstances
Scenario: Get all workflows instances - multiple
	Given I have an endpoint /workflowinstances
    And I have a Workflow Instance Existing_WFI_Created
    And I have a Workflow Instance Existing_WFI_Dispatched
    And I have a Workflow Instance WFI_Multi_Task_Dispatched
    When I send a GET request
    Then I will get a 200 response
    And I can see expected workflow instances are returned

@GetWorkflowInstances
Scenario: Get all workflows instances - single
	Given I have an endpoint /workflowinstances
    And I have a Workflow Instance Existing_WFI_Created
    When I send a GET request
    Then I will get a 200 response
    And I can see expected workflow instances are returned

@GetWorkflowInstances
Scenario: Get all workflows instances - empty
	Given I have an endpoint /workflowinstances
    When I send a GET request
    Then I will get a 200 response
    And I can see expected workflow instances are returned

@WorkflowInstancePagination
Scenario Outline: Get all workflow instances from API - Test pagination
    Given I have an endpoint /workflowinstances/<pagination_query>
    And I have <amount> Workflow Instances
    When I send a GET request
    Then I will get a 200 response
    And Pagination is working correctly for the <pagination_count> workflow instances
    Examples:
    | pagination_query           | amount | pagination_count |
    | ?pageSize=1                | 15     | 15               |
    | ?pageNumber=10             | 15     | 15               |
    | ?pageNumber=1&pageSize=10  | 15     | 15               |
    | ?pageSize=10&pageNumber=2  | 13     | 13               |
    | ?pageNumber=2&pageSize=7   | 4      | 4                |
    | ?pageNumber=3&pageSize=10  | 7      | 7                |
    | ?pageNumber=1&pageSize=3   | 10     | 10               |
    |                            | 15     | 15               |
    |                            | 3      | 3                |
    | ?pageNumber=3&pageSize=10  | 0      | 0                |
    | ?pageNumber=1              | 0      | 0                |
    |                            | 0      | 0                |
    | ?pageNumber=1&pageSize=100 | 15     | 15               |

@WorkflowInstancePagination
Scenario Outline: Invalid pagination returns 400
    Given I have an endpoint /workflowinstances/<pagination_query>
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
    
@GetWorkflowInstances
Scenario: Get all workflows instances by Id
	Given I have an endpoint /workflowinstances/bff4cfd0-3af3-4e2b-9f3c-de2a6f2b9569
    And I have a Workflow Instance WFI_Static_1
    And I have a Workflow Instance WFI_Static_2
    When I send a GET request
    Then I will get a 200 response
    And I can see expected workflow instance is returned

@GetWorkflowInstances
Scenario: Get all workflows instances by Id. Id Not Found
	Given I have an endpoint /workflowinstances/bff4cfd0-3af3-4e2b-9f3c-de2a6f2b9575
    And I have a Workflow Instance WFI_Static_1
    When I send a GET request
    Then I will get a 404 response

@GetWorkflowInstances
Scenario: Get all workflows instances by Id. Id Bad Request
	Given I have an endpoint /workflowinstances/absfsushs
    And I have a Workflow Instance WFI_Static_1
    When I send a GET request
    Then I will get a 400 response
