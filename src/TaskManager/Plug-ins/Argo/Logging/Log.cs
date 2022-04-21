// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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

        [LoggerMessage(EventId = 1006, Level = LogLevel.Debug, Message = "Creating Argo client with base URL: {baseUrl}")]
        public static partial void CreatingArgoClient(this ILogger logger, string baseUrl);

        [LoggerMessage(EventId = 1007, Level = LogLevel.Debug, Message = "Creating Kubernetes client.")]
        public static partial void CreatingKubernetesClient(this ILogger logger);

        [LoggerMessage(EventId = 1008, Level = LogLevel.Information, Message = "Argo workflow created: {name}")]
        public static partial void ArgoWorkflowCreated(this ILogger logger, string name);

        [LoggerMessage(EventId = 1009, Level = LogLevel.Debug, Message = "Creating Argo workflow: {generateName}")]
        public static partial void CreatingArgoWorkflow(this ILogger logger, string generateName);

        [LoggerMessage(EventId = 1010, Level = LogLevel.Information, Message = "Argo plugin initialized: namespace={argoNamespace}, base URL={baseUrl}, activeDeadlineSeconds={activeDeadlineSeconds}, apiToken configured={apiTokenSet}.")]
        public static partial void Initialized(this ILogger logger, string argoNamespace, string baseUrl, int? activeDeadlineSeconds, bool apiTokenSet);

        [LoggerMessage(EventId = 1011, Level = LogLevel.Error, Message = "Error generating Argo workflow.")]
        public static partial void ErrorGeneratingWorkflow(this ILogger logger, Exception ex);
    }
}
