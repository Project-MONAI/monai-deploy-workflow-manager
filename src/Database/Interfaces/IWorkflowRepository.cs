// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Database.Interfaces
{
    public interface IWorkflowRepository
    {
        Task<Workflow> GetByWorkflowIdAsync(Guid id);

        Task<IList<Workflow>> GetByWorkflowsIdsAsync(IEnumerable<string> ids);

        Task<Workflow> GetByAeTitleAsync(string aeTitle);

        Task<IList<Workflow>> GetWorkflowsByAeTitleAsync(string aeTitle);
    }
}
