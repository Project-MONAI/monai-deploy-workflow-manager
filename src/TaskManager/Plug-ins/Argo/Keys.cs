// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    internal static class Keys
    {
        /// <summary>
        /// Key for the namespace where the Argo workflows are stored and executed.
        /// </summary>
        public static readonly string Namespace = "Namespace";
        /// <summary>
        /// Key for the name of the main 'WorkflowTemplate' stored on the targeted Argo server.
        /// </summary>
        public static readonly string WorkflowTemplateName = "WorkflowTemplateName";
        /// <summary>
        /// Key for the name of the main 'template' inside the 'WorkflowTemplate' stored on the targeted Argo server.
        /// </summary>
        public static readonly string WorkflowTemplateTemplateName = "WorkflowTemplateTemplateName";

        /// <summary>
        /// Key for the name of the exit 'WorkflowTemplate' stored on the targeted Argo server.
        /// </summary>
        public static readonly string ExitWorkflowTemplateName = "WorkflowTemplateName";
        /// <summary>
        /// Key for the name of the exit 'template' inside the 'WorkflowTemplate' stored on the targeted Argo server.
        /// </summary>
        public static readonly string ExitWorkflowTemplateTemplateName = "WorkflowTemplateTemplateName";

        /// <summary>
        /// Key for the setting 'ActiveDeadlineSeconds' of a Argo workflow.
        /// </summary>
        public static readonly string TimeoutSeconds = "TimeoutSeconds";

        /// <summary>
        /// Required arguments to run the Argo workflow.
        /// </summary>
        public static readonly IReadOnlyList<string> RequiredParameters = new List<string> { WorkflowTemplateName, WorkflowTemplateName, WorkflowTemplateTemplateName, ExitWorkflowTemplateName, ExitWorkflowTemplateTemplateName };
    }
}
