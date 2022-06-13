// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Services
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

        /// <summary>
        /// Handles task destinations
        /// </summary>
        /// <param name="workflowInstance"></param>
        /// <param name="workflow"></param>
        /// <param name="currentTaskDestinations"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        List<TaskExecution> HandleTaskDestinations(WorkflowInstance workflowInstance,
                                                    WorkflowRevision workflow,
                                                    TaskDestination[]? currentTaskDestinations,
                                                    Dictionary<string, object> metadata);
    }
}
