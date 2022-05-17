// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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
    }
}
