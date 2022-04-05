using Ardalis.GuardClauses;
using Monai.Deploy.MessageBroker.Messages;
using Monai.Deploy.WorkloadManager.PayloadListener.Extensions;

namespace Monai.Deploy.WorkloadManager.PayloadListener.Validators
{
    public class EventPayloadValidator : IEventPayloadValidator
    {
        public EventPayloadValidator()
        {

        }

        public bool ValidateWorkflow(WorkflowRequestMessage payload)
        {
            Guard.Against.Null(payload, nameof(payload));

            var valid = true;
            valid &= payload.IsValid(out var validationErrors);

            var workflows = payload.Workflows;

            foreach (var workflow in workflows)
            {
                var validatedWorkflow = workflow.ToWorkflowAndValidate(out var validationErrorsWorkflow);
            }
            return valid;
        }
    }
}
