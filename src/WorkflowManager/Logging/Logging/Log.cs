/*
 * Copyright 2021-2022 MONAI Consortium
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

using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Logging.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "{ServiceName} started.")]
        public static partial void ServiceStarted(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "{ServiceName} starting.")]
        public static partial void ServiceStarting(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "{ServiceName} is stopping.")]
        public static partial void ServiceStopping(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Waiting for {ServiceName} to stop.")]
        public static partial void ServiceStopPending(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "{ServiceName} canceled.")]
        public static partial void ServiceCancelled(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "{ServiceName} canceled.")]
        public static partial void ServiceCancelledWithException(this ILogger logger, string serviceName, Exception ex);

        [LoggerMessage(EventId = 7, Level = LogLevel.Warning, Message = "{ServiceName} may be disposed.")]
        public static partial void ServiceDisposed(this ILogger logger, string serviceName, Exception ex);

        [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "{ServiceName} is running.")]
        public static partial void ServiceRunning(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 9, Level = LogLevel.Critical, Message = "Failed to start {ServiceName}.")]
        public static partial void ServiceFailedToStart(this ILogger logger, string serviceName, Exception ex);

        [LoggerMessage(EventId = 10, Level = LogLevel.Critical, Message = "Type '{type}' cannot be found.")]
        public static partial void TypeNotFound(this ILogger logger, string type);

        [LoggerMessage(EventId = 11, Level = LogLevel.Critical, Message = "Instance of '{type}' cannot be found.")]
        public static partial void InstanceOfTypeNotFound(this ILogger logger, string type);

        [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "{ServiceName} subscribed to {RoutingKey} messages.")]
        public static partial void EventSubscription(this ILogger logger, string serviceName, string routingKey);

        [LoggerMessage(EventId = 13, Level = LogLevel.Error, Message = "{message}")]
        public static partial void Exception(this ILogger logger, string message, Exception ex);

        [LoggerMessage(EventId = 14, Level = LogLevel.Error, Message = "The following validaition errors occured: {validationErrors}")]
        public static partial void ValidationErrors(this ILogger logger, string validationErrors);

        [LoggerMessage(EventId = 15, Level = LogLevel.Error, Message = "The following message {messageId} event validation has failed and has been rejected without being requeued.")]
        public static partial void EventRejectedNoQueue(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 16, Level = LogLevel.Error, Message = "The following message {messageId} failed unexpectedly and has been rejected and requeued.")]
        public static partial void EventRejectedRequeue(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 17, Level = LogLevel.Error, Message = "The following transaction {methodName} failed unexpectedly and has been aborted.")]
        public static partial void TransactionFailed(this ILogger logger, string methodName, Exception ex);

        [LoggerMessage(EventId = 18, Level = LogLevel.Error, Message = "The following database call {methodName} failed unexpectedly and has been aborted.")]
        public static partial void DbCallFailed(this ILogger logger, string methodName, Exception ex);

        [LoggerMessage(EventId = 19, Level = LogLevel.Error, Message = "The following task has already been dispatched: {payloadId}, {taskId}")]
        public static partial void TaskPreviouslyDispatched(this ILogger logger, string payloadId, string taskId);

        [LoggerMessage(EventId = 20, Level = LogLevel.Error, Message = "The following task: {taskId} cannot be found in the workflow: {workflowId}. Payload: {payloadId}")]
        public static partial void TaskNotFoundInWorkfow(this ILogger logger, string payloadId, string taskId, string workflowId);

        [LoggerMessage(EventId = 21, Level = LogLevel.Error, Message = "The following task: {taskId} cannot be found in the workflow instance: {workflowInstanceId}.")]
        public static partial void TaskNotFoundInWorkfowInstance(this ILogger logger, string taskId, string workflowInstanceId);

        [LoggerMessage(EventId = 22, Level = LogLevel.Error, Message = "The task execution status for task {taskId} cannot be updated from {oldStatus} to {newStatus}. Payload: {payloadId}")]
        public static partial void TaskStatusUpdateNotValid(this ILogger logger, string payloadId, string taskId, string oldStatus, string newStatus);

        [LoggerMessage(EventId = 23, Level = LogLevel.Error, Message = "The task {taskId} metadata store update failed. Payload: {payloadId} - Metadata: {metadata}")]
        public static partial void TaskMetaDataUpdateFailed(this ILogger logger, string payloadId, string taskId, Dictionary<string, object> metadata);

        [LoggerMessage(EventId = 24, Level = LogLevel.Error, Message = "No files to export for task {taskId} within workflow instance {workflowInstanceId}.")]
        public static partial void ExportFilesNotFound(this ILogger logger, string taskId, string workflowInstanceId);

        [LoggerMessage(EventId = 25, Level = LogLevel.Error, Message = "Failed to get dicom tag {keyId} in bucket {bucketId}. Payload: {payloadId}")]
        public static partial void FailedToGetDicomTag(this ILogger logger, string payloadId, string keyId, string bucketId, Exception ex);

        [LoggerMessage(EventId = 26, Level = LogLevel.Error, Message = "Failed to get patient details in bucket {bucketId}. Payload: {payloadId}")]
        public static partial void FailedToGetPatientDetails(this ILogger logger, string payloadId, string bucketId, Exception ex);

        [LoggerMessage(EventId = 27, Level = LogLevel.Error, Message = "The following task: {taskId} in workflow {workflowInstanceId} is currently timed out and not processing anymore updates, timed out at {timedOut}.")]
        public static partial void TaskTimedOut(this ILogger logger, string taskId, string workflowInstanceId, DateTime timedOut);

        [LoggerMessage(EventId = 28, Level = LogLevel.Error, Message = "The following payload: {payloadId} in workflow instance {workflowInstanceId} workflow revision {workflowRevisionId} for task {taskId} failed to load artifact and was unable to update DB.")]
        public static partial void LoadArtifactAndDBFailiure(this ILogger logger, string payloadId, string taskId, string workflowInstanceId, string workflowRevisionId);

        [LoggerMessage(EventId = 29, Level = LogLevel.Warning, Message = "Not Processing workflow: {workflowInstanceId} as it already has a status of {status}")]
        public static partial void WorkflowBadStatus(this ILogger logger, string workflowInstanceId, string status);

        public static void TaskComplete(this ILogger logger, TaskExecution task, WorkflowInstance workflowInstance, PatientDetails patientDetails, string correlationId, string taskStatus)
        {
            logger.LogInformation("TaskComplete Task {task}, workflowInstance {workflowInstance}, patientDetails {patientDetails}, correlationId {correlationId}, taskStatus {taskStatus}",
              JsonConvert.SerializeObject(task), JsonConvert.SerializeObject(workflowInstance), JsonConvert.SerializeObject(patientDetails), correlationId, taskStatus);
        }

        public static void TaskFailed(this ILogger logger, TaskExecution task, WorkflowInstance workflowInstance, PatientDetails patientDetails, string correlationId, string taskStatus)
        {
            logger.LogInformation("TaskFailed, Task {task}, workflowInstance {workflowInstance}, patientDetails {patientDetails}, correlationId {correlationId}, taskStatus {taskStatus}",
              JsonConvert.SerializeObject(task), JsonConvert.SerializeObject(workflowInstance), JsonConvert.SerializeObject(patientDetails), correlationId, taskStatus);
        }

        public static void LogControllerStartTime(this ILogger logger, ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var body = JsonConvert.SerializeObject(context.ActionArguments.FirstOrDefault());
            logger.LogInformation(29, "ControllerActionStart data  HttpType {httptype}, Path {path}, QueryString {querystring}, Body {body}",
            request.Method, request.Path, request.QueryString.Value.ToString(), body);
        }

        public static void LogControllerEndTime(this ILogger logger, ResultExecutedContext context)
        {
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;

            var startTime = context.HttpContext.Items["startTime"] as DateTime? ?? DateTime.UtcNow;
            var endtime = DateTime.UtcNow;

            var objResult = new ObjectResult("");

            if (context.Result is ObjectResult)
            {
                objResult = (ObjectResult)context.Result;
            }

            logger.LogInformation(30, "ControllerActionEnd data  EndTime {endtime}, Duration {duration}, HttpType {httptype}, Path {path}, QueryString {querystring}, StatusCode {statuscode}, Result {result}",
                endtime, (endtime - startTime).TotalMilliseconds, request.Method, request.Path,
                request.QueryString.Value.ToString(), response.StatusCode, JsonConvert.SerializeObject(objResult));

        }

        [LoggerMessage(EventId = 31, Level = LogLevel.Warning, Message = "Unable to locate a matching workflow for the given workflow request.")]
        public static partial void NoMatchingWorkflowFoundForPayload(this ILogger logger);

        [LoggerMessage(EventId = 32, Level = LogLevel.Debug, Message = "Verifying artifact existence on bucket {bucket}: {artifactKey}={artifactValue}.")]
        public static partial void VerifyArtifactExistence(this ILogger logger, string bucket, string artifactKey, string artifactValue);

        [LoggerMessage(EventId = 33, Level = LogLevel.Debug, Message = "Task destination condition for task {taskId} with condition: {conditions} resolved to false.")]
        public static partial void TaskDestinationConditionFalse(this ILogger logger, string conditions, string taskId);

        public static void LogArtifactPassing(this ILogger logger, Artifact artifact, string path, string artifactType, bool exists)
        {
            logger.LogInformation(34, "Artifact Passed data  Artifact {artifact}, Path {path}, ArtifactType {artifactType}, Exists {exists}",
            JsonConvert.SerializeObject(artifact), path, artifactType, exists);
        }

        [LoggerMessage(EventId = 35, Level = LogLevel.Debug, Message = "Payload already exists for {payloadId}. This is likley due to being requeued")]
        public static partial void PayloadAlreadyExists(this ILogger logger, string payloadId);

        [LoggerMessage(EventId = 36, Level = LogLevel.Debug, Message = "Mandatory output artefacts for task {taskId} are missing.")]
        public static partial void MandatoryOutputArtefactsMissingForTask(this ILogger logger, string taskId);
    }
}
