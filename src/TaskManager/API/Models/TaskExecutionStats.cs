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

using System.ComponentModel.DataAnnotations;
using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.Migrations;
using Mongo.Migration.Documents;
using Mongo.Migration.Documents.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.API.Models
{
    [CollectionLocation("ExecutionStats"), RuntimeVersion("1.0.0")]
    public class TaskExecutionStats : IDocument
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
        public DocumentVersion Version { get; set; } = new DocumentVersion(1, 0, 0);

        /// <summary>
        /// the correlationId of the event
        /// </summary>
        [JsonProperty(PropertyName = "correlation_id")]
        [Required]
        public string CorrelationId { get; set; }

        /// <summary>
        /// the workflow Instance that triggered the event
        /// </summary>
        [JsonProperty(PropertyName = "workflow_instance_id")]
        [Required]
        public string WorkflowInstanceId { get; set; }

        /// <summary>
        /// This execution ID
        /// </summary>
        [JsonProperty(PropertyName = "execution_id")]
        [Required]
        public string ExecutionId { get; set; }

        /// <summary>
        /// The event Task ID
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "task_id")]
        public string TaskId { get; set; }

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
        /// Gets or sets the duration, difference between startedAt and CompletedAt time.
        /// </summary>
        [JsonProperty(PropertyName = "durationSeconds")]
        public double DurationSeconds
        {
            get; set;
        }

        public TaskExecutionStats()
        {

        }

        public TaskExecutionStats(TaskDispatchEventInfo dispatchInfo)
        {
            Guard.Against.Null(dispatchInfo, "dispatchInfo");
            CorrelationId = dispatchInfo.Event.CorrelationId;
            WorkflowInstanceId = dispatchInfo.Event.WorkflowInstanceId;
            ExecutionId = dispatchInfo.Event.ExecutionId;
            TaskId = dispatchInfo.Event.TaskId;
            StartedUTC = dispatchInfo.Started.ToUniversalTime();
            Status = dispatchInfo.Event.Status.ToString();
        }

        public TaskExecutionStats(TaskUpdateEvent taskUpdateEvent)
        {
            Guard.Against.Null(taskUpdateEvent, "taskUpdateEvent");
            CorrelationId = taskUpdateEvent.CorrelationId;
            WorkflowInstanceId = taskUpdateEvent.WorkflowInstanceId;
            ExecutionId = taskUpdateEvent.ExecutionId;
            TaskId = taskUpdateEvent.TaskId;
            Status = taskUpdateEvent.Status.ToString();
        }
    }
}