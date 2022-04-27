namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class TaskDispatchMessage
    {
        public string ExecutionId { get; set; }

        public string WorkflowInstanceId { get; set; }

        public StorageInformation StorageInformation { get; set; }
    }
}
