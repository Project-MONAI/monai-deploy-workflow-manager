using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Common;
using Xunit;

namespace Monai.Deploy.WorkflowManager.WorkflowExecuter.Tests.Common
{
    public class TaskExecutionStatusExtensions
    {
        [Theory]
        [InlineData(TaskExecutionStatus.Succeeded, TaskExecutionStatus.Failed, false)]
        [InlineData(TaskExecutionStatus.Succeeded, TaskExecutionStatus.Created, false)]
        [InlineData(TaskExecutionStatus.Succeeded, TaskExecutionStatus.Canceled, false)]
        [InlineData(TaskExecutionStatus.Accepted, TaskExecutionStatus.Created, false)]
        [InlineData(TaskExecutionStatus.Dispatched, TaskExecutionStatus.Created, false)]
        [InlineData(TaskExecutionStatus.Canceled, TaskExecutionStatus.Created, false)]
        [InlineData(TaskExecutionStatus.Canceled, TaskExecutionStatus.Dispatched, false)]
        [InlineData(TaskExecutionStatus.Created, TaskExecutionStatus.Dispatched, true)]
        [InlineData(TaskExecutionStatus.Dispatched, TaskExecutionStatus.Succeeded, true)]
        [InlineData(TaskExecutionStatus.Dispatched, TaskExecutionStatus.Failed, true)]
        [InlineData(TaskExecutionStatus.Dispatched, TaskExecutionStatus.Canceled, true)]
        [InlineData(TaskExecutionStatus.Accepted, TaskExecutionStatus.Succeeded, true)]
        [InlineData(TaskExecutionStatus.Accepted, TaskExecutionStatus.Failed, true)]
        [InlineData(TaskExecutionStatus.Dispatched, TaskExecutionStatus.Exported, true)]
        [InlineData(TaskExecutionStatus.Exported, TaskExecutionStatus.Succeeded, true)]
        [InlineData(TaskExecutionStatus.Exported, TaskExecutionStatus.Failed, true)]
        public void IsTaskExecutionStatusUpdateValid_ReturnesExpected(TaskExecutionStatus oldStatus, TaskExecutionStatus newStatus, bool expected) => Assert.Equal(expected, newStatus.IsTaskExecutionStatusUpdateValid(oldStatus));
    }
}
