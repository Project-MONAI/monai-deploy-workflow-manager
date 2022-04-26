// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using System.Threading.Tasks;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Database.Interfaces
{
    public interface IWorkflowRepository
    {
        Task<List<Workflow>> GetListByAeTitleAsync(string aeTitle);

        Task<List<Workflow>> GetByWorkflowsIdsAsync(IEnumerable<string> ids);
    }
}
