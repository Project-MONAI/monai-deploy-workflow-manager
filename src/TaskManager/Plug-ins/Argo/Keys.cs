// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
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
        public static readonly string BaseUrl = "baseUrl";

        /// <summary>
        /// Key for the name of the main 'WorkflowTemplate' stored on the targeted Argo server.
        /// </summary>
        public static readonly string WorkflowTemplateName = "workflowTemplateName";

        /// <summary>
        /// Key for the name of the main 'template' inside the 'WorkflowTemplate' stored on the targeted Argo server.
        /// </summary>
        public static readonly string WorkflowTemplateEntrypoint = "workflowTemplateEntrypoint";

        /// <summary>
        /// Key for the setting 'ActiveDeadlineSeconds' of a Argo workflow.
        /// </summary>
        public static readonly string TimeoutSeconds = "timeoutSeconds";

        /// <summary>
        /// Key for the setting API token to authenticate to the Argo server.
        /// </summary>
        public static readonly string ArgoApiToken = "apiToken";

        /// <summary>
        /// Key for the setting the message broker's endpoint.
        /// </summary>
        public static readonly string MessagingEnddpoint = "messagingEndpoint";

        /// <summary>
        /// Key for the setting the user name to access the message broker.
        /// </summary>
        public static readonly string MessagingUsername = "messagingUsername";

        /// <summary>
        /// Key for the setting the password to access the message broker.
        /// </summary>
        public static readonly string MessagingPassword = "messagingPassword";

        /// <summary>
        /// Key for the setting the topic of the completion event.
        /// </summary>
        public static readonly string MessagingTopic = "messagingTopic";

        /// <summary>
        /// Key for the setting the exchange of the message broker.
        /// </summary>
        public static readonly string MessagingExchange = "messagingExchange";

        /// <summary>
        /// Required arguments to run the Argo workflow.
        /// </summary>
        public static readonly IReadOnlyList<string> RequiredParameters =
            new List<string> {
                BaseUrl,
                WorkflowTemplateName,
                WorkflowTemplateEntrypoint,
                MessagingEnddpoint,
                MessagingUsername,
                MessagingPassword,
                MessagingTopic,
                MessagingExchange
            };
    }
}
