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

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using Monai.Deploy.WorkflowManager.PayloadListener.Extensions;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Validators
{
    public class EventPayloadValidator : IEventPayloadValidator
    {
        private ILogger<EventPayloadValidator> Logger { get; }

        public EventPayloadValidator(ILogger<EventPayloadValidator> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool ValidateWorkflowRequest(WorkflowRequestEvent payload)
        {
            Guard.Against.Null(payload, nameof(payload));

            var valid = true;
            var payloadValid = payload.IsValid(out var validationErrors);

            if (!payloadValid)
            {
                Logger.ValidationErrors(string.Join(Environment.NewLine, validationErrors));
            }

            valid &= payloadValid;

            if (payload.Workflows is null)
            {
                return valid;
            }

            foreach (var workflow in payload.Workflows)
            {
                Guard.Against.Null(workflow, nameof(workflow));

                var workflowValid = !string.IsNullOrEmpty(workflow);

                if (!workflowValid)
                {
                    Logger.ValidationErrors("Workflow is null or empty");
                }

                valid &= workflowValid;
            }

            return valid;
        }

        public bool ValidateTaskUpdate(TaskUpdateEvent payload)
        {
            Guard.Against.Null(payload, nameof(payload));

            try
            {
                payload.Validate();
            }
            catch (MessageValidationException e)
            {
                Logger.Exception($"Failed to validate {nameof(TaskUpdateEvent)}", e);

                return false;
            }

            return true;
        }

        public bool ValidateExportComplete(ExportCompleteEvent payload)
        {
            Guard.Against.Null(payload, nameof(payload));

            try
            {
                payload.Validate();
            }
            catch (MessageValidationException e)
            {
                Logger.Exception($"Failed to validate {nameof(ExportCompleteEvent)}", e);

                return false;
            }

            return true;
        }
    }
}
