# Clinical Workflow Specification Language

# Note: this document is not currently up-to-date. It will be updated before the first release of the workflow manager.
## Overview

The MONAI Workflow Manager is responsible for executing pre-registered clinical workflows. This document describes how to create a workflow file and what the language allows.

- [Clinical Workflow Specification Language](#clinical-workflow-specification-language)
- [Note: this document is not currently up-to-date. It will be updated before the first release of the workflow manager.](#note-this-document-is-not-currently-up-to-date-it-will-be-updated-before-the-first-release-of-the-workflow-manager)
  - [Overview](#overview)
    - [Workflow Object](#workflow-object)
  - [Examples](#examples)
    - [Informatics Gateway](#informatics-gateway)
    - [Tasks](#tasks)
    - [Task Types](#task-types)
    - [Example of Plugin Names](#example-of-plugin-names)
- [Task Object](#task-object)
  - [Common Feilds](#common-feilds)
    - [Data Attributes for specific task types](#data-attributes-for-specific-task-types)
      - [Router](#router)
      - [Export](#export)
      - [Plugin](#plugin)
    - [Artifacts](#artifacts)
      - [Artifact Map](#artifact-map)
      - [Artifact](#artifact)
      - [Task Types](#task-types-1)
        - [Argo](#argo)
          - [Resource Request Object](#resource-request-object)
    - [Evaluators](#evaluators)
      - [Context](#context)
        - [Execution Context](#execution-context)
        - [DICOM Tags](#dicom-tags)
    - [Destinations](#destinations)
      - [Task Destinations](#task-destinations)
      - [Export Destinations](#export-destinations)
    - [Retention Policies](#retention-policies)



### Workflow Object
This is the top-level object in a workflow spec. It contains the following properties:

| Property | Type |
|------|------|
|name|str (15)|
|version|str|
|description|Optional[str] (200)|
|informatics_gateway|[InformaticsGateway](#informatics-gateway)|
|tasks|list[[Task](#tasks)]|
|task_templates|Optional[list[[TaskTemplate](#task-templates)]]|
|retention_policy|Optional[list[[RetentionPolicy](#retention-policies)]]|


## Examples
The following is an example of a complete workflow:
![scenario1](static/workflow_examples/scenario1.png)

The JSON implementing that scenario is available [here](static/workflow_examples/scenario1.json).
*Note: the examples are not up to date. Please do not rely on them*
### Informatics Gateway
This section contains the IG configuration. Specifically, it contains the following properties:

| Property | Type | Description |
|------|------|------|
|ae_title|str|The AE title for this workflow. Only data sent to this AE title will be processed by this workflow.|
|data_origins|list[str]|List of possible origin systems. These should be registered with the informatics gateway.|
|export_destinations|list[str]|List of possible destinations for the output of tasks in this workflow. Informatics gateways can subscribe to notifications of output to these destinations.|

```json
{
    "ae_title": "MY_AET",
    "data_origins": ["MY_MODALITY"],
    "export_destinations": ["PROD_PACS"]
}
```
The above specifies that the workflow should be triggered for inputs sent to the ae-title "MY_AET" from "MY_MODALITY".
It also defines the "PROD_PACS" output destination, meaning that it can be used:
* By tasks as the [destination of their output](#output).
* By subscribers to [export notifications](mwm-sadd.md#export-service).

### Tasks
Tasks are the basic building block of a workflow. They are provided as a list - the first Task in the list is executed when the workflow is triggered.
Subsequent tasks are triggered by the `task_destinations` specified by previous tasks.


### Task Types
These tasks are borken down into different types:

| Type | Purpose |
| ---- | ------- |
| Router | A task to control the flow through a workflow |
| Export | A task to trigger the exporting of data |
| `Plugin_Name` | A task which has a matching Plugin installed & enabled |


### Example of Plugin Names
| Type | Purpose |
| ---- | ------- |
| Argo | A task which will trigger the execution of an Argo Workflow |


# Task Object 

## Common Feilds
All task objects can have these attributes:-

| Property | Type | Description |
|------|------|------|
|id|str (15)|The id for this task. This should be unique within the current workflow.|
|description|str (2000)|A human readable task description|
|type|str (2000)|The task type - this determines the plugin that will be used to execute the task. See [task types](#task-types) for supported tasks.|
|timeout_minutes|number|How long the task is allowed to run before it's canceled|
|timeout_retries|number|How many retries are allowed|
|task_destinations|Optional[list[[TaskDestination](#task-destinations)]]|An optional list of possible tasks that could be executed following this task. They will be executed if their conditions are true.|

### Data Attributes for specific task types
Depending of the type of task, the task object may contain additional fields.

#### Router
Router tasks don't have additional fields. They are used to contain `task_destinations` so that workflow processing can be directed to the desired next step.

#### Export
These are task types that allow for artifacts to be exported based on the input artifacts list. This task type should not have Out artifacts listed.  
The task also requires these extra attributes:-

| Property | Type | Description |
|------|------|------|
|export_destinations|Optional[list[[ExportDestination](#export-destinations)]]|An optional lists of possible export destinations to which the output of this task can be sent.|
|artifacts|[ArtifactMap](#artifacts)| Only Input artifacts of this task.

#### Plugin
These are tasks are Named the same as the installed Pluging. 
The task also requires these extra attributes:-

| Property | Type | Description |
|------|------|------|
|args|object|An object that will be available to the task plugin when executing this task. This schema of this object is defined by the plugin itself.|
|artifacts|[ArtifactMap](#artifacts)|Input & output artifacts of this task.


### Artifacts

Tasks can be provided with input artifacts, and they generate output artifacts.
Inputs can contain artifacts generated by previous tasks or the original workflow input.

The root level Task object contains an optional ArtifactMap object:

#### Artifact Map

| Property | Type | Description |
|------|------|------|
|input|list[Artifact]|List of artifacts that are needed by this task.
|output|list[Artifact]|List of artifacts that are generated by this task.


#### Artifact
Each Artifact contains at least two elements:

| Property | Type | Required | Description |
|------|------|------|-----|
|name|str|Always|The name of this artifact.|
|value|Artifact identifier|Required for inputs, optional for outputs.|The context variable for this artifact (see example). If defined for output, that value will be used instead of a Task-generated one.|
| mandatory| bool | No (default false) | Determines whether this artifact is mandatory. If a mandatory artifact doesn't exist the task is marked as a failure and workflow execution stops. |


As you can see in the example below, input artifacts require a _value_. This is a reference to a previously generated artifact, or to `context.input` - a value for an artifact representing the original input that triggered the workflow.

> ##### Note regarding artifact names:
> it's not necessary for the input name to be identical to the _name_ specified in the output of a previous task. Output names are used to allow access using the context object. The input name should be what the current task expects to receive.


Example:
```json
"input": [
    {
      "name": "input_dicom",
      "value": "{{ context.executions.image_type_detector.artifacts.dicom }}",
      "mandatory": true
    }
],
"output": [
    {
      "name": "fracture_report",
      "mandatory": true
    }
]
```

> ##### Creating artifacts:
> How artifacts are created is beyond the scope of this document. The specifics differ depending on which Task Plugin is being used.
>
> It is possible to "hard-code" artifacts by providing the value for an output artifact. This is useful for tasks that are auxillary such as utility functions (e.g. email sending) or review steps - those tasks output the same input they've received.

#### Task Types
Tasks are implemented by task _plugins_. The following are the officially supported plugins that are bundled with the MWM.

##### Argo
The Argo plugin triggers workflows pre-deployed onto an [Argo workflow server](https://argoproj.github.io/argo-events/).

**type**: argo

The Task's "args" object should contain the following fields:

| Property | Type | Required | Description |
|------|------|------|------|
|workflow_id|str|Yes|The ID of this workflow as registered on the Argo server.|
|server_url|url|Yes|The URL of the Argo server.|
|parameters|dictionary|No|Key value pairs, Argo parameters that will be passed on to the Argo workflow.|
|priority_class|string|No|The name of a valid Kubernetes priority class to be assigned to the Argo workflow pods|
|resources|object|No|A resource requests & limits object (see below). These will be applied to the Argo workflow pods|

###### Resource Request Object

| Property | Type | Description |
|------|------|------|
|memory_reservation|str|A valid [Kubernetes memory request value](https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/#meaning-of-memory).|
|cpu_reservation|url|A valid [Kubernetes CPU request value](https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/#meaning-of-cpu).|
|gpu_limit|dictionary|The number of GPUs to be used by this task.|
|memory_limit|string|The maximum amount of memory this task may use|
|cpu_limit|object|The maximum amount of CPU this task may use. See |

For more information about Kubernetes requests & limits, see https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/.



### Evaluators
Evaluators are logical statement strings that may be used to determine which tasks are executed. They can make use of the execution context _metadata_.

#### Context
The workflow metadata is any data that can be used by Evaluators. This includes metadata added by previous tasks, but can also include metadata about the input files (most notably DICOM tags).
Metadata is available with the `context` object:

| Property | Type | Description |
|------|------|------|
|correlation_id|str|The unique ID identifying this Workflow execution.|
|input|Artifact|A reference to an artifact representing the workflow input data.|
|executions|Execution Context|This object contains the data added by previous tasks.|
|dicom|DICOM Tags object|This object contains the DICOM tags of the input.|

#####  Execution Context
The `executions` object contains data generated by previous executions.
Each execution is represented by an `Execution` object, which can be accessed by Task ID:
`context.executions.TASK_ID`

The Execution object contains the following properties:

| Property | Type | Description |
|------|------|------|
|execution_id|str|The unique ID identifying task execution.|
|task_id|str|The ID of this task in the workflow.|
|input_dir|str|Path to the input directory of this task.|
|output_dir|str|Path to the output directory of this task.|
|task|dict|The details of the executed task. Similar to the Task definition in the workflow spec.|
|start_time|timestamp|The UTC timestamp of when this task execution began.
|end_time|Optional[timestamp]|The UTC timestamp of when this task execution ended.
|status|string|One of "success", "fail" or "error".
|error_msg|Optional[str]|An error message, if one occurred.
|result|Optional[dict]|The metadata that was [added by the previous Task](#tasks).

Example:
```python
{{context.executions.body_part_identifier.result.body_part}} == 'leg'
```

##### DICOM Tags
When the input data is DICOM, Evaluators can use DICOM tag values in conditional statements.
DICOM tags are available in `context.input.dicom`. The reference for that object is as follows:

| Property | Type | Description |
|------|------|------|
|study_id|str|The Study ID tag (0020,0010)|
|tags|dict|All DICOM metadata tags that are common across all series & slices in this study.|
|series|list|The list of DICOM series in this study.|

Each `Series` object contains the tags of that series. They can be accessed either with dot notation or using key lookups (see examples below).

The DICOM tag matching engine allows evaluating conditions against all series and resulting in True if the condition matches _any one_ of them:
```python
{{context.input.dicom.series.any('0018','0050')}} < 5
```

In order to check a certain tag across _all_ series, use the study level tags. For example, to only evaluate True for Female patients:
```python
{{context.input.dicom.series.all('0010','0040')}} == 'F'
```

### Destinations
Destinations allow the workflow manager to determine what should happen to the output of a task. There are two types of destinations – task destinations, which reference another task in the workflow to be executed and export destinations, which reference a location external to the workflow manager.

#### Task Destinations

Task destinations define the next task to be executed. 
Sometimes the destination will differ based on some condition. For this, [evaluators](#evaluators) can be used as conditions for output destinations.

The basic format is as follows:

| Property | Type | Description |
|------|------|------|
|name|str|The name of the destination. This can either be an export destinations or a task's ID.|
|conditions|Optional[list[Evaluator]]|An optional array of [Evaluators](#evaluators) that need to be met in order for this destination to be used.|


Example (run my-task-id when the patient is female):

```json
{
    ...task...
    "task_destinations": [
        {
            "name": "my-task-id",
            "conditions": ["{{context.input.dicom.series.all('0010','0040')}} == 'F'"]
        },
    ],
    ...
}
```


#### Export Destinations
Export destinations define an external location to which the output of the task can be sent. This will take the form of an event published to a pub/sub service notifying of an available export to a specific destination reference. Most commonly, the export location will be a PACs system and the notification will be picked up by the Monai Informatics Gateway.

| Property | Type | Description |
|------|------|------|
|name|str|The name of the destination. This can either be an export destinations already defined within the [Informatics Gateway](#informatics-gateway) section of  the workflow configuration.|
|artifacts|list[Artifact]|An array of [Artifacts](#artifacts) that should be sent to this export destination.|

Example (output sent to another task if the patient is female, otherwise to PACS):
```json
{
    ...task...
    "export_destinations": [
        {
            "name": "PROD_PACS"
        }
    ],
    "task_destinations": [
        {
            "name": "my-task-id",
            "conditions": ["{{context.input.dicom.series.all('0010','0040')}} == 'F'"]
        }
    ],
    ...
}
```

### Retention Policies
_Future version: TBD_

-- This is a work in progress --

Retention policies define how long the data processed and generated by this workflow should be kept.

| Property | Type | Description |
|------|------|------|
|days|int|The number of days to keep the data.|
