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
	When A Task Dispatch event is published Task_Dispatch_Clinical_Review_Multi_File
    Then A Clincial Review Request event is published
