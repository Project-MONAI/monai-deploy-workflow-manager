/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Common
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
        /// <param name="shouldExistYet">Checks if it should exist yet.</param>
        Task<Dictionary<string, string>> ConvertArtifactVariablesToPath(Artifact[] artifacts, string payloadId, string workflowInstanceId, string bucketId, bool shouldExistYet = true);

        /// <summary>
        /// If ConvertArtifactVariablesToPath throws a FileNotFoundException will only return false and artifactPaths will be empty.
        /// </summary>
        /// <param name="artifacts"></param>
        /// <param name="payloadId"></param>
        /// <param name="workflowInstanceId"></param>
        /// <param name="bucketId"></param>
        /// <param name="shouldExistYet"></param>
        /// <param name="artifactPaths"></param>
        /// <returns>bool if ConvertArtifactVariablesToPath runs successfully</returns>
        bool TryConvertArtifactVariablesToPath(Artifact[] artifacts, string payloadId, string workflowInstanceId, string bucketId, bool shouldExistYet, out Dictionary<string, string> artifactPaths);
    }
}
