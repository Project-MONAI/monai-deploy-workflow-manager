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
using Monai.Deploy.WorkflowManager.PayloadListener.Extensions;
using Log = Monai.Deploy.WorkflowManager.Logging.Log;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Validators
{
    public class EventPayloadValidator : IEventPayloadValidator
    {
        private ILogger<EventPayloadValidator> Logger { get; }

        public EventPayloadValidator(ILogger<EventPayloadValidator> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool ValidateArtifactReceivedOrWorkflowRequestEvent(EventBase payload)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));

            if (payload is WorkflowRequestEvent or ArtifactsReceivedEvent)
            {
                var correlationId = string.Empty;
                Guid? payloadId = null;
                IEnumerable<string> workflows = Array.Empty<string>();

                switch (payload)
                {
                    case WorkflowRequestEvent wre:
                        correlationId = wre.CorrelationId;
                        payloadId = wre.PayloadId;
                        workflows = wre.Workflows;
                        break;
                    case ArtifactsReceivedEvent are:
                        correlationId = are.CorrelationId;
                        payloadId = are.PayloadId;
                        workflows = are.Workflows;
                        break;
                }

                using var loggingScope = Logger.BeginScope(new LoggingDataDictionary<string, object>
                {
                    ["correlationId"] = correlationId,
                    ["payloadId"] = payloadId,
                });

                var valid = true;
                var payloadValid = false;
                IList<string> validationErrors;
                payloadValid = payload switch
                {
                    ArtifactsReceivedEvent artifactsReceivedEvent => artifactsReceivedEvent.IsValid(out validationErrors),
                    WorkflowRequestEvent workflowRequestEvent => workflowRequestEvent.IsValid(out validationErrors),
                    _ => throw new ArgumentOutOfRangeException(nameof(payload), payload, null)
                };

                if (!payloadValid)
                {
                    Log.FailedToValidateWorkflowRequestEvent(Logger, string.Join(Environment.NewLine, validationErrors));
                }

                valid &= payloadValid;

                foreach (var workflow in workflows)
                {
                    var workflowValid = !string.IsNullOrEmpty(workflow);

                    if (!workflowValid)
                    {
                        Log.FailedToValidateWorkflowRequestEvent(Logger, "Workflow id is empty string");
                    }

                    valid &= workflowValid;
                }

                return valid;
            };
            return false;
        }

        public bool ValidateWorkflowRequest(WorkflowRequestEvent payload)
            => ValidateArtifactReceivedOrWorkflowRequestEvent(payload);

        public bool ValidateArtifactReceived(ArtifactsReceivedEvent payload)
            => ValidateArtifactReceivedOrWorkflowRequestEvent(payload);

        public bool ValidateTaskUpdate(TaskUpdateEvent payload)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));

            using var loggingScope = Logger.BeginScope(new LoggingDataDictionary<string, object>
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
                Log.FailedToValidateTaskUpdateEvent(Logger, e);
                return false;
            }

            return true;
        }

        public bool ValidateExportComplete(ExportCompleteEvent payload)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));

            using var loggingScope = Logger.BeginScope(new LoggingDataDictionary<string, object>
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
                Log.FailedToValidateExportCompleteEvent(Logger, e);

                return false;
            }

            return true;
        }
    }
}
