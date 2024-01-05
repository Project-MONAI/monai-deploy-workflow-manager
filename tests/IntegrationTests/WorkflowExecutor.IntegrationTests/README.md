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

# Description

Integration tests cover testing the functionality of the Workflow Executor. They seed DB state in Mongo, seed artifacts in MinIO and simulate the behavior of the Informatics Gateway and Task Executor.

Integration tests have also been written against the WorkflowManager API's using HttpClient.

## Architecture
- WorkflowExecutorStartup.cs starts the WorkflowExecutor services.
- Configuration comes from the appsettings.json from WorkflowManager.
- RabbitPublisher.cs and RabbitConsumer.cs are used to interact with Rabbit.
- MinioClientUtil.cs is used to interact with MinIO.
- MongoClientUtil.cs is used to interact with Mongo.
- HTML Execution Reports are generated using [Specflow+LivingDocs](https://docs.specflow.org/projects/specflow-livingdoc/en/latest/).

## Writing Tests

When writing tests you must:
- Write the tests using [Specflow](https://docs.specflow.org/projects/getting-started/en/latest/index.html) using a GIVEN-WHEN-THEN syntax in an existing or new feature file.
- Create the relevant DB Test Data (WorkflowRevision, WorkflowInstance) and reference it by name in the scenario using existing steps. DB test data can be any state i.e WorkflowRevision with a task dispatched, task failed etc
- Create the relevant Rabbit Event Test Data (WorkflowRequest, TaskUpdateEvent) and reference it by name in the scenario using existing steps.

## Running Tests

### Required Tools
- Docker
- .net 6 SDK

```bash
cd tests/IntegrationTests/WorkflowManager.IntegrationTests/
```

```bash
docker-compose up
```

Tests can be executed from the Visual Studio Test Explorer or as you would run any unit tests.

## Generating HTML Reports
```bash
dotnet tool install --global SpecFlow.Plus.LivingDoc.CLI
```

Run the integration tests to generate a TestExecution.json

```bash
livingdoc test-assembly {$PROJECT_ROOT}\monai-deploy-workflow-manager\tests\IntegrationTests\TaskManager.IntegrationTests\bin\Debug\net8.0\Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.dll -t {$PROJECT_ROOT}\monai-deploy-workflow-manager\tests\IntegrationTests\TaskManager.IntegrationTests\bin\Debug\net6.0\TestExecution.json
```
