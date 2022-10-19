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

# Workflow Manager - Task APIs

**TEMPLATE**

## GET/POST/PUT/DELETE /tasks/

*description*

### Parameters

*parameters*

### Responses

Response Content Type: JSON

*response type*

| Code | Description                                                                                                                             |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------- |
| 200  | Service is healthy.                                                                                                                     |
| 503  | Service is unhealthy.                                                                                                                   |
| 500  | Server error. The response will be a [Problem details](https://datatracker.ietf.org/doc/html/rfc7807) object with server error details. |

### Example Request

```bash
curl --location --request GET 'http://localhost:5000/workflows'
```

### Example Response

```json
*sample output*
```

