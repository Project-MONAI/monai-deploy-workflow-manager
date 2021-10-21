# MONAI Deploy Workload Manager Requirements

![MONAI Deploy Workload Manager](./static/mwm.png)

- [MONAI Deploy Workload Manager Requirements](#monai-deploy-workload-manager-requirements)
  - [Overview](#overview)
  - [Scope](#scope)
  - [Goal](#goal)
  - [Success Criteria](#success-criteria)
  - [Attributes of a Requirement](#attributes-of-a-requirement)
  - [Definitions, Acronyms, Abbreviations](#definitions-acronyms-abbreviations)
  - [(REQ-DI) Data Ingestion Requirements](#req-di-data-ingestion-requirements)
    - [[REQ-DI-001] MWM SHALL allow users to upload data](#req-di-001-mwm-shall-allow-users-to-upload-data)
    - [[REQ-DI-002] MWM SHALL allow users to notify data arrival via shared storages](#req-di-002-mwm-shall-allow-users-to-notify-data-arrival-via-shared-storages)
    - [[REQ-DI-003] MWM SHALL allow users to upload data and associate with one or more applications](#req-di-003-mwm-shall-allow-users-to-upload-data-and-associate-with-one-or-more-applications)
    - [[REQ-DI-004] MWM SHALL be able to discover applications deployed on MONAI App Server](#req-di-004-mwm-shall-be-able-to-discover-applications-deployed-on-monai-app-server)
    - [[REQ-DI-005] MWM SHALL provide an API to register applications](#req-di-005-mwm-shall-provide-an-api-to-register-applications)
  - [(REQ-DR) App Discovery Service/Data Filtering Rules Requirements](#req-dr-app-discovery-servicedata-filtering-rules-requirements)
    - [[REQ-DR-001] MWM App Discovery Service (ADS) SHALL be able to filter data by DICOM headers](#req-dr-001-mwm-app-discovery-service-ads-shall-be-able-to-filter-data-by-dicom-headers)
    - [[REQ-DR-002] MWM App Discovery Service (ADS) SHALL allow users to configure how long to wait for data before launching a job](#req-dr-002-mwm-app-discovery-service-ads-shall-allow-users-to-configure-how-long-to-wait-for-data-before-launching-a-job)
    - [[REQ-DR-003] MWM App Discovery Service (ADS) SHALL be able to filter data by FHIR data fields](#req-dr-003-mwm-app-discovery-service-ads-shall-be-able-to-filter-data-by-fhir-data-fields)
    - [[REQ-DR-004] MWM SHALL respect user-defined data discovery rules](#req-dr-004-mwm-shall-respect-user-defined-data-discovery-rules)
    - [[REQ-DR-005] MWM SHALL be able to route incoming data to one or more applications](#req-dr-005-mwm-shall-be-able-to-route-incoming-data-to-one-or-more-applications)
  - [(REQ-DX) Data Export Requirements](#req-dx-data-export-requirements)
    - [[REQ-DX-001] MWM SHALL support multiple export sinks (destinations)](#req-dx-001-mwm-shall-support-multiple-export-sinks-destinations)
    - [[REQ-DX-002] MWM SHALL be able to route data to multiple sinks](#req-dx-002-mwm-shall-be-able-to-route-data-to-multiple-sinks)
    - [[REQ-DX-003] MWM SHALL allow users to create custom sinks](#req-dx-003-mwm-shall-allow-users-to-create-custom-sinks)
  - [(REQ-FR) Functional Requirements](#req-fr-functional-requirements)
    - [[REQ-FR-001] MWM SHALL provide a mechanism to develop plugins for discovering applications](#req-fr-001-mwm-shall-provide-a-mechanism-to-develop-plugins-for-discovering-applications)
    - [[REQ-FR-002] MWM SHALL track status/states of all jobs initiated with orchestration engines](#req-fr-002-mwm-shall-track-statusstates-of-all-jobs-initiated-with-orchestration-engines)
    - [[REQ-FR-003] MWM SHALL provide a mechanism for clients to subscribe to notifications](#req-fr-003-mwm-shall-provide-a-mechanism-for-clients-to-subscribe-to-notifications)
    - [[REQ-FR-004] MWM SHALL allow users to define storage cleanup rules](#req-fr-004-mwm-shall-allow-users-to-define-storage-cleanup-rules)
    - [[REQ-FR-005] MWM SHALL allow application outputs routed to other applications](#req-fr-005-mwm-shall-allow-application-outputs-routed-to-other-applications)

## Overview

The MONAI Deploy Workload Manager (MWM) is the central hub for the MONAI Deploy platform. It routes received medical data from MONAI Informatics Gateway (or your custom ingestion service) to MONAI applications based on user-defined rulesets using the App Discovery Service. It is also responsible for monitoring application execution statuses and routing any results produced by the applications back to the configured destinations.

## Scope

The scope of this document is limited to the MONAI Deploy Workload Manager. There are other subsystems of the MONAI Deploy platform such as [MONAI Deploy Informatics Gateway](https://github.com/Project-MONAI/monai-deploy-informatics-gateway) (responsible for interoperability with external systems) and [MONAI App SDK](https://github.com/Project-MONAI/monai-app-sdk) (which provides the necessary APIs for the application developer to interact with the MWM). However, this requirements document does not address specifications belonging to those subsystems.

## Goal

The goal for this proposal is to enlist, prioritize and provide clarity on the requirements for MONAI Deploy Workload Manager. Developers working on different software modules in MONAI Deploy Workload Manager SHALL use this specification as a guideline when designing and implementing software for the MONAI Deploy Workload Manager.

## Success Criteria

Data SHALL be routed to the user-defined applications and results, if any, SHALL be routed back to configured destinations.

## Attributes of a Requirement

For each requirement, the following attributes have been specified:

**Requirement Body**: This is the text of the requirement which describes the goal and purpose behind the requirement.

**Background**: Provides necessary background to understand the context of the requirements.

**Verification Strategy**: A high-level plan on how to test this requirement at a system level.

**Target Release**: Specifies which release of the MONAI App SDK this requirement is targeted for.

## Definitions, Acronyms, Abbreviations

| Term        | Definition                                                                                                                                                      |
| ----------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| DDR         | Data Discovery Ruleset: a set of static rules defined by the users to determine if an incoming dataset meets the criteria of an application.                    |
| ADS         | App Discovery Service: a service that applies all DDRs to incoming payloads                                                                                     |
| Export Sink | An export sink is a user-configured sink where the results (application-generated artifacts) are assigned to and later picked up by the export service clients. |
| MIG         | MONAI Deploy Informatics Gateway                                                                                                                                |
| MWM         | MONAI Deploy Workload Manager                                                                                                                                   |

## (REQ-DI) Data Ingestion Requirements

### [REQ-DI-001] MWM SHALL allow users to upload data

An API MUST be provided to the data ingestion services, such as the Informatics Gateway, to upload payloads to the MONAI Workload Manager.

#### Background

With the design of MONAI Deploy, the MWM does not interface with HIS/RIS directly but rather through the Informatics Gateway (a data ingestion & export service). Therefore, APIs are provided to interface with any data ingestion services, allowing platform users to extend these APIs to interface their systems using different messaging protocols or storage services.

#### Verification Strategy

Verify that payloads can be uploaded from data ingestion services and dispatched to the App Discovery Service.

#### Target Release

MONAI Deploy Workload Manager R1

### [REQ-DI-002] MWM SHALL allow users to notify data arrival via shared storages

An API MUST be provided to the data ingestion services, such as the Informatics Gateway, to notify data has arrived at the shared storage. E.g., a mounted NAS volume or cloud storage services.

#### Background

Medical imaging data are relatively large, and transferring data between devices takes a significant amount of time of any given workflow. Often, shared storage is used to reduce the amount of data being transferred across services.

#### Verification Strategy

Verify that payloads can be uploaded from data ingestion services and dispatched to the App Discovery Service.

#### Target Release

MONAI Deploy Workload Manager R1

### [REQ-DI-003] MWM SHALL allow users to upload data and associate with one or more applications

An API SHALL be provided to allow data to be uploaded and routed to one or more designated applications directly bypassing the data filters in App Discovery Service.

#### Background

In a scenario where the application to be executed is already known, the API would skip the data filtering phase of the App Discovery Service.

#### Verification Strategy

Verify that payloads can be uploaded from a data ingestion service and then trigger one or more applications.

#### Target Release

MONAI Deploy Workload Manager R1

### [REQ-DI-004] MWM SHALL be able to discover applications deployed on MONAI App Server

MWM SHALL discover applications deployed on the MONAI App Server and make them available to the App Discovery Service and export sinks.

#### Background

Given that the users (system admins, workflow engineers, etc...) need to connect data discovery rules and sinks to deployed applications, MWM must first know their existence and provide a way to identify those applications.

#### Verification Strategy

For a deployed application, it must be associable by the App Discovery Service or export sinks.

#### Target Release

MONAI Deploy Workload Manager R1


### [REQ-DI-005] MWM SHALL provide an API to register applications

MWM SHALL allow users to register applications for supported third-party orchestration/workflow engines.

#### Background

Some orchestration/workflow engines do not support the concept of persisting workflows but instead launch the workflows immediately based on the provided workflow definitions.  This requirement supports such scenarios and allows users to register their workflow definition with the Workload Manager.

#### Verification Strategy

Register a supported workflow with MWM.

#### Target Release

MONAI Deploy Workload Manager R1


## (REQ-DR) App Discovery Service/Data Filtering Rules Requirements

### [REQ-DR-001] MWM App Discovery Service (ADS) SHALL be able to filter data by DICOM headers

MWM ADS SHALL allow users to define filtering rules based on DICOM Attributes that do not require parsing pixel data.

#### Background

Given that multiple applications may be deployed on the MONAI Deploy platform and often more jobs are scheduled and launched than available resources. To avoid launching all applications and let the applications decide if a dataset is a fit, the ADS applies user-defined DICOM header rules to select the dataset that meets its requirements before launching the application.

#### Verification Strategy

Given a set of data discovery rules using the pre-built functions and a DICOM dataset, the App Discovery Service applies the ruleset to select any data that match the filtering criteria.

#### Target Release

MONAI Deploy Workload Manager R1

### [REQ-DR-002] MWM App Discovery Service (ADS) SHALL allow users to configure how long to wait for data before launching a job

MWM ADS SHALL allow users to define a time range to wait for all data to be ready before launching the associated application(s)

#### Background

Often time, data comes in through multiple connections or different sources. E.g., a DICOM study may be sent over multiple associations. This requirement allows the ADS to wait for a period before it assembles the payload for the associated application(s).

#### Verification Strategy

Configure a rule set with a timeout and send data that meets the requirements of an application in separate connections within the timeout defined.

#### Target Release

MONAI Deploy Workload Manager R1

### [REQ-DR-003] MWM App Discovery Service (ADS) SHALL be able to filter data by FHIR data fields

MWM ADS SHALL allow users to define filtering rules based on FHIR data attributes using pre-built functions, such as equals, contains, greater, greater-than, less, less-than, etc...

#### Background

Given that multiple applications may be deployed on the MONAI Deploy platform, more jobs are often scheduled and launched than available resources. To avoid launching all applications and let the applications decide if a dataset fits, the ADS applies user-defined FHIR filtering rules to select the dataset that meets application requirements before launching the application.

#### Verification Strategy

Given a set of data discovery rules using the pre-built functions and some FHIR resources, the App Discovery Service applies the rules to remove any data that does not meet the application's requirements.

#### Target Release

MONAI Deploy Workload Manager R2

### [REQ-DR-004] MWM SHALL respect user-defined data discovery rules

App Discovery Service MUST apply all user-defined rules to the data arrived at the system.

#### Background

An application/model is often designed and restricted to a particular type/format of data. A data discovery rule is a pre-filter that validates a given dataset to see if the dataset meets the requirements of an/a application/model.

#### Verification Strategy

Given a data discovery rule set and a dataset, the App Discovery Service applies the rules to removes any data that is not suitable.

#### Target Release

MONAI Deploy Workload Manager R1

### [REQ-DR-005] MWM SHALL be able to route incoming data to one or more applications

A user-defined data discovery rule SHALL be associable with one or more deployed applications.

#### Background

A given set of user-defined data discovery rules can sometimes meet the criteria of multiple applications/models. This allows users to deploy the rules once and associate them with multiple applications/models.

#### Verification Strategy

Deploy a rule set and associate it with two applications. Verify that both applications are launched when data is received and meets the requirements of the rule sets.

#### Target Release

MONAI Deploy Workload Manager R1

## (REQ-DX) Data Export Requirements

### [REQ-DX-001] MWM SHALL support multiple export sinks (destinations)

An export sink is an association of an application and a data export service that allows results generated by the application to be exported to the designated destination.

#### Background

AI applications and medical imaging algorithms often output data in different formats that need to be exported back to HIS/RIS for reading, storage, or validation. The concept of a sink links a deployed application to an export service so the output data can be exported back to RIS/HIS applications.

#### Verification Strategy

Verify that applications can be linked to a sink.

#### Target Release

MONAI Deploy Workload Manager R1

### [REQ-DX-002] MWM SHALL be able to route data to multiple sinks

MWM SHALL allow multiple sinks to be linked to an application so output data can be exported to multiple destinations.

#### Background

There are a few common data protocols in the healthcare industry, such as DICOM, HL7, and FHIR. Often each protocol requires a dedicated client to communicate with it. Given that applications often produce outputs in different formats, each format, again, DICOM, HL7, or FHIR, must be handled by a dedicated client. This requirement would enable users to link one application to multiple sinks.

#### Verification Strategy

Link a deployed application to multiple sinks.

#### Target Release

MONAI Deploy Workload Manager R1

### [REQ-DX-003] MWM SHALL allow users to create custom sinks

In order to support custom export services, MWM SHALL allow custom sinks to be created.

#### Background

Besides industry-standard protocols, such as DICOM, there may be other proprietary methods of transmitting data. This requirement allows users to enable their services to pick up any results produced by their applications.

#### Verification Strategy

Create an application to simulate an export service by implementing available APIs.

#### Target Release

MONAI Deploy Workload Manager R1

## (REQ-FR) Functional Requirements

### [REQ-FR-001] MWM SHALL provide a mechanism to develop plugins for discovering applications

Besides integrating MONAI App Server, MWM SHALL provide a mechanism to allow users to develop plugins to discover apps registered with an orchestration engine such as Argo.

#### Background

Many existing users have already invested in other orchestration engines, which may have already become a requirement for their workflow. Therefore, supporting other OSS orchestration engines would simplify the integration of their existing environment with MONAI products.

#### Verification Strategy

Verify by repeating the same workflow and the same application on both MONAI App Server and another OSS orchestration engine, such as Argo.

#### Target Release

MONAI Deploy Workload Manager R2



### [REQ-FR-002] MWM SHALL track status/states of all jobs initiated with orchestration engines

MWM SHALL track the status and states of all the jobs that it has initiated so it can be used for reporting and allows other sub-components to react upon.

#### Background

Internally, in MWM, many subcomponents need to know the state and statuses of a running job. It is ideal to have a dedicated component that queries all the configured orchestration engines and share that information among other parts of the system.

#### Verification Strategy

Set up MWM with two orchestration engines, trigger a couple of jobs and make sure statues and states are stored in MWM.

#### Target Release

MONAI Deploy Workload Manager R1

### [REQ-FR-003] MWM SHALL provide a mechanism for clients to subscribe to notifications

MWM SHALL provide a mechanism so that users or clients can subscribe to the notification service to get job status or other system information.

#### Background

Maintainers typically require real-time or (daily, hourly, weekly) summaries on any failures experienced by the system, especially in cases of logical errors in running applications.

#### Verification Strategy

Send a data format into a test application that is not able to process that data format and expect notifications on failures. Send fewer (one less) or more (one more) studies than required into an application that requires a specific amount of studies and expect notifications of failure(s).

#### Target Release

MONAI Deploy Workload Manager R3

### [REQ-FR-004] MWM SHALL allow users to define storage cleanup rules

MWM SHALL provide functionalities to let the users configure when data are removed from the MWM cache.

#### Background

Often medical records, especially medical images, requires a large amount of disk storage, and given that disk storage space is always limited, there must exist a method to remove payloads. For MWM, payloads that are associated with a job that completes in a successful state may be removed. However, jobs that failed may need to be retained for further investigation.

#### Verification Strategy

Verify that payloads are removed based on users' configuration.

#### Target Release

MONAI Deploy Workload Manager R2

### [REQ-FR-005] MWM SHALL allow application outputs routed to other applications

MWM SHALL allow the output of an application routed back to other application(s).

#### Background

Given that ADS only filters data based on a static list of rules and cannot apply complex algorithms to a dataset, it may often not meet the needs of an application. Therefore, this requirement enables the user to construct a complex data filtering application to decide and output the suitable dataset for another application.

#### Verification Strategy

Deploy and link a data filtering application and the main application. Verify that the main application receives output from the data filtering application.

#### Target Release

MONAI Deploy Workload Manager R2
