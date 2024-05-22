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
        [LoggerMessage(EventId = 200000, Level = LogLevel.Error, Message = "Workflow validation errors: {errors}.")]
        public static partial void WorkflowValidationErrors(this ILogger logger, string errors);

        [LoggerMessage(EventId = 200001, Level = LogLevel.Information, Message = "Acknowledged task error.")]
        public static partial void AckowledgedTaskError(this ILogger logger);

        [LoggerMessage(EventId = 200002, Level = LogLevel.Information, Message = "Acknowledged task error.")]
        public static partial void AckowledgedWorkflowInstanceErrors(this ILogger logger);

        [LoggerMessage(EventId = 200003, Level = LogLevel.Information, Message = "Workflow '{name}'({workflowId}) created. ")]
        public static partial void WorkflowCreated(this ILogger logger, string workflowId, string name);

        [LoggerMessage(EventId = 200004, Level = LogLevel.Information, Message = "Workflow '{name}'({workflowId}) updated.")]
        public static partial void WorkflowUpdated(this ILogger logger, string workflowId, string name);

        [LoggerMessage(EventId = 200005, Level = LogLevel.Information, Message = "Workflow '{name}'({workflowId}) revision {workflowRevisionId} deleted.")]
        public static partial void WorkflowDeleted(this ILogger logger, string workflowId, string workflowRevisionId, string name);

        [LoggerMessage(EventId = 200006, Level = LogLevel.Warning, Message = "Unable to locate a matching workflow for the given workflow request.")]
        public static partial void NoMatchingWorkflowFoundForPayload(this ILogger logger);

        [LoggerMessage(EventId = 200007, Level = LogLevel.Warning, Message = "Unable to locate a matching workflow for the given workflow request.")]
        public static partial void FailedToCreateWorkflowInstances(this ILogger logger);

        [LoggerMessage(EventId = 200008, Level = LogLevel.Warning, Message = "Not Processing workflow: {workflowInstanceId} as it already has a status of {status}")]
        public static partial void WorkflowBadStatus(this ILogger logger, string workflowInstanceId, Status status);

        [LoggerMessage(EventId = 200009, Level = LogLevel.Error, Message = "The following task has already been dispatched: {payloadId}, {taskId}")]
        public static partial void TaskPreviouslyDispatched(this ILogger logger, string payloadId, string taskId);

        [LoggerMessage(EventId = 200010, Level = LogLevel.Critical, Message = "Workflow instance `{workflowInstanceId}` not found.")]
        public static partial void WorkflowInstanceNotFound(this ILogger logger, string workflowInstanceId);

        [LoggerMessage(EventId = 200011, Level = LogLevel.Error, Message = "The following task: {taskId} cannot be found in the workflow instance: {workflowInstanceId}.")]
        public static partial void TaskNotFoundInWorkflowInstance(this ILogger logger, string taskId, string workflowInstanceId);

        [LoggerMessage(EventId = 200012, Level = LogLevel.Error, Message = "The following task: {taskId} in workflow {workflowInstanceId} is currently timed out and not processing anymore updates, timed out at {timedOut}.")]
        public static partial void TaskTimedOut(this ILogger logger, string taskId, string workflowInstanceId, DateTime timedOut);

        [LoggerMessage(EventId = 200013, Level = LogLevel.Critical, Message = "Workflow `{workflowId}` not found or is deleted.")]
        public static partial void WorkflowNotFound(this ILogger logger, string workflowId);

        [LoggerMessage(EventId = 200014, Level = LogLevel.Error, Message = "The task execution status for task {taskId} cannot be updated from {oldStatus} to {newStatus}. Payload: {payloadId}")]
        public static partial void TaskStatusUpdateNotValid(this ILogger logger, string payloadId, string taskId, string oldStatus, string newStatus);

        [LoggerMessage(EventId = 200015, Level = LogLevel.Information, Message = "TaskFailed, Task {task}, workflowInstance {workflowInstance}, patientDetails {patientDetails}, correlationId {correlationId}, taskStatus {taskStatus}")]
        public static partial void TaskFailed(this ILogger logger, TaskExecution task, WorkflowInstance workflowInstance, PatientDetails patientDetails, string correlationId, string taskStatus);

        [LoggerMessage(EventId = 200016, Level = LogLevel.Information, Message = "TaskComplete Task {task}, workflowInstance {workflowInstance}, patientDetails {patientDetails}, correlationId {correlationId}, taskStatus {taskStatus}")]
        public static partial void TaskComplete(this ILogger logger, TaskExecution task, WorkflowInstance workflowInstance, PatientDetails patientDetails, string correlationId, string taskStatus);

        [LoggerMessage(EventId = 200017, Level = LogLevel.Error, Message = "No files to export for task {taskId} within workflow instance {workflowInstanceId}.")]
        public static partial void ExportFilesNotFound(this ILogger logger, string taskId, string workflowInstanceId);

        [LoggerMessage(EventId = 200018, Level = LogLevel.Error, Message = "The following task: {taskId} cannot be found in the workflow: {workflowId}. Payload: {payloadId}")]
        public static partial void TaskNotFoundInWorkflow(this ILogger logger, string payloadId, string taskId, string workflowId);

        [LoggerMessage(EventId = 200019, Level = LogLevel.Debug, Message = "Task destination condition for task {taskId} with resolved condition: {resolvedConditional} resolved to false. initial conditional: {conditions}")]
        public static partial void TaskDestinationConditionFalse(this ILogger logger, string resolvedConditional, string conditions, string taskId);

        [LoggerMessage(EventId = 200020, Level = LogLevel.Warning, Message = "Use new ArtifactReceived Queue for continuation messages.")]
        public static partial void DontUseWorkflowReceivedForPayload(this ILogger logger);

        [LoggerMessage(EventId = 200021, Level = LogLevel.Trace, Message = "The task execution status for task {taskId} is already {status}. Payload: {payloadId}")]
        public static partial void TaskStatusUpdateNotNeeded(this ILogger logger, string payloadId, string taskId, string status);

        // Conditions Resolver
        [LoggerMessage(EventId = 210000, Level = LogLevel.Warning, Message = "Failed to parse condition: {condition}. resolvedConditional: {resolvedConditional}")]
        public static partial void FailedToParseCondition(this ILogger logger, string resolvedConditional, string condition, Exception ex);

        [LoggerMessage(EventId = 210001, Level = LogLevel.Debug, Message = "Resolving value: {value}. resolved result: {result}")]
        public static partial void ResolveValue(this ILogger logger, string value, string result);

        [LoggerMessage(EventId = 210002, Level = LogLevel.Debug, Message = "Resolving DICOM value: subValue={subValue}, keyId={keyId}.")]
        public static partial void ResolveDicomValue(this ILogger logger, string subValue, string keyId);

        [LoggerMessage(EventId = 210003, Level = LogLevel.Debug, Message = "Resolving execution tasks: subValue={subValueKey}.")]
        public static partial void ResolveExecutionTask(this ILogger logger, string subValueKey);

        [LoggerMessage(EventId = 210004, Level = LogLevel.Debug, Message = "Resolving workflow: keyValue={keyValue}.")]
        public static partial void ResolveWorkflow(this ILogger logger, string keyValue);

        [LoggerMessage(EventId = 210005, Level = LogLevel.Information, Message = "Conditional resolver parsed {conditions} to {resolvedConditional} with result: {result}.")]
        public static partial void ConditionalParserResult(this ILogger logger, string conditions, string resolvedConditional, string result);

        [LoggerMessage(EventId = 210006, Level = LogLevel.Information, Message = "Attached PatientMetadata {metadata}.")]
        public static partial void AttachedPatientMetadataToTaskExec(this ILogger logger, string metadata);

        [LoggerMessage(EventId = 210007, Level = LogLevel.Information, Message = "Exporting to MIG task Id {taskid}, export destination {destination} number of files {fileCount} Mig data plugins {plugins}.")]
        public static partial void LogMigExport(this ILogger logger, string taskid, string destination, int fileCount, string plugins);

        [LoggerMessage(EventId = 210018, Level = LogLevel.Error, Message = "ExportList or Artifacts are empty! workflowInstanceId {workflowInstanceId} TaskId {taskId}")]
        public static partial void ExportListOrArtifactsAreEmpty(this ILogger logger, string taskId, string workflowInstanceId);

        [LoggerMessage(EventId = 210019, Level = LogLevel.Error, Message = "Task is missing required input artifacts {taskId} Artifacts {ArtifactsJson}")]
        public static partial void TaskIsMissingRequiredInputArtifacts(this ILogger logger, string taskId, string ArtifactsJson);

        [LoggerMessage(EventId = 200020, Level = LogLevel.Warning, Message = "no workflow to execute for the given workflow request.")]
        public static partial void DidntToCreateWorkflowInstances(this ILogger logger);
    }
}
