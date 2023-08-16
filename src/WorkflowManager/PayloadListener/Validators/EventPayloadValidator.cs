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

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Common.Logging;
using Monai.Deploy.Common.PayloadListener.Extensions;

namespace Monai.Deploy.Common.PayloadListener.Validators
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

            using var loggingScope = Logger.BeginScope(new Dictionary<string, object>
            {
                ["correlationId"] = payload.CorrelationId,
                ["payloadId"] = payload.PayloadId,
            });

            var valid = true;
            var payloadValid = payload.IsValid(out var validationErrors);

            if (!payloadValid)
            {
                Logger.FailedToValidateWorkflowRequestEvent(string.Join(Environment.NewLine, validationErrors));
            }

            valid &= payloadValid;

            foreach (var workflow in payload.Workflows)
            {
                var workflowValid = !string.IsNullOrEmpty(workflow);

                if (!workflowValid)
                {
                    Logger.FailedToValidateWorkflowRequestEvent("Workflow id is empty string");
                }

                valid &= workflowValid;
            }

            return valid;
        }

        public bool ValidateTaskUpdate(TaskUpdateEvent payload)
        {
            Guard.Against.Null(payload, nameof(payload));

            using var loggingScope = Logger.BeginScope(new Dictionary<string, object>
            {
                ["correlationId"] = payload.CorrelationId,
                ["executionId"] = payload.ExecutionId,
                ["taskId"] = payload.TaskId,
            });

            try
            {
                payload.Validate();
            }
            catch (MessageValidationException e)
            {
                Logger.FailedToValidateTaskUpdateEvent(e);
                return false;
            }

            return true;
        }

        public bool ValidateExportComplete(ExportCompleteEvent payload)
        {
            Guard.Against.Null(payload, nameof(payload));

            using var loggingScope = Logger.BeginScope(new Dictionary<string, object>
            {
                ["workflowInstanceId"] = payload.WorkflowInstanceId,
                ["exportTaskId"] = payload.ExportTaskId,
            });

            try
            {
                payload.Validate();
            }
            catch (MessageValidationException e)
            {
                Logger.FailedToValidateExportCompleteEvent(e);

                return false;
            }

            return true;
        }
    }
}
