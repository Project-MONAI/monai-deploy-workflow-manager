using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Common
{
    public static class TaskExecutionStatusExtensions
    {
        public static bool IsTaskExecutionStatusUpdateValid(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus)
        {
            Guard.Against.Null(newStatus);
            Guard.Against.Null(oldStatus);

            return newStatus switch
            {
                TaskExecutionStatus.Unknown => newStatus.UnknownValidStatuses(),
                TaskExecutionStatus.Created => newStatus.CreatedValidStatuses(oldStatus),
                TaskExecutionStatus.Dispatched => newStatus.DispatchedValidStatuses(oldStatus),
                TaskExecutionStatus.Accepted => newStatus.AcceptedValidStatuses(oldStatus),
                TaskExecutionStatus.Succeeded => newStatus.SucceededValidStatuses(oldStatus),
                TaskExecutionStatus.Failed => newStatus.FailedValidStatuses(oldStatus),
                TaskExecutionStatus.Canceled => newStatus.CanceledValidStatuses(oldStatus),
                _ => false,
            };
        }

        private static bool AcceptedValidStatuses(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus) =>
            newStatus == TaskExecutionStatus.Accepted &&
                oldStatus != TaskExecutionStatus.Accepted &&
                oldStatus != TaskExecutionStatus.Created &&
                oldStatus != TaskExecutionStatus.Failed &&
                oldStatus != TaskExecutionStatus.Canceled &&
                oldStatus != TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Unknown;

        private static bool DispatchedValidStatuses(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus) =>
            newStatus == TaskExecutionStatus.Dispatched &&
                oldStatus != TaskExecutionStatus.Dispatched &&
                oldStatus != TaskExecutionStatus.Canceled &&
                oldStatus != TaskExecutionStatus.Failed &&
                oldStatus != TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Unknown;

        private static bool CreatedValidStatuses(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus) =>
            newStatus == TaskExecutionStatus.Created &&
                oldStatus != TaskExecutionStatus.Created &&
                oldStatus != TaskExecutionStatus.Dispatched &&
                oldStatus != TaskExecutionStatus.Failed &&
                oldStatus != TaskExecutionStatus.Canceled &&
                oldStatus != TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Accepted &&
                oldStatus != TaskExecutionStatus.Unknown;

        private static bool UnknownValidStatuses(this TaskExecutionStatus newStatus) =>
            false;

        private static bool SucceededValidStatuses(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus) =>
            newStatus == TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Failed;

        private static bool FailedValidStatuses(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus) =>
            newStatus == TaskExecutionStatus.Failed &&
                oldStatus != TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Failed;

        private static bool CanceledValidStatuses(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus) =>
            newStatus == TaskExecutionStatus.Canceled &&
                oldStatus != TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Failed;
    }
}
