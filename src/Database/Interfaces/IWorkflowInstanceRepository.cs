// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using System.Threading.Tasks;

namespace Monai.Deploy.WorkflowManager.Database.Interfaces
{
    public interface IWorkflowInstanceRepository
    {

        Task<bool> CreateAsync(IList<WorkflowInstance> workflows);
    }
}
