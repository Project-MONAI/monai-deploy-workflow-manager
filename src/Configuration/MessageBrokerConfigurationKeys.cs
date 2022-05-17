// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Newtonsoft.Json;

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Configuration
{
    public class MessageBrokerConfigurationKeys
    {
        /// <summary>
        /// Gets or sets the topic for publishing workflow requests.
        /// Defaults to `md.workflow.request`.
        /// </summary>
        [ConfigurationKeyName("workflowRequest")]
        public string WorkflowRequest { get; set; } = "md.workflow.request";

        /// <summary>
        /// Gets or sets the topic for publishing workflow requests.
        /// Defaults to `md.export.complete`.
        /// </summary>
        [ConfigurationKeyName("exportComplete")]
        public string ExportComplete { get; set; } = "md.export.complete";

        /// <summary>
        /// Gets or sets the topic for publishing workflow requests.
        /// Defaults to `md.export.request`.
        /// </summary>
        [ConfigurationKeyName("exportRequestPrefix")]
        public string ExportRequestPrefix { get; set; } = "md.export.request";

        /// <summary>
        /// Gets or sets the topic for publishing task dispatch events.
        /// Defaults to `md.tasks.dispatch`.
        /// </summary>
        [ConfigurationKeyName("taskDispatch")]
        public string TaskDispatchRequest { get; set; } = "md.tasks.dispatch";

        /// <summary>
        /// Gets or sets the topic for publishing task callback events.
        /// Defaults to `md.tasks.callback`.
        /// </summary>
        [ConfigurationKeyName("taskCallback")]
        public string TaskCallbackRequest { get; set; } = "md.tasks.callback";

        /// <summary>
        /// Gets or sets the topic for publishing task update events.
        /// Defaults to `md.tasks.update`.
        /// </summary>
        [ConfigurationKeyName("taskUpdate")]
        public string TaskUpdateRequest { get; set; } = "md.tasks.update";
    }
}
