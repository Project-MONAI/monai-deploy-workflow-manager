# MONAI Deploy Workflow Manager

[![License](https://img.shields.io/badge/license-Apache%202.0-green.svg)](LICENSE)
[![codecov](https://codecov.io/gh/Project-MONAI/monai-deploy-workload-manager/branch/main/graph/badge.svg?token=NXYQIABXZ7)](https://codecov.io/gh/Project-MONAI/monai-deploy-workload-manager)

This repository contains the Workload Manager subsystem, part of MONAI Deploy. Please refer to the main [MONAI Deploy](https://github.com/Project-MONAI/monai-deploy) repo for more details.

## Build

### Prerequisites

- [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)

### Development Environment

During development, change any settings inside the `appsettings.Development.json` file.
First, export the following environment variable before executing `dotnet run`:

```bash
export DOTNET_ENVIRONMENT=Development
```

### Building MONAI Workload Manager

```bash
cd src && dotnet build
```
