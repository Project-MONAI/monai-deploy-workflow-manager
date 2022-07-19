# Description

Integration tests cover testing the functionality of the Task manager. They simulate the behavior of the Workflow Executor to trigger task plugins and check output messages.

## Architecture
- TaskManagerStartup.cs starts the TaskManager services.
- Configuration comes from the appsettings.Test.json from WorkflowManager.
- RabbitPublisher.cs and RabbitConsumer.cs are used to interact with Rabbit.
- HTML Execution Reports are generated using [Specflow+LivingDocs](https://docs.specflow.org/projects/specflow-livingdoc/en/latest/).

## Writing Tests

When writing tests you must:
- Write the tests using [Specflow](https://docs.specflow.org/projects/getting-started/en/latest/index.html) using a GIVEN-WHEN-THEN syntax in an existing or new feature file.
- Create the relevant Rabbit Event Test Data (TaskDispatchEvent, TaskCallbackEvent) and reference it by name in the scenario using existing steps.

## Running Tests

### Required Tools
- Docker
- .net 6 SDK

```bash
cd tests/IntegrationTests/TaskManager.IntegrationTests/
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
livingdoc test-assembly {$PROJECT_ROOT}\monai-deploy-workflow-manager\tests\IntegrationTests\TaskManager.IntegrationTests\bin\Debug\net6.0\Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.dll -t {$PROJECT_ROOT}\monai-deploy-workflow-manager\tests\IntegrationTests\TaskManager.IntegrationTests\bin\Debug\net6.0\TestExecution.json
```