// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Interfaces
{
    public interface ITasksService : IPaginatedApi<TaskExecution>
    {
        Task<TaskExecution> GetTaskAsync(string workflowInstanceId, string taskId, string executionId);
    }
}
