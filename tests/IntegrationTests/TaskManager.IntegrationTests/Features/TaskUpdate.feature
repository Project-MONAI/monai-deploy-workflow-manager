Feature: TaskUpdate

Integration tests for testing TaskUpdateEvents from TaskManager

@TaskDispatch_TaskUpdate
Scenario: TaskUpdateEvent is published with status Accepted after receiving a valid TaskDispatchEvent
	Given I have a bucket in MinIO bucket1
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Full_Patient_Details
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
    | Task_Dispatch_Invalid_CorrelationId_Missing        |
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
	And A Task Callback event is published Task_Callback_Basic
    And A Task Update event with status Succeeded is published with Task Callback details
