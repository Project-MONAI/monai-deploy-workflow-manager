# Copyright 2023 MONAI Consortium
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
Feature: EmialNotification

Integration tests for the email plugin

@EmailPlugin
Scenario: Email task dispatch event triggers an email request event with all relevant args
    Given I have an input DICOM file saved in MinIO for TaskDispatch Task_Dispatch_Email_All
	When A Task Dispatch event is published Task_Dispatch_Email_All
    Then An email request event is published

@EmailPlugin
Scenario: Email task dispatch event triggers an email request event with only emails arg
    Given I have an input DICOM file saved in MinIO for TaskDispatch Task_Dispatch_Email_Emails
	When A Task Dispatch event is published Task_Dispatch_Email_Emails
    Then An email request event is published

@EmailPlugin
Scenario: Email task dispatch event triggers an email request event with only roles arg
    Given I have an input DICOM file saved in MinIO for TaskDispatch Task_Dispatch_Email_Roles
	When A Task Dispatch event is published Task_Dispatch_Email_Roles
    Then An email request event is published
