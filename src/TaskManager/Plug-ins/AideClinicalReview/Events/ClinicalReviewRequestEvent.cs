using System.ComponentModel.DataAnnotations;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Models;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Events
{
    public class ClinicalReviewRequestEvent : EventBase
    {
        /// <summary>
        /// Gets or sets the execution ID representing the instance of the task.
        /// </summary>
        [JsonProperty(PropertyName = "execution_id")]
        [Required]
        public string ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the ID representing the instance of the Task.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "task_id")]
        public string TaskId { get; set; }

        /// <summary>
        /// Gets or sets the correlation ID.
        /// </summary>
        [JsonProperty(PropertyName = "correlation_id")]
        [Required]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow.
        /// </summary>
        [JsonProperty(PropertyName = "workflow_name")]
        [Required]
        public string WorkflowName { get; set; }

        /// <summary>
        /// Gets or sets patient metadata.
        /// </summary>
        [JsonProperty(PropertyName = "patient_metadata")]
        public PatientMetadata PatientMetadata { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow.
        /// </summary>
        [JsonProperty(PropertyName = "files")]
        public List<Messaging.Common.Storage> Files { get; set; }
    }
}
