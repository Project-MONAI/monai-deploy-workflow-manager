// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Common
{
    public interface IArtifactMapper
    {
        Task<Dictionary<string, string>> ConvertArtifactVariablesToPath(Artifact[] artifacts, string payloadId, string workflowInstanceId);
    }
}
