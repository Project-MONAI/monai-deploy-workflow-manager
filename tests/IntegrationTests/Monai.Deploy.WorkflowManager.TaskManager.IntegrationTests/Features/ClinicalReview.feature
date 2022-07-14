Feature: ClinicalReview

Integration tests for the clinical review plugin

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers a clinical review request event with all patient details
	Given I have a bucket in MinIO bucket1
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Full_Patient_Details
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers a clinical review request event with partial patient details
	Given I have a bucket in MinIO bucket1
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Partial_Patient_Details
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers a clinical review request event with no patient details
	Given I have a bucket in MinIO bucket1
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review_No_Patient_Details
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers a clinical review request event with multiple files
	Given I have a bucket in MinIO bucket1
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Multiple_Files
    Then A Clincial Review Request event is published
