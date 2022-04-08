Feature: Test

Scenario: Basic mongo and rabbit test
    Given I have a DAG in Mongo Dag_Mongo_Connection
    When I publish an Export Message Request ExportMessageRequest_1
    Then I can see the event ExportMessageRequest_1
    And I can retrieve the DAG Dag_Mongo_Connection

