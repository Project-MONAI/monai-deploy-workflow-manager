/*
 * Copyright 2023 MONAI Consortium
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

using System;
using System.ComponentModel.DataAnnotations;
using Monai.Deploy.WorkflowManager.Common.Contracts.Migrations;
using Monai.Deploy.Messaging.Events;
using Mongo.Migration.Documents;
using Mongo.Migration.Documents.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Common.Contracts.Models
{
    [CollectionLocation("ExecutionStats"), RuntimeVersion("1.0.3")]
    public class ExecutionStats : IDocument
    {
        /// <summary>
        /// Gets or sets the ID of the object.
        /// </summary>
        [BsonId]
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets Db version.
        /// </summary>
        [JsonConverter(typeof(DocumentVersionConvert)), BsonSerializer(typeof(DocumentVersionConverBson))]
        public DocumentVersion Version { get; set; } = new DocumentVersion(1, 0, 2);

        /// <summary>
        /// the correlationId of the event
        /// </summary>
        [JsonProperty(PropertyName = "correlation_id")]
        [Required]
        public string CorrelationId { get; set; } = "";

        /// <summary>
        /// The id of the workflow
        /// </summary>
        [JsonProperty(PropertyName = "workflow_id")]
        [Required]
        public string WorkflowId { get; set; } = "";

        /// <summary>
        /// the workflow Instance that triggered the event
        /// </summary>
        [JsonProperty(PropertyName = "workflow_instance_id")]
        [Required]
        public string WorkflowInstanceId { get; set; } = "";

        /// <summary>
        /// This execution ID
        /// </summary>
        [JsonProperty(PropertyName = "execution_id")]
        [Required]
        public string ExecutionId { get; set; } = "";

        /// <summary>
        /// The event Task ID
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "task_id")]
        public string TaskId { get; set; } = "";

        /// <summary>
        /// Gets or sets the date time that the task started with the plug-in.
        /// </summary>
        [JsonProperty(PropertyName = "startedUTC")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime StartedUTC { get; set; }

        /// <summary>
        /// Gets or sets the date time that the task last updated.
        /// </summary>
        [JsonProperty(PropertyName = "lastUpdatedUTC")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastUpdatedUTC { get; set; }

        /// <summary>
        /// Gets or sets the date time that the task completed.
        /// </summary>
        [JsonProperty(PropertyName = "completedAtUTC")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CompletedAtUTC { get; set; }

        /// <summary>
        /// Gets or sets the duration of time actually executing in Argo, calculated from the metadata.
        /// </summary>
        [JsonProperty(PropertyName = "executionTimeSeconds")]
        public double ExecutionTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; } = TaskExecutionStatus.Created.ToString();

        /// <summary>
        /// Gets or sets the failure reason.
        /// </summary>
        [JsonProperty(PropertyName = "reason")]
        public FailureReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the duration, difference between startedAt and CompletedAt time.
        /// </summary>
        [JsonProperty(PropertyName = "durationSeconds")]
        public double DurationSeconds
        {
            get; set;
        }

        public ExecutionStats()
        {

        }

        public ExecutionStats(TaskExecution execution, string workflowId, string correlationId)
        {
            ArgumentNullException.ThrowIfNull(execution, "dispatchInfo");
            CorrelationId = correlationId;
            WorkflowInstanceId = execution.WorkflowInstanceId;
            ExecutionId = execution.ExecutionId;
            TaskId = execution.TaskId;
            StartedUTC = execution.TaskStartTime.ToUniversalTime();
            Status = execution.Status.ToString();
            WorkflowId = workflowId;
            Reason = execution.Reason;
        }

        public ExecutionStats(TaskUpdateEvent taskUpdateEvent, string workflowId)
        {
            ArgumentNullException.ThrowIfNull(taskUpdateEvent, "taskUpdateEvent");
            CorrelationId = taskUpdateEvent.CorrelationId;
            WorkflowInstanceId = taskUpdateEvent.WorkflowInstanceId;
            ExecutionId = taskUpdateEvent.ExecutionId;
            TaskId = taskUpdateEvent.TaskId;
            Status = taskUpdateEvent.Status.ToString();
            WorkflowId = workflowId;
            Reason = taskUpdateEvent.Reason;
        }

        public ExecutionStats(TaskCancellationEvent taskCanceledEvent, string workflowId, string correlationId)
        {
            ArgumentNullException.ThrowIfNull(taskCanceledEvent, "taskCanceledEvent");
            CorrelationId = correlationId;
            WorkflowInstanceId = taskCanceledEvent.WorkflowInstanceId;
            ExecutionId = taskCanceledEvent.ExecutionId;
            TaskId = taskCanceledEvent.TaskId;
            Status = TaskExecutionStatus.Failed.ToString();
            WorkflowId = workflowId;
            Reason = taskCanceledEvent.Reason;
        }
    }
}
