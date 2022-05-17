using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Models;
using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public class Assertions
    {
        public void AssertWorkflowInstanceDetails(WorkflowInstance workflowInstance, WorkflowRevision workflowRevision, WorkflowRequestMessage workflowRequestMessage)
        {
            workflowInstance.PayloadId.Should().Match(workflowRequestMessage.PayloadId.ToString());
            workflowInstance.WorkflowId.Should().Match(workflowRevision.WorkflowId);
            workflowInstance.AeTitle.Should().Match(workflowRevision.Workflow.InformaticsGateway.AeTitle);
            workflowInstance.Tasks.Count.Should().Be(workflowRevision.Workflow.Tasks.Length);

            foreach (var task in workflowInstance.Tasks)
            {
                var workflowTask = workflowRevision.Workflow.Tasks.FirstOrDefault(x => x.Id.Equals(task.TaskId));
                if (workflowTask != null)
                {
                    task.TaskId.Should().Match(workflowTask.Id);
                    task.TaskType.Should().Match(workflowTask.Type);
                }
                else
                {
                    throw new Exception($"Workflow Revision Task or {task.TaskId} not found!");
                }
            }
        }

        public void AssertTaskDispatchEvent(TaskDispatchEvent taskDispatchEvent, WorkflowInstance workflowInstance, WorkflowRevision workflowRevision, WorkflowRequestMessage workflowRequestMessage)
        {
            taskDispatchEvent.CorrelationId.Should().Match(workflowRequestMessage.CorrelationId);
            taskDispatchEvent.TaskId.Should().Match(workflowRevision.Workflow.Tasks[0].Id);
            taskDispatchEvent.WorkflowInstanceId.Should().Match(workflowRevision.WorkflowId);
            workflowInstance.Tasks[0].Status.Should().Be(TaskExecutionStatus.Dispatched);
        }
    }
}
