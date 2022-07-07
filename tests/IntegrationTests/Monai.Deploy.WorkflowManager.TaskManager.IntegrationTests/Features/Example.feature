Feature: Example

A short summary of the feature

@tag1
Scenario: Publish task dispatch message
	When A Task Dispatch event is published Task_Dispatch
	Then I can see the event is consumed
