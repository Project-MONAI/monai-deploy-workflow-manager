Feature: PayloadCollection

Tests around the payload collection

@PatientDetails
Scenario: Seed MinIO with patient metadata, and payload collection has details
    Given I have a clinical workflow Basic_Workflow_1
    And I have a bucket in MinIO bucket1
    And I have a payload <metadata> in the bucket bucket1 with payload id <payloadId> 
    When I publish a Workflow Request Message <workflowRequestMessage>
    Then A payload collection is created with patient details <patientTestData>
    Then I can see <workflowInstance> Workflow Instance is created
    Examples:
    | workflowRequestMessage                     | metadata                 | patientTestData | payloadId                            | workflowInstance |
    | Basic_AeTitle_Payload_Collection_Request_1 | full_patient_metadata    | Full_Patient    | 23b96697-0174-465c-b9cb-368b20a4591d | 1                |
    | Basic_Non_Existant_Request                 | partial_patient_metadata | Partial_Patient | c9c1e0f1-5994-4882-b3d4-9e1009729377 | 0                |

@PatientDetails
Scenario: Seed MinIO with blank patient data, and payload collection has null details
    Given I have a clinical workflow Basic_Workflow_1
    And I have a bucket in MinIO bucket1
    And I have a payload null_patient_metadata in the bucket bucket1 with payload id 64a2b260-0379-4614-9f05-ff1279cf9e83 
    When I publish a Workflow Request Message Basic_AeTitle_Payload_Collection_Request_2
    Then A payload collection is created with patient details Null_Patient
    Then I can see 1 Workflow Instance is created

@PatientDetails
Scenario: Seed MinIO with no patient data, and payload collection has null details
    Given I have a clinical workflow Basic_Workflow_1
    And I have a bucket in MinIO bucket1
    And I have a payload no_patient_metadata in the bucket bucket1 with payload id b91f5559-8ab2-455a-806d-961244ea22af 
    When I publish a Workflow Request Message Basic_AeTitle_Payload_Collection_Request_3
    Then A payload collection is created with patient details Null_Patient
    Then I can see 1 Workflow Instance is created
