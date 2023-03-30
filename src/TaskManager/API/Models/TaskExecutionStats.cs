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

using System.ComponentModel.DataAnnotations;
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
        public Guid Id { get; set; }

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
        /// the messageId of the message
        /// </summary>
        [JsonProperty(PropertyName = "message_id")]
        [Required]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the date time that the task started with the plug-in.
        /// </summary>
        [JsonProperty(PropertyName = "started")]
        public DateTime Started { get; set; }

        /// <summary>
        /// Gets or sets the date time that the task started with the plug-in.
        /// </summary>
        [JsonProperty(PropertyName = "lastUpdated")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets the date time that the task started with the plug-in.
        /// </summary>
        [JsonProperty(PropertyName = "completedAt")]
        public DateTime CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets the date time that the task started with the plug-in.
        /// </summary>
        [JsonProperty(PropertyName = "executionTimeSeconds")]
        public double ExecutionTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the date time that the task started with the plug-in.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; } = TaskStatus.Created.ToString();

        public TaskExecutionStats(TaskDispatchEventInfo dispatchInfo)
        {
            CorrelationId = dispatchInfo.Event.CorrelationId;
            WorkflowInstanceId = dispatchInfo.Event.WorkflowInstanceId;
            ExecutionId = dispatchInfo.Event.ExecutionId;
            TaskId = dispatchInfo.Event.TaskId;
            Started = dispatchInfo.Started;
            Status = dispatchInfo.Event.Status.ToString();
        }

        public TaskExecutionStats(TaskUpdateEvent taskUpdateEvent)
        {
            CorrelationId = taskUpdateEvent.CorrelationId;
            WorkflowInstanceId = taskUpdateEvent.WorkflowInstanceId;
            ExecutionId = taskUpdateEvent.ExecutionId;
            TaskId = taskUpdateEvent.TaskId;
            Status = taskUpdateEvent.Status.ToString();
        }
    }
}
