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

# Workflow Manager - Health APIs

The _health_ endpoint provides the following APIs to get the status of the internal services & dependent services of 
the Workflow Manager.


## GET /health/

Returns the MONAI Deploy Workflow Manager service readiness and liveness.

### Parameters

N/A

### Responses

Response Content Type: JSON

- `Healthy`: All services are running.
- `Unhealthy`: One or more services have stopped or crashed.

| Code | Description                     |
| ---- | ------------------------------- |
| 200  | Service is healthy or degraded. |
| 503  | Service is unhealthy.           |

### Example Request

```bash
curl --location --request GET 'http://localhost:5000/health'
```

### Example Response

```json
{
    "status": "Healthy",
    "checks": [
        {
            "check": "minio",
            "result": "Healthy"
        },
        {
            "check": "minio-admin",
            "result": "Healthy"
        },
        {
            "check": "Rabbit MQ Publisher",
            "result": "Healthy"
        },
        {
            "check": "Rabbit MQ Subscriber",
            "result": "Healthy"
        },
        {
            "check": "Workflow Manager Services",
            "result": "Healthy"
        },
        {
            "check": "mongodb",
            "result": "Healthy"
        }
    ]
}
```
