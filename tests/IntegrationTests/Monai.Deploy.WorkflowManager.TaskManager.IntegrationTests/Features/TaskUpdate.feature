Feature: TaskUpdate


@TaskUpdate
Scenario: Task Update event is published by the Task Manager after receiving a clincial review Task Dispatch event
	Given I have a bucket in MinIO bucket1
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Full_Patient_Details
    Then A Task Update event with status Accepted is published

@TaskUpdate
Scenario: Task Update event is published by the Task Manager after receiving a Task Dispatch event
	Given I have a bucket in MinIO bucket1
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Full_Patient_Details
    Then A Task Update event with status Accepted is published
