using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkloadManager.Logging.Logging;
using Monai.Deploy.WorkloadManager.PayloadListener.Extensions;

namespace Monai.Deploy.WorkloadManager.PayloadListener.Validators
{
    public class EventPayloadValidator : IEventPayloadValidator
    {
        private ILogger<EventPayloadValidator> Logger { get; }

        public EventPayloadValidator(ILogger<EventPayloadValidator> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool ValidateWorkflowRequest(WorkflowRequestMessage payload)
        {
            Guard.Against.Null(payload, nameof(payload));

            var valid = true;
            var payloadValid = payload.IsValid(out var validationErrors);

            if (!payloadValid)
            {
                Logger.ValidationErrors(string.Join(Environment.NewLine, validationErrors));
            }

            valid &= payloadValid;

            var workflows = payload.Workflows;

            foreach (var workflow in workflows)
            {
                var workflowValid = workflow.ToWorkflowAndValidate(out var validationErrorsWorkflow);

                if (!workflowValid)
                {
                    Logger.ValidationErrors(string.Join(Environment.NewLine, validationErrors));
                }

                valid &= workflowValid;
            }

            return valid;
        }
    }
}
