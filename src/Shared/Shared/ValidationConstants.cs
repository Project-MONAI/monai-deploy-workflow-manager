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
    public class ValidationConstants
    {
        /// <summary>
        /// Key for the workflow name.
        /// </summary>
        public const string ArgoWorkflowName = "workflow_name";

        /// <summary>
        /// Key for the reviewed task id.
        /// </summary>
        public const string ArgoReviewedTaskId = "reviewed_task_id";

        /// <summary>
        /// Key for the queue name to send the clinical review message.
        /// </summary>
        public const string ArgoQueueName = "queue_name";

        /// <summary>
        /// Required arguments to run the Argo workflow.
        /// </summary>
        public static readonly IReadOnlyList<string> ArgoRequiredParameters = new List<string> {
            ArgoQueueName,
            ArgoWorkflowName,
            ArgoReviewedTaskId
        };
    }
}
