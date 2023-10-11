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

using Microsoft.Extensions.Logging;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 1000, Level = LogLevel.Debug, Message = "Generating Kubernetes secrets for accessing artifacts: {name}.")]
        public static partial void GeneratingArtifactSecret(this ILogger logger, string name);

        [LoggerMessage(EventId = 1001, Level = LogLevel.Error, Message = "Error deleting Kubernetes secrets {name}.")]
        public static partial void ErrorDeletingKubernetesSecret(this ILogger logger, string name, Exception ex);

        [LoggerMessage(EventId = 1002, Level = LogLevel.Error, Message = "Error creating Argo workflow.")]
        public static partial void ErrorCreatingWorkflow(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 1003, Level = LogLevel.Debug, Message = "Generating Argo workflow template.")]
        public static partial void GeneratingArgoWorkflow(this ILogger logger);

        [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "Argo workflow template generated {generateName}.")]
        public static partial void ArgoWorkflowTemplateGenerated(this ILogger logger, string generateName);

        [LoggerMessage(EventId = 1005, Level = LogLevel.Debug, Message = "Argo workflow template: {generateName}:\r\n{json}")]
        public static partial void ArgoWorkflowTemplateJson(this ILogger logger, string generateName, string json);

        [LoggerMessage(EventId = 1006, Level = LogLevel.Debug, Message = "Creating Argo client with base URL: {baseUrl}.")]
        public static partial void CreatingArgoClient(this ILogger logger, string baseUrl);

        [LoggerMessage(EventId = 1007, Level = LogLevel.Debug, Message = "Creating Kubernetes client: host={hostString}, namespace={ns}.")]
        public static partial void CreatingKubernetesClient(this ILogger logger, string hostString, string ns);

        [LoggerMessage(EventId = 1008, Level = LogLevel.Information, Message = "Argo workflow created: {name}")]
        public static partial void ArgoWorkflowCreated(this ILogger logger, string name);

        [LoggerMessage(EventId = 1009, Level = LogLevel.Debug, Message = "Creating Argo workflow: {generateName}")]
        public static partial void CreatingArgoWorkflow(this ILogger logger, string generateName);

        [LoggerMessage(EventId = 1010, Level = LogLevel.Information, Message = "Argo plugin initialized: namespace={argoNamespace}, base URL={baseUrl}, activeDeadlineSeconds={activeDeadlineSeconds}, apiToken configured={apiTokenSet}. allowInsecure={allowInsecure}")]
        public static partial void Initialized(this ILogger logger, string argoNamespace, string baseUrl, int? activeDeadlineSeconds, string apiTokenSet, string allowInsecure);

        [LoggerMessage(EventId = 1011, Level = LogLevel.Error, Message = "Error generating Argo workflow.")]
        public static partial void ErrorGeneratingWorkflow(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 1012, Level = LogLevel.Error, Message = "Error loading WorkflowTemplate '{workflowTemplateName}' from Argo.")]
        public static partial void ErrorLoadingWorkflowTemplate(this ILogger logger, string workflowTemplateName, Exception ex);

        [LoggerMessage(EventId = 1013, Level = LogLevel.Information, Message = "Using intermediary artifact storage for artifact {artifactName} in template {templateName}.")]
        public static partial void UseIntermediaryArtifactStorage(this ILogger logger, string artifactName, string templateName);

        [LoggerMessage(EventId = 1014, Level = LogLevel.Information, Message = "{artifactName} in template {templateName} is not configured as no matching input information was found.")]
        public static partial void NoInputArtifactStorageConfigured(this ILogger logger, string artifactName, string templateName);

        [LoggerMessage(EventId = 1015, Level = LogLevel.Debug, Message = "Metadata file in {path} in bucket {bucket} was not found.")]
        public static partial void MetadataFileNotFound(this ILogger logger, string bucket, string path);

        [LoggerMessage(EventId = 1016, Level = LogLevel.Information, Message = "Argo: {logString}")]
        public static partial void ArgoLog(this ILogger logger, string logString);

        [LoggerMessage(EventId = 1017, Level = LogLevel.Error, Message = "Error creating Template in Argo.")]
        public static partial void ErrorCreatingWorkflowTemplate(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 1018, Level = LogLevel.Error, Message = "Error deserializing WorkflowTemplateCreateRequest. {message}")]
        public static partial void ErrorDeserializingWorkflowTemplateCreateRequest(this ILogger logger, string message, Exception ex);

        [LoggerMessage(EventId = 1019, Level = LogLevel.Error, Message = "Error deleting Template in Argo.")]
        public static partial void ErrorDeletingWorkflowTemplate(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 1020, Level = LogLevel.Trace, Message = "Calling argo at url {url} : {method} : {stringContent}")]
        public static partial void CallingArgoHttpInfo(this ILogger logger, string url, string method, string stringContent);

        [LoggerMessage(EventId = 1021, Level = LogLevel.Debug, Message = "Exception stopping argo workflow {workflowId}, does it exist?")]
        public static partial void ExecptionStoppingArgoWorkflow(this ILogger logger, string workflowId, Exception ex);

    }
}
