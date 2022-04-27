// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    internal static class Strings
    {
        public const string ArgoApiVersion = "argoproj.io/v1alpha1";
        public const string DefaultNamespace = "default";
        public const string KindWorkflow = "Workflow";

        public const string TaskIdLabelSelectorName = "md-task-id";
        public const string WorkflowIdLabelSelectorName = "md-workflow-id";
        public const string CorrelationIdLabelSelectorName = "md-correlation-id";
        public const string ExecutionIdLabelSelectorName = "md-execution-id";

        public const string WorkflowEntrypoint = "md-workflow-entrypoint";
        public const string WorkflowTemplatePrefix = "call-";

        public const string ExitHook = "exit";
        public const string ExitHookTemplateName = "exit-message-template";
        public const string ExitHookTemplateGenerateTemplateName = "generate-message";
        public const string ExitHookTemplateSendTemplateName = "send-message";
        public const string ExitHookParameterEvent = "event";
        public const string ExitHookParameterMessage = "message";
        public const string ExitHookGenerateMessageContainerImage = "alpine:latest";
        public const string ExitHookOutputArtifactName = "output";
        public const string ExitHookOutputPath = "/tmp/";
        public const string ExitHookOutputStorageName = "tempStorage";
        public const string ExitHookSendMessageContainerImage = "ghcr.io/jandelgado/rabtap:latest";

        public const string SecretAccessKey = "accessKey";
        public const string SecretSecretKey = "secretKey";
        public const string SecretTypeOpaque = "Opaque";

        public const string ArgoPhaseSucceeded = "Succeeded";
        public const string ArgoPhaseFailed = "Failed";
        public const string ArgoPhaseError = "Error";
        public const string ArgoPhaseSkipped = "Skipped";
        public const string ArgoPhaseRunning = "Running";
        public const string ArgoPhasePending = "Pending";

        public const string LabelCreator = "creator";
        public const string LabelCreatorValue = "monai-deploy";

        public const string ContentTypeJson = "application/json";
        public const string ApplicationId = "Argo";

        public static readonly IList<string> ArgoFailurePhases = new List<string> { ArgoPhaseFailed, ArgoPhaseError };
    }
}
