// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    internal static class Strings
    {
        public const string ArgoApiVersion = "argoproj.io/v1alpha1";
        public const string KindWorkflow = "Workflow";

        public const string TaskIdLabelSelectorName = "monai-deploy-task-id";
        public const string WorkflowIdLabelSelectorName = "monai-deploy-workflow-id";
        public const string CorrelationIdLabelSelectorName = "monai-deploy-correlation-id";

        public const string WorkflowEntrypoint = "monai-deploy-workflow-entrypoint";

        public const string ExitHook = "exit";
        public const string ExitHookTemplateName = "exit-message-notification";
        public const string ExitHookTemplateTemplateName = "call-exit-hook";

        public const string SecretNamePostfix = "-credentials";
        public const string SecretAccessKey = "accessKey";
        public const string SecretSecretKey = "secretKey";
        public const string SecretTypeOpaque = "Opaque";
    }

}
