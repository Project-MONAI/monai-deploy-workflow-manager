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

<p align="center">
<img src="https://raw.githubusercontent.com/Project-MONAI/MONAI/dev/docs/images/MONAI-logo-color.png" width="50%" alt='project-monai'>
</p>

💡 If you want to know more about MONAI Deploy WG vision, overall structure, and guidelines, please read [MONAI Deploy](https://github.com/Project-MONAI/monai-deploy) first.

# MONAI Deploy Workflow Manager

[![License](https://img.shields.io/badge/license-Apache%202.0-green.svg)](LICENSE)
[![codecov](https://codecov.io/gh/Project-MONAI/monai-deploy-workflow-manager/branch/main/graph/badge.svg?token=NXYQIABXZ7)](https://codecov.io/gh/Project-MONAI/monai-deploy-workflow-manager)

This repository contains the Workflow Manager subsystem, part of MONAI Deploy. Please refer to the main [MONAI Deploy](https://github.com/Project-MONAI/monai-deploy) repo for more details.

## Build

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

### Development Environment

During development, change any settings inside the `appsettings.Development.json` file.
First, export the following environment variable before executing `dotnet run`:

#### Linux 

```bash
export DOTNET_ENVIRONMENT=Development
```
#### Powershell

```powershell
$env:DOTNET_ENVIRONMENT="Development"
```


### Building MONAI Workflow Manager

```bash
cd src && dotnet build
```

## Contributing

For guidance on making a contribution to MONAI Deploy Workflow Manager, see the [contributing guidelines](https://github.com/Project-MONAI/monai-deploy/blob/main/CONTRIBUTING.md).

## Community

To participate, please join the MONAI Deploy Workflow Manager weekly meetings on the [calendar](https://calendar.google.com/calendar/u/0/embed?src=c_954820qfk2pdbge9ofnj5pnt0g@group.calendar.google.com&ctz=America/New_York) and review the [meeting notes](https://docs.google.com/document/d/1ipCGxlq0Pd7Xnil2zGa1va99K7VbdhwcJiqel9aWzyA/edit?usp=sharing).

Join the conversation on Twitter [@ProjectMONAI](https://twitter.com/ProjectMONAI) or join our [Slack channel](https://forms.gle/QTxJq3hFictp31UM9).

Ask and answer questions over on [MONAI Deploy Workflow Manager's GitHub Discussions tab](https://github.com/Project-MONAI/monai-deploy-workflow-manager/discussions).

## Links

- Website: <https://monai.io>
- Code: <https://github.com/Project-MONAI/monai-deploy-workflow-manager>
- Project tracker: <https://github.com/Project-MONAI/monai-deploy-workflow-manager/projects>
- Issue tracker: <https://github.com/Project-MONAI/monai-deploy-workflow-manager/issues>
- Test status: <https://github.com/Project-MONAI/monai-deploy-workflow-manager/actions>
