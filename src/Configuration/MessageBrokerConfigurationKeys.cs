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

        /// <summary>
        /// Gets or sets the topic for publishing task update events.
        /// Defaults to `md.tasks.update`.
        /// </summary>
        [ConfigurationKeyName("taskUpdate")]
        public string TaskCancellationRequest { get; set; } = "md.tasks.cancellation";
    }
}
