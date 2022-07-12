Feature: ClinicalReview

Integration tests for the clinical review plugin

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers a clinical review request event to be published
	Given I have a bucket in MinIO bucket1
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review
    Then A Clincial Review Request event is published
