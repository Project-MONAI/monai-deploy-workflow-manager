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
    }
}
