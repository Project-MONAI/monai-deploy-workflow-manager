using Monai.Deploy.MessageBroker.Messages;

namespace Monai.Deploy.WorkloadManager.PayloadListener.Validators
{
    public interface IEventPayloadValidator
    {
        /// <summary>
        /// Validates the workflow input payload from the RabbitMQ queue.
        /// </summary>
        Task<bool> ValidateWorkflow(WorkflowRequestMessage payload);
    }
}
