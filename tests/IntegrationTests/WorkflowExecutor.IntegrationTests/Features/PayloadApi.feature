Feature: PayloadApi

API to interact with Payloads collection

@GetPayloads
Scenario: Get all payloads from API - Single payload
    Given I have an endpoint /payload
    And I have a payload saved in mongo Payload_Full_Patient
    When I send a GET request
    Then I will get a 200 response
    And I can see expected Payloads are returned

@GetPayloads
Scenario: Get all payloads from API - multiple payloads
    Given I have an endpoint /payload
    And I have a payload saved in mongo Payload_Full_Patient
    And I have a payload saved in mongo Payload_Partial_Patient
    And I have a payload saved in mongo Payload_Null_Patient
    When I send a GET request
    Then I will get a 200 response
    And I can see expected Payloads are returned

@GetPayloads
Scenario: Get all payloads from API - no payloads
    Given I have an endpoint /payload
    When I send a GET request
    Then I will get a 200 response
    And I can see no Payloads are returned

@GetPayloadById
Scenario Outline: Get payload by Id returns 200
    Given I have an endpoint /payload/c5c3636b-81dd-44a9-8c4b-71adec7d47b2
    And I have a payload saved in mongo Payload_Full_Patient
    And I have a payload saved in mongo Payload_Partial_Patient
    When I send a GET request
    Then I will get a 200 response
    And I can see expected Payload is returned

@GetPayloadById
Scenario Outline: Get payload by Id returns 404
    Given I have an endpoint /payload/c5c3636b-81dd-44a9-8c4b-71adec7d47c3
    And I have a payload saved in mongo Payload_Full_Patient
    And I have a payload saved in mongo Payload_Partial_Patient
    When I send a GET request
    Then I will get a 404 response

@GetPayloadById
Scenario Outline: Get payload by Id returns 400
    Given I have an endpoint /payload/hagaaaa
    And I have a payload saved in mongo Payload_Full_Patient
    When I send a GET request
    Then I will get a 400 response
