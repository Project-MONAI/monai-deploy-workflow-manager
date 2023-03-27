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

# Task Manager - Argo Template API

The template endpoint provides the following APIs to post a new template to Argo plugin of 
the Task Manager.


## POST /argo/template

Posts a new template to Argo and returns the resultant template.

### Parameters

body, json string of the wrapped template 

### Responses

Response Content Type: JSON

- the actual Argo resultant template.

| Code | Description                     |
| ---- | ------------------------------- |
| 200  | template posted sucsessfully.   |
| 400  | bad json or template post error |
| 500  | Internal service error.         |

### Example Request

file saved locally as workflowtemplate.json
```json
{
	"Namespace": "argo",
	"serverDryRun": false,
	"submitOptions": {
		"parameters": [
			"name=value"
		]
	},
	"template": {
		"metadata": {
			"name": "fantastic-tiger",
			"namespace": "argo",
			"labels": {
				"example": "true"
			}
		},
		"spec": {
			"workflowMetadata": {
				"labels": {
					"example": "true"
				}
			},
			"entrypoint": "argosay",
			"arguments": {
				"parameters": [
					{
						"name": "message",
						"value": "hello argo"
					}
				]
			},
			"templates": [
				{
					"name": "argosay",
					"inputs": {
						"parameters": [
							{
								"name": "message",
								"value": "{{workflow.parameters.message}}"
							}
						]
					},
					"container": {
						"name": "main",
						"image": "argoproj/argosay:v2",
						"command": [
							"/argosay"
						],
						"args": [
							"echo",
							"{{inputs.parameters.message}}"
						]
					}
				}
			],
			"ttlStrategy": {
				"secondsAfterCompletion": 300
			},
			"podGC": {
				"strategy": "OnPodCompletion"
			}
		}
	}
}
```

```bash
curl -d @workflowtemplate.json 'http://localhost:5000/argo/template'
```

### Example Response

```json
{
	"metadata": {
		"creationTimestamp": "2023-03-23T17:28:34+00:00",
		"generation": 1,
		"labels": {
			"example": "true",
			"workflows.argoproj.io/creator": "system-serviceaccount-argo-argo-argo-workflows-server"
		},
		"managedFields": [
			{
				"apiVersion": "argoproj.io/v1alpha1",
				"fieldsType": "FieldsV1",
				"fieldsV1": {},
				"manager": "argo",
				"operation": "Update",
				"time": "2023-03-23T17:28:34+00:00"
			}
		],
		"name": "fantastic-tiger",
		"namespace": "argo",
		"resourceVersion": "12030505",
		"uid": "e7609791-2d30-4dae-a47e-f1796846068d"
	},
	"spec": {
		"arguments": {
			"parameters": [
				{
					"name": "message",
					"value": "hello argo"
				}
			]
		},
		"entrypoint": "argosay",
		"podGC": {
			"strategy": "OnPodCompletion"
		},
		"templates": [
			{
				"container": {
					"args": [
						"echo",
						"{{inputs.parameters.message}}"
					],
					"command": [
						"/argosay"
					],
					"image": "argoproj/argosay:v2",
					"name": "main",
					"resources": {}
				},
				"inputs": {
					"parameters": [
						{
							"name": "message",
							"value": "{{workflow.parameters.message}}"
						}
					]
				},
				"metadata": {},
				"name": "argosay",
				"outputs": {}
			}
		],
		"ttlStrategy": {
			"secondsAfterCompletion": 300
		},
		"workflowMetadata": {
			"labels": {
				"example": "true"
			}
		}
	}
}
```
