// SPDX-FileCopyrightText: � 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Rest;

namespace Monai.Deploy.WorkflowManager.Services.Common
{
    public interface IMonaiService
    {
        ServiceStatus Status { get; set; }
        string ServiceName { get; }
    }
}
