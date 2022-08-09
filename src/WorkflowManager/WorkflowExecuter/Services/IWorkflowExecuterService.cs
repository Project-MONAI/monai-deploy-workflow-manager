/*
 * Copyright 2021-2022 MONAI Consortium
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
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.WorkfowExecuter.Services
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
    }
}
