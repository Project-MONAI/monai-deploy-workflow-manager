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

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous
{
    public static class ValidationConstants
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
        /// Key for the queue name to send the clinical review message.
        /// </summary>
        public static readonly string QueueName = "queue_name";

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
        /// Key for the notifications.
        /// </summary>
        public static readonly string Notifications = "notifications";

        /// <summary>
        /// Key for recipient emails.
        /// </summary>
        public static readonly string RecipientEmails = "recipient_emails";

        /// <summary>
        /// Key for recipient roles.
        /// </summary>
        public static readonly string RecipientRoles = "recipient_roles";

        /// <summary>
        /// Key for the metadata values.
        /// </summary>
        public static readonly string MetadataValues = "metadata_values";

        public enum ModeValues
        {
            QA,
            Research,
            Clinical
        }

        public enum NotificationValues
        {
            True,
            False
        }

        /// <summary>
        /// Required arguments to run the clinical review task workflow args.
        /// </summary>
        public static readonly IReadOnlyList<string> ClinicalReviewRequiredParameters = new List<string> {
            WorkflowName,
            ReviewedTaskId,
            ApplicationVersion,
            ApplicationName,
            Mode
        };


        /// <summary>
        /// Key for the argo task type.
        /// </summary>
        public const string ArgoTaskType = "argo";

        /// <summary>
        /// Key for the clinical review task type.
        /// </summary>
        public const string ClinicalReviewTaskType = "aide_clinical_review";

        /// <summary>
        /// Key for the router task type.
        /// </summary>
        public const string RouterTaskType = "router";

        /// <summary>
        /// Key for the export task type.
        /// </summary>
        public const string ExportTaskType = "export";

        /// <summary>
        /// Key for the export task type.
        /// </summary>
        public const string ExternalAppTaskType = "remote_app_execution";

        /// <summary>
        /// Key for Hl7 export task type.
        /// </summary>
        public const string HL7ExportTask = "export_hl7";

        /// <summary>
        /// Key for the export task type.
        /// </summary>
        public const string DockerTaskType = "docker";

        /// <summary>
        /// Key for the email task type.
        /// </summary>
        public const string Email = "email";

        public static readonly string[] AcceptableTasksToReview = { ArgoTaskType, ExternalAppTaskType };

        /// <summary>
        /// Valid task types.
        /// </summary>
        public static readonly IReadOnlyList<string> ValidTaskTypes =
            new List<string> {
                ArgoTaskType,
                ClinicalReviewTaskType,
                RouterTaskType,
                ExportTaskType,
                DockerTaskType,
                Email,
                ExternalAppTaskType,
                HL7ExportTask
            };
    }
}
