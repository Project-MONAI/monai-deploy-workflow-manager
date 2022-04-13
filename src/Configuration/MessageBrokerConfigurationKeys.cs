using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Configuration
{
    public class MessageBrokerConfigurationKeys
    {
        /// <summary>
        /// Gets or sets the topic for publishing workflow requests.
        /// Defaults to `md_workflow_request`.
        /// </summary>
        [JsonProperty(PropertyName = "workflowRequest")]
        public string WorkflowRequest { get; set; } = "md.workflow.request";

        /// <summary>
        /// Gets or sets the topic for publishing workflow requests.
        /// Defaults to `md_workflow_request`.
        /// </summary>
        [JsonProperty(PropertyName = "exportComplete")]
        public string ExportComplete { get; set; } = "md.export.complete";

        /// <summary>
        /// Gets or sets the topic for publishing workflow requests.
        /// Defaults to `md_workflow_request`.
        /// </summary>
        [JsonProperty(PropertyName = "exportRequestPrefix")]
        public string ExportRequestPrefix { get; set; } = "md.export.request";
    }
}
