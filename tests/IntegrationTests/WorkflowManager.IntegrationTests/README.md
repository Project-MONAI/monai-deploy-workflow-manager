# Description

Integration tests cover testing the functionality of the Workflow Executor. They seed DB state in Mongo, seed artifacts in MinIO and simulate the behavior of the Informatics Gateway and Task Executor.

Integration tests have also been written against the WorkflowManager API's using HttpClient.

## Architecture
- WorkflowExecutorStartup.cs starts the WorkflowExecutor services.
- Configuration comes from the appsettings.json from WorkflowManager.
- RabbitPublisher.cs and RabbitConsumer.cs are used to interact with Rabbit.
- MinioClientUtil.cs is used to interact with MinIO.
- MongoClientUtil.cs is used to interact with Mongo.

## Writing Tests

When writing tests you must:
- Write the tests using [Specflow](https://docs.specflow.org/projects/getting-started/en/latest/index.html) using a GIVEN-WHEN-THEN syntax in an existing or new feature file.
- Create the relevant DB Test Data (WorkflowRevision, WorkflowInstance) and reference it by name in the scenario using existing steps. DB test data can be any state i.e WorkflowRevision with a task dispatched, task failed etc
- Create the relevant Rabbit Event Test Data (WorkflowRequest, TaskUpdate) and reference it by name in the scenario using existing steps.

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

