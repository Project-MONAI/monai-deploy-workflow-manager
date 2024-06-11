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

        /// <summary>
        /// Receives an export complete message payload and validates it,
        /// then goes on to update the workflow instance database record
        /// </summary>
        /// <param name="message">The export complete event.</param>
        Task ExportCompletePayload(MessageReceivedEventArgs message);

        /// <summary>
        /// Receives a artifactReceived payload and validates it,
        /// either passing it on to the workflow executor or handling the message accordingly.
        /// </summary>
        /// <param name="message">The artifactReceived event.</param>
        Task ArtifactReceivePayload(MessageReceivedEventArgs message);
    }
}
