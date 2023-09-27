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

using Argo;


namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    public interface IArgoClient
    {
        Task<Workflow> Argo_CreateWorkflowAsync(string argoNamespace, WorkflowCreateRequest body, CancellationToken cancellationToken);

        Task<Workflow?> Argo_GetWorkflowAsync(string argoNamespace, string name, string? getOptionsResourceVersion, string? fields, CancellationToken cancellationToken);

        Task<WorkflowTemplate?> Argo_GetWorkflowTemplateAsync(string argoNamespace, string name, string? getOptionsResourceVersion);

        Task<Workflow> Argo_StopWorkflowAsync(string argoNamespace, string name, WorkflowStopRequest body);

        Task<Workflow> Argo_TerminateWorkflowAsync(string argoNamespace, string name, WorkflowTerminateRequest body);

        Task<Version?> Argo_GetVersionAsync();

        Task<string?> Argo_Get_WorkflowLogsAsync(string argoNamespace, string name, string? podName, string logOptionsContainer);

        Task<WorkflowTemplate> Argo_CreateWorkflowTemplateAsync(string argoNamespace, WorkflowTemplateCreateRequest body, CancellationToken cancellationToken);

        Task<bool> Argo_DeleteWorkflowTemplateAsync(string argoNamespace, string templateName, CancellationToken cancellationToken);
    }
}
