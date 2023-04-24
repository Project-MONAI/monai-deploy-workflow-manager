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
        public const string Namespace = "namespace";

        /// <summary>
        /// Key for the endpoint where the Argo server is running.
        /// </summary>
        public const string BaseUrl = "server_url";

        /// <summary>
        /// Key for the endpoint where the Argo server is running.
        /// </summary>
        public const string AllowInsecureseUrl = "allow_insecure";

        /// <summary>
        /// Key for the name of the main 'WorkflowTemplate' stored on the targeted Argo server.
        /// </summary>
        public const string WorkflowTemplateName = "workflow_template_name";

        /// <summary>
        /// Key for the setting 'ActiveDeadlineSeconds' of a Argo workflow.
        /// </summary>
        public const string TimeoutSeconds = "timeoutSeconds";

        /// <summary>
        /// Key for setting the API token to authenticate to the Argo server.
        /// </summary>
        public const string ArgoApiToken = "apiToken";

        /// <summary>
        /// Key for setting the message broker's endpoint.
        /// </summary>
        public const string MessagingEndpoint = "endpoint";

        /// <summary>
        /// Key for setting the user name to access the message broker.
        /// </summary>
        public const string MessagingUsername = "username";

        /// <summary>
        /// Key for setting the password to access the message broker.
        /// </summary>
        public const string MessagingPassword = "password";

        /// <summary>
        /// Key for setting the exchange of the message broker.
        /// </summary>
        public const string MessagingExchange = "exchange";

        /// <summary>
        /// Key for setting the vhost of the message broker.
        /// </summary>
        public const string MessagingVhost = "virtualHost";

        /// <summary>
        /// Key for resource limitations
        /// </summary>
        public const string ArgoResource = "resources";

        /// <summary>
        /// Key for resource limitations
        /// </summary>
        public const string ArgoParameters = "parameters";

        /// <summary>
        /// Key for priority classnames on task plugin arguments side
        /// </summary>
        public const string TaskPriorityClassName = "priority";

        /// <summary>
        /// Key for CPU
        /// </summary>
        public const string Cpu = "cpu";

        /// <summary>
        /// Key for memory allocation
        /// </summary>
        public const string Memory = "memory_gb";

        /// <summary>
        /// Key for GPU
        /// </summary>
        public const string Gpu = "number_gpu";

        /// <summary>
        /// Required arguments to run the Argo workflow.
        /// </summary>
        public static readonly IReadOnlyList<string> RequiredParameters =
            new List<string> {
                WorkflowTemplateName
            };

        /// <summary>
        /// Required settings to run the Argo workflow.
        /// </summary>
        public static readonly IReadOnlyList<string> RequiredSettings =
            new List<string> {
                MessagingEndpoint,
                MessagingUsername,
                MessagingPassword,
                MessagingExchange,
                MessagingVhost
            };
    }
}
