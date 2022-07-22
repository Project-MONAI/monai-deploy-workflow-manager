﻿// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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
