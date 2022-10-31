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

using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 600000, Level = LogLevel.Error, Message = "Artifact Passed data  Artifact {artifact}, Path {path}, ArtifactType {artifactType}, Exists {exists}")]
        public static partial void LogArtifactPassing(this ILogger logger, Artifact artifact, string path, string artifactType, bool exists);

        [LoggerMessage(EventId = 600001, Level = LogLevel.Debug, Message = "Verifying artifact existence on bucket {bucket}: {artifactKey}={artifactValue}.")]
        public static partial void VerifyArtifactExistence(this ILogger logger, string bucket, string artifactKey, string artifactValue);

        [LoggerMessage(EventId = 600002, Level = LogLevel.Debug, Message = "Converting variable to string path: {variableString}={variableString}.")]
        public static partial void ConvertingVariableStringToPath(this ILogger logger, string variableString);

        [LoggerMessage(EventId = 600003, Level = LogLevel.Debug, Message = "Failed to convert artifact variable to path.")]
        public static partial void ConvertArtifactVariablesToPathError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 600004, Level = LogLevel.Debug, Message = "Mandatory output artefacts for task {taskId} are missing.")]
        public static partial void MandatoryOutputArtefactsMissingForTask(this ILogger logger, string taskId);

        [LoggerMessage(EventId = 600005, Level = LogLevel.Error, Message = "The following payload: {payloadId} in workflow instance {workflowInstanceId} workflow revision {workflowRevisionId} for task {taskId} failed to load artifact and was unable to update DB.")]
        public static partial void LoadArtifactAndDBFailiure(this ILogger logger, string payloadId, string taskId, string workflowInstanceId, string workflowRevisionId);
    }
}
