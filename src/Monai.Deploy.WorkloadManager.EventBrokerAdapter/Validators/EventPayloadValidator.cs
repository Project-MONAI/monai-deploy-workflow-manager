using Ardalis.GuardClauses;
using Monai.Deploy.MessageBroker.Messages;

namespace Monai.Deploy.WorkloadManager.PayloadListener.Validators
{
    public class EventPayloadValidator : IEventPayloadValidator
    {
        public EventPayloadValidator()
        {

        }

        public Task<bool> ValidateWorkflow(WorkflowRequestMessage payload)
        {
            Guard.Against.Null(payload, nameof(payload));

            var validationErrors = new List<string>();

            var valid = true;
            valid &= IsAeTitleValid(payload.GetType().Name, payload.CallingAeTitle, validationErrors);

            return true;
        }
    }
}
