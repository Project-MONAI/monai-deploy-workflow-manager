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

namespace Monai.Deploy.WorkflowManager.Common.Configuration
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
        /// Defaults to `md.export.complete`.
        /// </summary>
        [ConfigurationKeyName("exportHL7Complete")]
        public string ExportHL7Complete { get; set; } = "md.export.hl7complete";

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
        /// Defaults to `md.tasks.cancellation`.
        /// </summary>
        [ConfigurationKeyName("taskCancellation")]
        public string TaskCancellationRequest { get; set; } = "md.tasks.cancellation";

        /// <summary>
        /// Gets or sets the topic for publishing clinical review request events.
        /// Defaults to `aide.clinical_review.request`.
        /// </summary>
        [ConfigurationKeyName("aideClinicalReviewRequest")]
        public string AideClinicalReviewRequest { get; set; } = "aide.clinical_review.request";

        /// <summary>
        /// Gets or sets the topic for publishing clinical review cancelation events.
        /// Defaults to `aide.clinical_review.cancellation`.
        /// </summary>
        [ConfigurationKeyName("aideClinicalReviewCancelation")]
        public string AideClinicalReviewCancelation { get; set; } = "aide.clinical_review.cancellation";

        [ConfigurationKeyName("notificationEmailRequest")]
        public string NotificationEmailRequest { get; set; } = "aide.notification_email.request";

        [ConfigurationKeyName("notificationEmailCancelation")]
        public string NotificationEmailCancelation { get; set; } = "aide.notification_email.cancellation";

        [ConfigurationKeyName("artifactrecieved")]
        public string ArtifactRecieved { get; set; } = "md.workflow.artifactrecieved";

        /// <summary>
        /// Gets or sets the topic for publishing export requests.
        /// Defaults to `md_export_request`.
        /// </summary>
        [ConfigurationKeyName("externalAppRequest")]
        public string ExternalAppRequest { get; set; } = "md.externalapp.request";

        /// <summary>
        /// Gets or sets the topic for publishing workflow requests.
        /// Defaults to `md.export.request`.
        /// </summary>
        [ConfigurationKeyName("exportHl7")]
        public string ExportHL7 { get; set; } = "md.export.hl7";


        /// <summary>
        /// Gets or sets the topic for publishing export complete requests.
        /// Defaults to `md_export_complete`.
        /// </summary>
        [ConfigurationKeyName("exportHl7Complete")]
        public string ExportHl7Complete { get; set; } = "md.export.hl7complete";
    }
}
