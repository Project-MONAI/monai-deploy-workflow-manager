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

# Workflow Instances APIs

The workflow instances endpoint provides the following APIs to get the running instance of a workflow as well as acknowledge failed tasks. 
## GET /workflowinstances

Returns a paginated list of workflow instances.

### Parameters

(Query) pageNumber: int

(Query) pageSize: int

(Query) maxPageSize: int (default of 10)

(Query) status: string

(Query) payloadId: string

(disablePagination) : boolean (default of false)

### Responses

Response Content Type: JSON

| Code | Description                                                                                                                             |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------- |
| 200  | List of instances. | 
| 400  | Payload Id is invalid. |
| 500  | Server error. The response will be a [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) object with server error details. |

### Example Request

```bash
curl --location --request GET 'http://localhost:5000/workflowinstances'
```

### Example Response

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "firstPage": "/workflowinstances?pageNumber=1&pageSize=10",
  "lastPage": "/workflowinstances?pageNumber=1&pageSize=10",
  "totalPages": 1,
  "totalRecords": 1,
  "nextPage": null,
  "previousPage": null,
  "data": [
    {
      "id": "13cd39b5-7914-4e31-ad29-c9ff74955184",
      "ae_title": "MonaiSCU",
      "workflow_name": "pixel-workflow",
      "workflow_id": "8e039cb2-2338-408b-9a4e-09cfc14f80cd",
      "payload_id": "00000000-1000-0000-0000-000000000000",
      "start_time": "2022-10-19T13:14:35.699Z",
      "status": "Failed",
      "bucket_id": "my-bucket",
      "input_metadata": {},
      "tasks": [
        {
          "execution_id": "f57684fc-8b1e-4750-a7eb-a3d2ca05f4f4",
          "workflow_instance_id": "13cd39b5-7914-4e31-ad29-c9ff74955184",
          "task_type": "argo",
          "task_start_time": "2022-10-19T13:14:36.149Z",
          "task_end_time": null,
          "execution_stats": {},
          "task_plugin_arguments": {
            "namespace": "argo",
            "workflow_template_name": "aide-artifact-passing-j5ndx",
            "server_url": "https://localhost:2746",
            "allow_insecure": "true"
          },
          "task_id": "aide-passing",
          "previous_task_id": "",
          "status": "Failed",
          "reason": "ExternalServiceError",
          "input_artifacts": {},
          "output_artifacts": {},
          "output_directory": "00000000-1000-0000-0000-000000000000/workflows/13cd39b5-7914-4e31-ad29-c9ff74955184/f57684fc-8b1e-4750-a7eb-a3d2ca05f4f4",
          "result": {},
          "input_parameters": {},
          "next_timeout": "2022-10-19T14:14:36.149Z",
          "timeout_interval": 60,
          "acknowledged_task_errors": null
        }
      ],
      "acknowledged_workflow_errors": null
    }
  ],
  "succeeded": true,
  "errors": null,
  "message": null
}
```

---

## GET /workflowinstances/{workflowId}

Returns an specific workflow instance the provided id.

### Parameters

id: guid

### Responses

Response Content Type: JSON

| Code | Description                                                                                                                             |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------- |
| 200  | Returns a workflow instance. |
 400  | Provided Id is not a valid Guid.  |
| 404  | Workflow instance not found for given Id. |
| 500  | Server error. The response will be a [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) object with server error details. |

### Example Request

```bash
curl -X 'GET' \
  'http://localhost:5000/workflowinstances/13cd39b5-7914-4e31-ad29-c9ff74955184' \
  -H 'accept: text/plain'
```

### Example Response

```json
{
  "id": "13cd39b5-7914-4e31-ad29-c9ff74955184",
  "ae_title": "MonaiSCU",
  "workflow_name": "pixel-workflow",
  "workflow_id": "8e039cb2-2338-408b-9a4e-09cfc14f80cd",
  "payload_id": "00000000-1000-0000-0000-000000000000",
  "start_time": "2022-10-19T13:14:35.699Z",
  "status": "Failed",
  "bucket_id": "my-bucket",
  "input_metadata": {},
  "tasks": [
    {
      "execution_id": "f57684fc-8b1e-4750-a7eb-a3d2ca05f4f4",
      "workflow_instance_id": "13cd39b5-7914-4e31-ad29-c9ff74955184",
      "task_type": "argo",
      "task_start_time": "2022-10-19T13:14:36.149Z",
      "task_end_time": null,
      "execution_stats": {},
      "task_plugin_arguments": {
        "namespace": "argo",
        "workflow_template_name": "aide-artifact-passing-j5ndx",
        "server_url": "https://localhost:2746",
        "allow_insecure": "true"
      },
      "task_id": "aide-passing",
      "previous_task_id": "",
      "status": "Failed",
      "reason": "ExternalServiceError",
      "input_artifacts": {},
      "output_artifacts": {},
      "output_directory": "00000000-1000-0000-0000-000000000000/workflows/13cd39b5-7914-4e31-ad29-c9ff74955184/f57684fc-8b1e-4750-a7eb-a3d2ca05f4f4",
      "result": {},
      "input_parameters": {},
      "next_timeout": "2022-10-19T14:14:36.149Z",
      "timeout_interval": 60,
      "acknowledged_task_errors": null
    }
  ],
  "acknowledged_workflow_errors": null
}
```

---

## GET /workflowinstances/failed

Returns an specific workflow instance the provided id.

### Parameters

acknowledged: Date (ISO) - Required

### Responses

Response Content Type: JSON

| Code | Description                                                                                                                             |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------- |
| 200  | Returns a list of failed workflows. |
 400  | Date missing.  |
| 404  | Workflow instance not found for given Id. |
| 500  | Server error. The response will be a [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) object with server error details. |

### Example Request

```bash
curl -X 'GET' \
  'http://localhost:5000/workflowinstances/failed?acknowledged=2022-01-19' \
  -H 'accept: text/plain'
```

### Example Response

```json
{
  "id": "13cd39b5-7914-4e31-ad29-c9ff74955184",
  "ae_title": "MonaiSCU",
  "workflow_name": "pixel-workflow",
  "workflow_id": "8e039cb2-2338-408b-9a4e-09cfc14f80cd",
  "payload_id": "00000000-1000-0000-0000-000000000000",
  "start_time": "2022-10-19T13:14:35.699Z",
  "status": "Failed",
  "bucket_id": "my-bucket",
  "input_metadata": {},
  "tasks": [
    {
      "execution_id": "f57684fc-8b1e-4750-a7eb-a3d2ca05f4f4",
      "workflow_instance_id": "13cd39b5-7914-4e31-ad29-c9ff74955184",
      "task_type": "argo",
      "task_start_time": "2022-10-19T13:14:36.149Z",
      "task_end_time": null,
      "execution_stats": {},
      "task_plugin_arguments": {
        "namespace": "argo",
        "workflow_template_name": "aide-artifact-passing-j5ndx",
        "server_url": "https://localhost:2746",
        "allow_insecure": "true"
      },
      "task_id": "aide-passing",
      "previous_task_id": "",
      "status": "Failed",
      "reason": "ExternalServiceError",
      "input_artifacts": {},
      "output_artifacts": {},
      "output_directory": "00000000-1000-0000-0000-000000000000/workflows/13cd39b5-7914-4e31-ad29-c9ff74955184/f57684fc-8b1e-4750-a7eb-a3d2ca05f4f4",
      "result": {},
      "input_parameters": {},
      "next_timeout": "2022-10-19T14:14:36.149Z",
      "timeout_interval": 60,
      "acknowledged_task_errors": null
    }
  ],
  "acknowledged_workflow_errors": null
}
```

---