// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.WorkfowExecuter.Common
{
    public interface IArtifactMapper
    {
        /// <summary>
        /// Converts an array of artifacts to a dictionary of artifact path variables.
        /// </summary>
        /// <param name="artifacts">Array of artifacts to convert.</param>
        /// <param name="payloadId">Payload id to check against.</param>
        /// <param name="workflowInstanceId">Workflow instance id to check against.</param>
        /// <param name="bucketId">Bucket id used to verify.</param>
        Task<Dictionary<string, string>> ConvertArtifactVariablesToPath(Artifact[] artifacts, string payloadId, string workflowInstanceId, string bucketId);
    }
}
