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
Feature: HealthCheckApi

Health check API endpoints for taskmanager.

@HealthChecks
Scenario: Get taskmanager database health endpoint data from API
	Given I have a taskmanager endpoint /taskmanager/health/database
	When I send a GET request
	Then I will get a 200 response
    And I will get a status message Healthy

@HealthChecks
Scenario: Get taskmanager health endpoint data from API
	Given I have a taskmanager endpoint /taskmanager/health
	When I send a GET request
	Then I will get a 200 response
    And I will get a health check response status message Healthy
