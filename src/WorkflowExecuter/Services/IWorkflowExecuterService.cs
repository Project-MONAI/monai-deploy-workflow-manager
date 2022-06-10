// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.WorkfowExecuter.Services
{
    public interface IWorkflowExecuterService
    {
        /// <summary>
        /// Processes the workflow request payload and create a new workflow instance.
        /// </summary>
        /// <param name="message">The workflow request message event.</param>
        Task<bool> ProcessPayload(WorkflowRequestEvent message);

        /// <summary>
        /// Processes the task update payload and updates the workflow instance.
        /// </summary>
        /// <param name="message">The workflow request message event.</param>
        Task<bool> ProcessTaskUpdate(TaskUpdateEvent message);
    }
}
