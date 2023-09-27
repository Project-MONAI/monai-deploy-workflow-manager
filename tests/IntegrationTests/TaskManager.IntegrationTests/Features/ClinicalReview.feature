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

@IntegrationTests
Feature: ClinicalReview

Integration tests for the clinical review plugin

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers a clinical review request event with all patient details
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Full_Patient_Details
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers a clinical review request event with partial patient details
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Partial_Patient_Details
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers a clinical review request event with no patient details
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review_No_Patient_Details
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers a clinical review request event with multiple files
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Multi_File
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers clincial review event with single reviewer role
    When A Task Dispatch event is published Task_Dispatch_Clinical_Reviewer_Role_Single_Role
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers clincial review event with default reviewer role
    When A Task Dispatch event is published Task_Dispatch_Clinical_Reviewer_Role_Mutiple_Roles
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers clincial review event with mutiple reviewer roles
    When A Task Dispatch event is published Task_Dispatch_Clinical_Reviewer_Role_Default_Role
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers clincial review event with application name
    When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Application_Name
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers clincial review event with application version
    When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Application_Version
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers clincial review event with QA mode
    When A Task Dispatch event is published Task_Dispatch_Clinical_Review_QA_Mode
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario: Clincial review task dispatch event triggers clincial review event with reviewed execution Id
    When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Reviewed_Execution_Id
    Then A Clincial Review Request event is published

@ClinicalReviewPlugin
Scenario Outline: Clincial review task dispatch event triggers clincial review event with notifications
    When A Task Dispatch event is published <testData>
    Then A Clincial Review Request event is published
    Examples:
    | testData                                          |
    | Task_Dispatch_Clinical_Review_Notifications_True  |
    | Task_Dispatch_Clinical_Review_Notifications_False |
