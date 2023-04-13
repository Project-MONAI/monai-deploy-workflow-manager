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

# Workflow APIs

The _workflow_ endpoint provides the following APIs to get, create, update and delete clinical workflows within the Workflow Manager.


## GET /workflows

Returns a paginated list of workflows wrapped in a workflow revision object.

### Parameters

(Query) pageNumber: int

(Query) pageSize: int

(Query) maxPageSize: int (default of 10)


### Responses

Response Content Type: JSON

| Code | Description                                                                                                                             |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------- |
| 200  | List of workflows returned.                                                                                                                                                                       |
| 500  | Server error. The response will be a [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) object with server error details. |

### Example Request

```bash
curl --location --request GET 'http://localhost:5000/workflows'
```

### Example Response

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "firstPage": "/workflows?pageNumber=1&pageSize=10",
  "lastPage": "/workflows?pageNumber=1&pageSize=10",
  "totalPages": 1,
  "totalRecords": 1,
  "nextPage": null,
  "previousPage": null,
  "data": [
    {
      "id": "4206f468-4a05-4f34-be71-521591e3c093",
      "workflow_id": "8e039cb2-2338-408b-9a4e-09cfc14f80cd",
      "revision": 1,
      "workflow": {
        "name": "pixel-workflow",
        "version": "1.0.0",
        "description": "Attempt at making a workflow",
        "informatics_gateway": {
          "ae_title": "MonaiSCU",
          "data_origins": [
            "MY_SCANNER"
          ],
          "export_destinations": [
            "PROD_PACS"
          ]
        },
        "tasks": [
          {
            "id": "aide-passing",
            "description": "trigger simple argo workflow",
            "type": "argo",
            "args": {
              "namespace": "argo",
              "workflow_template_name": "aide-artifact-passing-j5ndx",
              "server_url": "https://localhost:2746",
              "allow_insecure": "true"
            },
            "ref": "",
            "task_destinations": [],
            "export_destinations": [],
            "artifacts": {
              "input": [
                {
                  "name": "input-dicom",
                  "value": "{{ context.input.dicom }}",
                  "mandatory": true
                }
              ],
              "output": [
                {
                  "name": "report-pdf",
                  "value": "",
                  "mandatory": true
                }
              ]
            },
            "input_parameters": null,
            "timeout_minutes": -1
          }
        ]
      }
    }
  ],
  "succeeded": true,
  "errors": null,
  "message": null
}
```

---

## GET /workflows/{workflowId}

Returns an specific workflow with the provided WorkflowId. The workflow is wrapped in a WorkflowRevision object, containing the WorkflowId and Revision number.

### Parameters

id: guid

### Responses

Response Content Type: JSON

| Code | Description                                                                                                                             |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------- |
| 200  | Returns a workflow returned. |
 400  | Provided Id is not a valid Guid.  |
| 404  | Workflow not found for given Id. |
| 500  | Server error. The response will be a [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) object with server error details. |

### Example Request

```bash
curl --location --request GET 'http://localhost:5000/workflows/8e039cb2-2338-408b-9a4e-09cfc14f80cd'
```

### Example Response

```json
{
  "id": "4206f468-4a05-4f34-be71-521591e3c093",
  "workflow_id": "8e039cb2-2338-408b-9a4e-09cfc14f80cd",
  "revision": 1,
  "workflow": {
    "name": "pixel-workflow",
    "version": "1.0.0",
    "description": "Attempt at making a workflow",
    "informatics_gateway": {
      "ae_title": "MonaiSCU",
      "data_origins": [
        "MY_SCANNER"
      ],
      "export_destinations": [
        "PROD_PACS"
      ]
    },
    "tasks": [
      {
        "id": "aide-passing",
        "description": "trigger simple argo workflow",
        "type": "argo",
        "args": {
          "namespace": "argo",
          "workflow_template_name": "aide-artifact-passing-j5ndx",
          "server_url": "https://localhost:2746",
          "allow_insecure": "true"
        },
        "ref": "",
        "task_destinations": [],
        "export_destinations": [],
        "artifacts": {
          "input": [
            {
              "name": "input-dicom",
              "value": "{{ context.input.dicom }}",
              "mandatory": true
            }
          ],
          "output": [
            {
              "name": "report-pdf",
              "value": "",
              "mandatory": true
            }
          ]
        },
        "input_parameters": null,
        "timeout_minutes": -1
      }
    ]
  }
}
```

---

## POST /workflows

Creates a new workflow.

### Parameters
None.
### Responses

Response Content Type: JSON

| Code | Description                                                                                                                             |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------- |
| 201  | Workflow created and new Workflow Id returned. |
 400  | Request body is invalid, a list of errors will be returned.  |
| 500  | Server error. The response will be a [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) object with server error details. |

### Example Request

```bash
curl -X 'POST' \
  'http://localhost:5000/workflows' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json-patch+json' \
  -d '{
	"name": "pixel-workflow",
	"version": "1.0.0",
	"description": "Attempt at making a workflow",
	"informatics_gateway": {
		"ae_title": "MonaiSCU",
		"data_origins": [
			"MY_SCANNER"
		],
		"export_destinations": [
			"PROD_PACS"
		]
	},
	"tasks": [
		{
			"id": "aide-passing",
			"description": "trigger simple argo workflow",
			"type": "argo",
			"args": {
				"namespace":"argo",
				"workflow_template_name": "aide-artifact-passing-j5ndx",
				"server_url": "https://localhost:2746",
				"allow_insecure": true
			},
			"artifacts": {
				"input": [
					{
						"name": "input-dicom",
						"value": "{{ context.input.dicom }}"
					}
				],
				"output": [
					{
						"name": "report-pdf",
						"Mandatory": true
					}
				]
			}
		}
	]
}'
```

JSON Request Body:

```json
{
	"name": "pixel-workflow",
	"version": "1.0.0",
	"description": "Attempt at making a workflow",
	"informatics_gateway": {
		"ae_title": "MonaiSCU",
		"data_origins": [
			"MY_SCANNER"
		],
		"export_destinations": [
			"PROD_PACS"
		]
	},
	"tasks": [
		{
			"id": "aide-passing",
			"description": "trigger simple argo workflow",
			"type": "argo",
			"args": {
				"namespace":"argo",
				"workflow_template_name": "aide-artifact-passing-j5ndx",
				"server_url": "https://localhost:2746",
				"allow_insecure": true
			},
			"artifacts": {
				"input": [
					{
						"name": "input-dicom",
						"value": "{{ context.input.dicom }}"
					}
				],
				"output": [
					{
						"name": "report-pdf",
						"Mandatory": true
					}
				]
			}
		}
	]
}
```

### Example Response

```json
{
  "workflow_id": "74041027-aeae-4c4c-b950-8fe653b659cb"
}
```

---

## PUT /workflows/{WorkflowId}

Updates a workflow and creates a new workflow revision.

### Parameters

id: guid

### Responses

Response Content Type: JSON

| Code | Description                                                                                                                             |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------- |
| 201  | Workflow updated and new revision created. The Workflow Id is returned. |
 400  | Request body or Id is invalid, a list of errors will be returned.  |
| 500  | Server error. The response will be a [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) object with server error details. |

### Example Request

```bash
curl -X 'PUT' \
  'http://localhost:5000/workflows/74041027-aeae-4c4c-b950-8fe653b659cb' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json-patch+json' \
  -d '{
    "original_workflow_name": "pixel-workflow"
    "workflow": {
    "name": "pixel-workflow",
    "version": "1.0.0",
    "description": "Attempt at making a workflow",
    "informatics_gateway": {
      "ae_title": "MonaiSCU",
      "data_origins": [
        "MY_SCANNER"
      ],
      "export_destinations": [
        "PROD_PACS"
      ]
    },
    "tasks": [
      {
        "id": "aide-passing",
        "description": "trigger simple argo workflow",
        "type": "argo",
        "args": {
          "namespace":"argo",
          "workflow_template_name": "aide-artifact-passing-j5ndx",
          "server_url": "https://localhost:2746",
          "allow_insecure": true
        },
        "artifacts": {
          "input": [
            {
              "name": "input-dicom",
              "value": "{{ context.input.dicom }}"
            }
          ],
          "output": [
            {
              "name": "report-pdf",
              "Mandatory": true
            }
          ]
        }
      }
    ]
  }
}'
```

JSON Request Body:

```json
{
  "original_workflow_name": "pixel-workflow"
  "workflow": {
    "name": "pixel-workflow",
    "version": "1.0.0",
    "description": "Attempt at making a workflow",
    "informatics_gateway": {
      "ae_title": "MonaiSCU",
      "data_origins": [
        "MY_SCANNER"
      ],
      "export_destinations": [
        "PROD_PACS"
      ]
    },
    "tasks": [
      {
        "id": "aide-passing",
        "description": "trigger simple argo workflow",
        "type": "argo",
        "args": {
          "namespace":"argo",
          "workflow_template_name": "aide-artifact-passing-j5ndx",
          "server_url": "https://localhost:2746",
          "allow_insecure": true
        },
        "artifacts": {
          "input": [
            {
              "name": "input-dicom",
              "value": "{{ context.input.dicom }}"
            }
          ],
          "output": [
            {
              "name": "report-pdf",
              "Mandatory": true
            }
          ]
        }
      }
    ]
  }
}
```

### Example Response

```json
{
  "workflow_id": "74041027-aeae-4c4c-b950-8fe653b659cb"
}
```

---

## DELETE /workflows/{WorkflowId}

Deletes a workflow within the Workflow Manager.

### Parameters

id: guid

### Responses

Response Content Type: JSON

| Code | Description                                                                                                                             |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------- |
| 200  | Workflow deleted. |
 400  | Id is invalid.  |
  404  | Workflow does not exist.  |
| 500  | Server error. The response will be a [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) object with server error details. |

### Example Request

```bash
curl -X 'DELETE' \
  'http://localhost:5000/workflows/74041027-aeae-4c4c-b950-8fe653b659cb' \
  -H 'accept: text/plain'
```

### Example Response

```json
{
  "id": "c32791f0-c6c1-4e90-b923-e32012431dfa",
  "workflow_id": "74041027-aeae-4c4c-b950-8fe653b659cb",
  "revision": 1,
  "workflow": {
    "name": "pixel-workflow",
    "version": "1.0.0",
    "description": "Attempt at making a workflow",
    "informatics_gateway": {
      "ae_title": "MonaiSCU",
      "data_origins": [
        "MY_SCANNER"
      ],
      "export_destinations": [
        "PROD_PACS"
      ]
    },
    "tasks": [
      {
        "id": "aide-passing",
        "description": "trigger simple argo workflow",
        "type": "argo",
        "args": {
          "namespace": "argo",
          "workflow_template_name": "aide-artifact-passing-j5ndx",
          "server_url": "https://localhost:2746",
          "allow_insecure": "true"
        },
        "ref": "",
        "task_destinations": [],
        "export_destinations": [],
        "artifacts": {
          "input": [
            {
              "name": "input-dicom",
              "value": "{{ context.input.dicom }}",
              "mandatory": true
            }
          ],
          "output": [
            {
              "name": "report-pdf",
              "value": "",
              "mandatory": true
            }
          ]
        },
        "input_parameters": null,
        "timeout_minutes": -1
      }
    ]
  }
}
```

---
