using Monai.Deploy.Messaging.Messages;

namespace Monai.Deploy.WorkloadManager.PayloadListener.Validators
{
    public interface IEventPayloadValidator
    {
        /// <summary>
        /// Validates the workflow input payload from the RabbitMQ queue.
        /// </summary>
        bool ValidateWorkflowRequest(WorkflowRequestMessage payload);
    }
}
