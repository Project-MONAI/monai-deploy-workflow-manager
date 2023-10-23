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

using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Services
{
    public interface IWorkflowExecuterService
    {
        /// <summary>
        /// Processes the workflow request payload and create a new workflow instance.
        /// </summary>
        /// <param name="message">The workflow request message event.</param>
        Task<bool> ProcessPayload(WorkflowRequestEvent message, Payload payload);

        /// <summary>
        /// Processes the task update payload and updates the workflow instance.
        /// </summary>
        /// <param name="message">The task update message event.</param>
        Task<bool> ProcessTaskUpdate(TaskUpdateEvent message);

        /// <summary>
        /// Processes the export complete payload and updates the workflow instance.
        /// </summary>
        /// <param name="message">The export complete message event.</param>
        Task<bool> ProcessExportComplete(ExportCompleteEvent message, string correlationId);

        /// <summary>
        /// Creates a new Task Execution.
        /// </summary>
        /// <param name="task">Task Object.</param>
        /// <param name="workflowInstance">Workflow Instance.</param>
        /// <param name="bucketName">Bucket Name.</param>
        /// <param name="payloadId">Payload Id.</param>
        /// <param name="previousTaskId">Previous Tasks Id.</param>
        /// <returns></returns>
        Task<TaskExecution> CreateTaskExecutionAsync(TaskObject task, WorkflowInstance workflowInstance, string? bucketName = null, string? payloadId = null, string? previousTaskId = null);

        /// <summary>
        /// Processes the artifactReceived payload and continue workflow instance.
        /// </summary>
        /// <param name="message">The workflow request message event.</param>
        Task<bool> ProcessArtifactReceivedAsync(ArtifactsReceivedEvent message);
    }
}
