using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Models;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using System.Linq;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public class Assertions
    {
        public void AssertWorkflowInstanceMatchesExpectedWorkflow(WorkflowInstance workflowInstance, WorkflowRevision workflowRevision, WorkflowRequestMessage workflowRequestMessage)
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

        public void AssertTaskDispatchEvent(TaskDispatchEvent taskDispatchEvent, WorkflowInstance workflowInstance, WorkflowRevision workflowRevision, WorkflowRequestMessage workflowRequestMessage = null, TaskUpdateEvent taskUpdateEvent = null)
        {
            var taskDetails = workflowInstance.Tasks.FirstOrDefault(c => c.TaskId.Equals(taskDispatchEvent.TaskId));

            if (workflowRequestMessage != null)
            {
                taskDispatchEvent.CorrelationId.Should().Match(workflowRequestMessage.CorrelationId);
            }
            else
            {
                taskDispatchEvent.CorrelationId.Should().Match(taskUpdateEvent.CorrelationId);
            }

            taskDispatchEvent.WorkflowInstanceId.Should().Match(workflowInstance.Id);

            if (taskUpdateEvent == null)
            {
                taskDispatchEvent.TaskId.Should().Match(workflowRevision.Workflow.Tasks[0].Id);
            }
            else
            {
                taskDispatchEvent.TaskId.Should().Match(taskDetails.TaskId);
            }

            taskDetails.Status.Should().Be(TaskExecutionStatus.Dispatched);
        }

        public void AssertWorkflowIstanceMatchesExpectedTaskStatusUpdate(WorkflowInstance updatedWorkflowInstance, TaskExecutionStatus taskExecutionStatus)
        {
            updatedWorkflowInstance.Tasks[0].Status.Should().Be(taskExecutionStatus);
        }

        public void WorkflowInstanceIncludesTaskDetails(List<TaskDispatchEvent> taskDispatchEvents, WorkflowInstance workflowInstance, WorkflowRevision workflowRevision)
        {
            foreach (var taskDispatchEvent in taskDispatchEvents)
            {
                var workflowInstanceTaskDetails = workflowInstance.Tasks.FirstOrDefault(c => c.TaskId.Equals(taskDispatchEvent.TaskId));
                var workflowTaskDetails = workflowRevision.Workflow.Tasks.FirstOrDefault(c => c.Id.Equals(taskDispatchEvent.TaskId));
                workflowInstanceTaskDetails.ExecutionId.Should().Be(taskDispatchEvent.ExecutionId);
                workflowInstanceTaskDetails.Status.Should().Be(TaskExecutionStatus.Dispatched);
                workflowInstanceTaskDetails.TaskType.Should().Be(workflowTaskDetails.Type);
            }
        }

        public void WorkflowInstanceStatus(string status, WorkflowInstance workflowInstance)
        {
            workflowInstance.Status.Should().Be((Status)Enum.Parse(typeof(Status), status));
        }

        public void AssertWorkflowList(List<WorkflowRevision> expectedWorkflowRevisions, List<WorkflowRevision> actualWorkflowRevisions)
        {
            actualWorkflowRevisions.Should().HaveCount(expectedWorkflowRevisions.Count);
            expectedWorkflowRevisions.OrderBy(x => x.Id).SequenceEqual(actualWorkflowRevisions.OrderBy(x => x.Id));
        }
    }
}
