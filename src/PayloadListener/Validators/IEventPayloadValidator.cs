// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Validators
{
    public interface IEventPayloadValidator
    {
        /// <summary>
        /// Validates the workflow input payload from the RabbitMQ queue.
        /// </summary>
        /// <param name="payload">The workflow message event.</param>
        bool ValidateWorkflowRequest(WorkflowRequestEvent payload);

        /// <summary>
        /// Validates the task input payload from the RabbitMQ queue.
        /// </summary>
        /// <param name="payload">The workflow message event.</param>
        bool ValidateTaskUpdate(TaskUpdateEvent payload);
    }
}
