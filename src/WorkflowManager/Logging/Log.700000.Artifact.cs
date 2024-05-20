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
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 700000, Level = LogLevel.Debug, Message = "Artifact Passed data  Artifact {artifact}, Path {path}, ArtifactType {artifactType}, Exists {exists}")]
        public static partial void LogArtifactPassing(this ILogger logger, Artifact artifact, string path, string artifactType, bool exists);

        [LoggerMessage(EventId = 700001, Level = LogLevel.Debug, Message = "Verifying artifact existence on bucket {bucket}: {artifactKey}={artifactValue}.")]
        public static partial void VerifyArtifactExistence(this ILogger logger, string bucket, string artifactKey, string artifactValue);

        [LoggerMessage(EventId = 700002, Level = LogLevel.Debug, Message = "Converting variable to string path: {variableString}={variableString}.")]
        public static partial void ConvertingVariableStringToPath(this ILogger logger, string variableString);

        [LoggerMessage(EventId = 700003, Level = LogLevel.Debug, Message = "Failed to convert artifact variable to path.")]
        public static partial void ConvertArtifactVariablesToPathError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 700004, Level = LogLevel.Debug, Message = "Mandatory output artefacts for task {taskId} are missing.")]
        public static partial void MandatoryOutputArtefactsMissingForTask(this ILogger logger, string taskId);

        [LoggerMessage(EventId = 700005, Level = LogLevel.Error, Message = "The following payload: {payloadId} in workflow instance {workflowInstanceId} workflow revision {workflowRevisionId} for task {taskId} failed to load artifact and was unable to update DB.")]
        public static partial void LoadArtifactAndDBFailure(this ILogger logger, string payloadId, string taskId, string workflowInstanceId, string workflowRevisionId);

        [LoggerMessage(EventId = 700006, Level = LogLevel.Error, Message = "Task Dispatch failed to resolve output artifacts: PayloadId: {payloadId}, TaskId: {taskId}, WorkflowInstanceId: {workflowInstanceId}, WorkflowRevisionId: {workflowRevisionId}, output artifact object: {pathOutputArtifacts}")]
        public static partial void LogTaskDispatchFailure(this ILogger logger, string payloadId, string taskId, string workflowInstanceId, string workflowRevisionId, string pathOutputArtifacts);

        [LoggerMessage(EventId = 700007, Level = LogLevel.Information, Message = "Task Dispatch resolved successfully output artifacts: PayloadId: {payloadId}, TaskId: {taskId}, WorkflowInstanceId: {workflowInstanceId}, WorkflowRevisionId: {workflowRevisionId}, output artifact object: {pathOutputArtifacts}")]
        public static partial void LogGeneralTaskDispatchInformation(this ILogger logger, string payloadId, string taskId, string workflowInstanceId, string workflowRevisionId, string pathOutputArtifacts);

        [LoggerMessage(EventId = 700008, Level = LogLevel.Warning, Message = "Unexpected Artifacts output artifacts: TaskId: {taskId}, WorkflowInstanceId: {workflowInstanceId}, output artifact object: {unexpectedArtifacts}")]
        public static partial void UnexpectedArtifactsReceived(this ILogger logger, string taskId, string workflowInstanceId, string unexpectedArtifacts);

        [LoggerMessage(EventId = 700009, Level = LogLevel.Debug, Message = "Mandatory output artifacts for task {taskId} are missing. waiting for remaining artifacts... {missingArtifacts}")]
        public static partial void MandatoryOutputArtifactsMissingForTask(this ILogger logger, string taskId, string missingArtifacts);

        [LoggerMessage(EventId = 700010, Level = LogLevel.Debug, Message = "no files exsist in storage {artifactList}")]
        public static partial void NoFilesExistInStorage(this ILogger logger, string artifactList);

        [LoggerMessage(EventId = 700011, Level = LogLevel.Debug, Message = "adding files to workflowInstance {workflowInstanceId} :Task {taskId} :  {artifactList}")]
        public static partial void AddingFilesToWorkflowInstance(this ILogger logger, string workflowInstanceId, string taskId, string artifactList);

        [LoggerMessage(EventId = 700012, Level = LogLevel.Error, Message = "Error finding Task :{taskId}")]
        public static partial void ErrorFindingTask(this ILogger logger, string taskId);

        //[LoggerMessage(EventId = 700013, Level = LogLevel.Error, Message = "Error finding Task :{taskId} or previousTask {previousTask}")]
        //public static partial void ErrorFindingTaskOrPrevious(this ILogger logger, string taskId, string previousTask);

        [LoggerMessage(EventId = 700014, Level = LogLevel.Warning, Message = "Error Task :{taskId} cant be trigger as it has missing artifacts {missingtypesJson}")]
        public static partial void ErrorTaskMissingArtifacts(this ILogger logger, string taskId, string missingtypesJson);

        [LoggerMessage(EventId = 700015, Level = LogLevel.Warning, Message = "Error Task :{taskId} cant be trigger as it has missing artifacts {artifactName}")]
        public static partial void ErrorFindingArtifactInPrevious(this ILogger logger, string taskId, string artifactName);


    }
}
