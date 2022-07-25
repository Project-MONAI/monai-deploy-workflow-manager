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

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.StaticValues
{
    internal static class Keys
    {
        /// <summary>
        /// Key for the namespace where the Argo workflows are stored and executed.
        /// </summary>
        public static readonly string Namespace = "namespace";

        /// <summary>
        /// Key for the endpoint where the Argo server is running.
        /// </summary>
        public static readonly string BaseUrl = "server_url";

        /// <summary>
        /// Key for the endpoint where the Argo server is running.
        /// </summary>
        public static readonly string AllowInsecureseUrl = "allow_insecure";

        /// <summary>
        /// Key for the name of the main 'WorkflowTemplate' stored on the targeted Argo server.
        /// </summary>
        public static readonly string WorkflowTemplateName = "workflow_id";

        /// <summary>
        /// Key for the setting 'ActiveDeadlineSeconds' of a Argo workflow.
        /// </summary>
        public static readonly string TimeoutSeconds = "timeoutSeconds";

        /// <summary>
        /// Key for setting the API token to authenticate to the Argo server.
        /// </summary>
        public static readonly string ArgoApiToken = "apiToken";

        /// <summary>
        /// Key for setting the message broker's endpoint.
        /// </summary>
        public static readonly string MessagingEnddpoint = "messaging_endpoint";

        /// <summary>
        /// Key for setting the user name to access the message broker.
        /// </summary>
        public static readonly string MessagingUsername = "messaging_username";

        /// <summary>
        /// Key for setting the password to access the message broker.
        /// </summary>
        public static readonly string MessagingPassword = "messaging_password";

        /// <summary>
        /// Key for setting the topic of the completion event.
        /// </summary>
        public static readonly string MessagingTopic = "messaging_topic";

        /// <summary>
        /// Key for setting the exchange of the message broker.
        /// </summary>
        public static readonly string MessagingExchange = "messaging_exchange";

        /// <summary>
        /// Key for setting the vhost of the message broker.
        /// </summary>
        public static readonly string MessagingVhost = "messaging_vhost";

        /// <summary>
        /// Key for resource limitations
        /// </summary>
        public static readonly string ArgoResource = "resources";

        /// <summary>
        /// Key for resource limitations
        /// </summary>
        public static readonly string ArgoParameters = "parameters";

        /// <summary>
        /// Key for priority classnames on task plugin arguments side
        /// </summary>
        public static readonly string TaskPriorityClassName = "priorityClass";

        /// <summary>
        /// Required arguments to run the Argo workflow.
        /// </summary>
        public static readonly IReadOnlyList<string> RequiredParameters =
            new List<string> {
                BaseUrl,
                WorkflowTemplateName,
                MessagingEnddpoint,
                MessagingUsername,
                MessagingPassword,
                MessagingTopic,
                MessagingExchange,
            };
    }
}
