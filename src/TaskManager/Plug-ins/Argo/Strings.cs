/*
 * Copyright 2023 MONAI Consortium
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

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    public static class Strings
    {
        public const string ArgoApiVersion = "argoproj.io/v1alpha1";
        public const string DefaultNamespace = "argo";
        public const string KindWorkflow = "Workflow";

        public const string TaskIdLabelSelectorName = "md-task-id";
        public const string WorkflowIdLabelSelectorName = "md-workflow-id";
        public const string CorrelationIdLabelSelectorName = "md-correlation-id";
        public const string ExecutionIdLabelSelectorName = "md-execution-id";

        public const string WorkflowEntrypoint = "md-workflow-entrypoint";
        public const string WorkflowTemplatePrefix = "call-";

        public const string ExitHook = "exit";
        public const string ExitHookTemplateName = "exit-message-template";
        public const string ExitHookTemplateSendTemplateName = "send-message";
        public const string ExitHookParameterEvent = "event";
        public const string ExitHookOutputArtifactName = "output";
#pragma warning disable S5443 // public directory /tmp/ is used in Docker container.
        public const string ExitHookOutputPath = "/tmp/";
#pragma warning restore S5443 // public directory /tmp/ is used in Docker container

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

        public const string IdentityKey = "IdentityKey";

        public static readonly IList<string> ArgoFailurePhases = new List<string> { ArgoPhaseFailed, ArgoPhaseError };
    }
}
