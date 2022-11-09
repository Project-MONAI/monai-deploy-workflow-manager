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

namespace Monai.Deploy.WorkflowManager.Shared
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
        /// Required arguments to run the clinical review task workflow args.
        /// </summary>
        public static readonly IReadOnlyList<string> ClinicalReviewRequiredParameters = new List<string> {
            QueueName,
            WorkflowName,
            ReviewedTaskId,
            ApplicationVersion,
            ApplicationName,
            Mode
        };

        /// <summary>
        /// Key for the endpoint where the Argo server is running.
        /// </summary>
        public static readonly string BaseUrl = "server_url";

        /// <summary>
        /// Key for the name of the main 'WorkflowTemplate' stored on the targeted Argo server.
        /// </summary>
        public static readonly string WorkflowTemplateName = "workflow_template_name";

        /// <summary>
        /// Required arguments to run the Argo task args.
        /// </summary>
        public static readonly IReadOnlyList<string> ArgoRequiredParameters =
            new List<string> {
                BaseUrl,
                WorkflowTemplateName
            };


        /// <summary>
        /// Key for the argo task type.
        /// </summary>
        public static readonly string ArgoTaskType = "argo";

        /// <summary>
        /// Key for the clinical review task type.
        /// </summary>
        public static readonly string ClinicalReviewTaskType = "aide_clinical_review";

        /// <summary>
        /// Key for the router task type.
        /// </summary>
        public static readonly string RouterTaskType = "router";

        /// <summary>
        /// Key for the export task type.
        /// </summary>
        public static readonly string ExportTaskType = "export";

        /// <summary>
        /// Key for the export task type.
        /// </summary>
        public static readonly string DockerTaskType = "docker";

        /// <summary>
        /// Valid task types.
        /// </summary>
        public static readonly IReadOnlyList<string> ValidTaskTypes =
            new List<string> {
                ArgoTaskType,
                ClinicalReviewTaskType,
                RouterTaskType,
                ExportTaskType,
                DockerTaskType
            };
    }
}
