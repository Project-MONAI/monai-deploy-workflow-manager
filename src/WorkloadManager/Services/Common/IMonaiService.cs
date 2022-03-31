// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkloadManager.Contracts.Rest;

namespace Monai.Deploy.WorkloadManager.Services.Common
{
    public interface IMonaiService
    {
        ServiceStatus Status { get; set; }
        string ServiceName { get; }
    }
}
