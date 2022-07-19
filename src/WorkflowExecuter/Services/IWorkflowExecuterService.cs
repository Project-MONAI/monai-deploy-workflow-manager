// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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

        /// <summary>
        /// Validates a WorkflowRevision.
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public bool ValidateWorkflow(WorkflowRevision workflow, out string errors);
    }
}
