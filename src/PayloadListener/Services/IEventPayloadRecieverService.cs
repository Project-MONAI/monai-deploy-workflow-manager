// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Common;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Services
{
    public interface IEventPayloadReceiverService
    {
        /// <summary>
        /// Receives a workflow message payload and validates it,
        /// either passing it on to the workflow executor or handling the message accordingly.
        /// </summary>
        /// <param name="message">The workflow message event.</param>
        Task ReceiveWorkflowPayload(MessageReceivedEventArgs message);

        /// <summary>
        /// Receives an update task message payload and validates it,
        /// then goes on to update the workflow instance database record
        /// </summary>
        /// <param name="message">The workflow message event.</param>
        Task TaskUpdatePayload(MessageReceivedEventArgs message);
    }
}
