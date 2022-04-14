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
        public static readonly string WorkflowTemplateTemplateName = "workflowTemplateTemplateName";

        /// <summary>
        /// Key for the name of the exit 'WorkflowTemplate' stored on the targeted Argo server.
        /// </summary>
        public static readonly string ExitWorkflowTemplateName = "workflowTemplateName";

        /// <summary>
        /// Key for the name of the exit 'template' inside the 'WorkflowTemplate' stored on the targeted Argo server.
        /// </summary>
        public static readonly string ExitWorkflowTemplateTemplateName = "workflowTemplateTemplateName";

        /// <summary>
        /// Key for the setting 'ActiveDeadlineSeconds' of a Argo workflow.
        /// </summary>
        public static readonly string TimeoutSeconds = "timeoutSeconds";

        /// <summary>
        /// Required arguments to run the Argo workflow.
        /// </summary>
        public static readonly IReadOnlyList<string> RequiredParameters = new List<string> { BaseUrl, WorkflowTemplateName, WorkflowTemplateName, WorkflowTemplateTemplateName, ExitWorkflowTemplateName, ExitWorkflowTemplateTemplateName };
    }
}
