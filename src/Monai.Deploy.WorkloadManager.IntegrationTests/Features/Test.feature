﻿Feature: Test

A short summary of the feature

@tag1
Scenario: I can test Rabbit connection
	Given I have a Rabbit connection
	When I publish an event WorkflowEvent_1
	Then I can see the event WorkflowEvent_1

@ignore
Scenario: I can test Mongo connection
	Given I have a Mongo connection
	When I save a DAG
	Then I can retrieve the DAG
