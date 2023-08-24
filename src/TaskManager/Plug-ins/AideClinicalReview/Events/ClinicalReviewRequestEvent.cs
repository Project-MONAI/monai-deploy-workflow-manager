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
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Models;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Events
{
    public class ClinicalReviewRequestEvent : EventBase
    {
        /// <summary>
        /// Gets or sets the workflow instance ID.
        /// </summary>
        [JsonProperty(PropertyName = "workflow_instance_id")]
        [Required]
        public string WorkflowInstanceId { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the execution ID representing the instance of the task.
        /// </summary>
        [JsonProperty(PropertyName = "execution_id")]
        [Required]
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID representing the instance of the Task.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "task_id")]
        public string TaskId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the reviewed task ID.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "reviewed_task_id")]
        public string ReviewedTaskId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the reviewed execution ID.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "reviewed_execution_id")]
        public string ReviewedExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the correlation ID.
        /// </summary>
        [JsonProperty(PropertyName = "correlation_id")]
        [Required]
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the workflow.
        /// </summary>
        [JsonProperty(PropertyName = "workflow_name")]
        [Required]
        public string WorkflowName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the workflow.
        /// </summary>
        [JsonProperty(PropertyName = "notifications")]
        [Required]
        public bool Notifications { get; set; }

        /// <summary>
        /// Gets or sets patient metadata.
        /// </summary>
        [JsonProperty(PropertyName = "patient_metadata")]
        public PatientMetadata? PatientMetadata { get; set; }

        /// <summary>
        /// Gets or sets the reviewer roles.
        /// </summary>
        [JsonProperty(PropertyName = "reviewer_roles")]
        public string[] ReviewerRoles { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the application metadata.
        /// </summary>
        [JsonProperty(PropertyName = "application_metadata")]
        public Dictionary<string, string> ApplicationMetadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the name of the workflow.
        /// </summary>
        [JsonProperty(PropertyName = "files")]
        public List<Messaging.Common.Storage> Files { get; set; } = new();
    }
}
