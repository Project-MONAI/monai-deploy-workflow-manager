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

        /// <summary>
        /// Validates the export complete payload from the RabbitMQ queue.
        /// </summary>
        /// <param name="payload">The workflow message event.</param>
        bool ValidateExportComplete(ExportCompleteEvent payload);

        /// <summary>
        /// Validates the artifactReceived payload from the RabbitMQ queue.
        /// </summary>
        /// <param name="payload">The artifactReceived event.</param>
        bool ValidateArtifactReceived(ArtifactsReceivedEvent payload);
    }
}
