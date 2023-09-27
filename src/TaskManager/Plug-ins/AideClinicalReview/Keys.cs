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

namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview
{
    public class Keys
    {

        /// <summary>
        /// Key for the workflow name.
        /// </summary>
        public static readonly string WorkflowName = "workflow_name";

        /// <summary>
        /// Key for the reviewed task id.
        /// </summary>
        public static readonly string ReviewedTaskId = "reviewed_task_id";

        /// <summary>
        /// Key for the reviewed execution id.
        /// </summary>
        public static readonly string ReviewedExecutionId = "reviewed_execution_id";

        /// <summary>
        /// Key for the application name.
        /// </summary>
        public static readonly string ApplicationName = "application_name";

        /// <summary>
        /// Key for the application version.
        /// </summary>
        public static readonly string ApplicationVersion = "application_version";

        /// <summary>
        /// Key for the mode.
        /// </summary>
        public static readonly string Mode = "mode";

        /// <summary>
        /// Key for the reviewer roles.
        /// </summary>
        public static readonly string ReviewerRoles = "reviewer_roles";

        /// <summary>
        /// Key for the workflow name.
        /// </summary>
        public static readonly string Notifications = "notifications";

        /// <summary>
        /// Key for the queue name to send the clinical review message.
        /// </summary>
        public static readonly string QueueName = "queue_name";

        /// <summary>
        /// Key for the acceptance.
        /// </summary>
        public static readonly string MetadataAcceptance = "acceptance";

        /// <summary>
        /// Key for the reject reason.
        /// </summary>
        public static readonly string MetadataReason = "reason";

        /// <summary>
        /// Key for the message.
        /// </summary>
        public static readonly string MetadataMessage = "message";

        /// <summary>
        /// Key for the user ID.
        /// </summary>
        public static readonly string MetadataUserId = "user_id";

        /// <summary>
        /// Required arguments to run the Argo workflow.
        /// </summary>
        public static readonly IReadOnlyList<string> RequiredParameters =
            new List<string> {
                ReviewedTaskId,
                ApplicationName,
                ApplicationVersion,
                Mode
            };
    }
}
