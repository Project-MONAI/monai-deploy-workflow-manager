// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Services
{
    public interface IWorkflowMetadataRepository
    {
        Task<bool> UpdateForTaskAsync(string workflowInstanceId, List<TaskMetadata> metadata);
    }
}