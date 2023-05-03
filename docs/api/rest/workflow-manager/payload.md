<!--
  ~ Copyright 2022 MONAI Consortium
  ~
  ~ Licensed under the Apache License, Version 2.0 (the "License");
  ~ you may not use this file except in compliance with the License.
  ~ You may obtain a copy of the License at
  ~
  ~ http://www.apache.org/licenses/LICENSE-2.0
  ~
  ~ Unless required by applicable law or agreed to in writing, software
  ~ distributed under the License is distributed on an "AS IS" BASIS,
  ~ WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  ~ See the License for the specific language governing permissions and
  ~ limitations under the License.
-->

# Workflow Manager - Payload APIs

## GET /payloads

Returns a paginated list of Payloads with additional computed property of payloadStatus.

- If any of the workflow Instances status' attached to that payload are “Created” then the payloadStatus would be “In Progress”

- If all workflow Instances status' attached to that payload are not “Created” then the payloadStatus would be “Complete”

- If there are no workflow Instances attached to that payload then the payloadStatus would be “Complete”

### Parameters

(Query) pageNumber: int

(Query) pageSize: int

(Query) maxPageSize: int (default of 10)

(Query) patientId: string

(Query) patientName: string

### Responses

Response Content Type: JSON

| Code | Description                                                                                                                             |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------- |
| 200  | List of payloads. | 
| 500  | Server error. The response will be a [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) object with server error details. |

### Example Request

```bash
curl --location --request GET 'http://localhost:5000/payloads'
```

### Example Response

```json
{
  "PageNumber": 1,
  "PageSize": 10,
  "FirstPage": "/payload?pageNumber=1&pageSize=10",
  "LastPage": "/payload?pageNumber=1&pageSize=10",
  "TotalPages": 1,
  "TotalRecords": 3,
  "NextPage": null,
  "PreviousPage": null,
  "Data":
    [
      {
        "Version": "1.0.0",
        "id": "3042cac6-b8b8-4f65-a2b2-8ec340652c9b",
        "payload_id": "c5c3636b-81dd-44a9-8c4b-71adec7d47b2",
        "workflows": ["1e7b49f2-a3a2-4ded-b489-86ad9b2da9c8"],
        "workflow_instance_ids": [],
        "file_count": 50,
        "correlation_id": "68ccea88-1f40-4eb3-ad23-7f444ac12910",
        "bucket": "bucket_1",
        "calling_aetitle": "Basic_AE",
        "called_aetitle": "MIG",
        "timestamp": "2023-05-03T12:47:59.046Z",
        "files": [],
        "patient_details":
          {
            "patient_id": "732ca351-6267-41e9-a6e8-21b3a74abe7c",
            "patient_name": "Steve Jobs",
            "patient_sex": "male",
            "patient_dob": "1996-02-05T00:00:00Z",
            "patient_age": null,
            "patient_hospital_id": null
          },
        "payload_status": "Completed",
        "payload_deleted": 1
      },
      {
        "Version": "1.0.0",
        "id": "2435a8d7-84f4-407d-9d0e-941bb87b0190",
        "payload_id": "86c0f117-4021-412e-b163-0dc621df672a",
        "workflows": ["3d517d26-118c-4241-beb8-6b51d462c746"],
        "workflow_instance_ids": [],
        "file_count": 3,
        "correlation_id": "1ef12067-0fda-45c6-87b4-bcc4b245e8d9",
        "bucket": "bucket_1",
        "calling_aetitle": "Basic_AE",
        "called_aetitle": "MIG",
        "timestamp": "2023-05-03T12:47:59.046Z",
        "files": [],
        "patient_details":
          {
            "patient_id": "dae4a6d1-573d-4a3f-978f-ed056f628de6",
            "patient_name": "Jane Doe",
            "patient_sex": "female",
            "patient_dob": null,
            "patient_age": null,
            "patient_hospital_id": null
          },
        "payload_status": "Completed",
        "payload_deleted": 1
      },
      {
        "Version": "1.0.0",
        "id": "39d599bc-6635-4f94-9b55-b72a5b11c849",
        "payload_id": "30a8e0c6-e6c4-458f-aa4d-b224b493d3c0",
        "workflows": ["db52fafe-1035-436a-b4ff-50ab850f5f68"],
        "workflow_instance_ids": [],
        "file_count": 3,
        "correlation_id": "40544d26-b4bb-4f67-b4ae-68ff3a237cf2",
        "bucket": "bucket_2",
        "calling_aetitle": "Basic_AE",
        "called_aetitle": "MIG",
        "timestamp": "2023-05-03T12:47:59.046Z",
        "files": [],
        "patient_details":
          {
            "patient_id": null,
            "patient_name": null,
            "patient_sex": null,
            "patient_dob": null,
            "patient_age": null,
            "patient_hospital_id": null
          },
        "payload_status": "Completed",
        "payload_deleted": 1
      }
    ],
  "Succeeded": true,
  "Errors": null,
  "Message": null
}
```

---

## GET /payloads/{id}

Returns specific payload for given id.

### Parameters
(Route) id: string - UUID

### Responses

Response Content Type: JSON

| Code | Description                                                                                                                             |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------- |
| 200  | List of payloads. | 
| 400  | [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) Failed to validate id. |
| 404  | [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) Failed to find payload with payload id. |
| 500  | Server error. The response will be a [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) object with server error details. |


### Example Request

```bash
curl --location --request GET 'http://localhost:5000/payloads/3042cac6-b8b8-4f65-a2b2-8ec340652c9b'
```

### Example Response
```json
{
  "Version": "1.0.0",
  "id": "3042cac6-b8b8-4f65-a2b2-8ec340652c9b",
  "payload_id": "c5c3636b-81dd-44a9-8c4b-71adec7d47b2",
  "workflows": ["1e7b49f2-a3a2-4ded-b489-86ad9b2da9c8"],
  "workflow_instance_ids": [],
  "file_count": 50,
  "correlation_id": "68ccea88-1f40-4eb3-ad23-7f444ac12910",
  "bucket": "bucket_1",
  "calling_aetitle": "Basic_AE",
  "called_aetitle": "MIG",
  "timestamp": "2023-05-03T12:47:59.046Z",
  "files": [],
  "patient_details":
    {
      "patient_id": "732ca351-6267-41e9-a6e8-21b3a74abe7c",
      "patient_name": "Steve Jobs",
      "patient_sex": "male",
      "patient_dob": "1996-02-05T00:00:00Z",
      "patient_age": null,
      "patient_hospital_id": null
    },
}
```

---

## DELETE /payloads/{id}

TODO

---