/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Common
{
    public static class TaskExecutionStatusExtensions
    {
        public static bool IsTaskExecutionStatusUpdateValid(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus)
        {
            ArgumentNullException.ThrowIfNull(newStatus, nameof(newStatus));
            ArgumentNullException.ThrowIfNull(oldStatus, nameof(oldStatus));

            return newStatus switch
            {
                TaskExecutionStatus.Created => newStatus.CreatedValidStatuses(oldStatus),
                TaskExecutionStatus.Dispatched => newStatus.DispatchedValidStatuses(oldStatus),
                TaskExecutionStatus.Accepted => newStatus.AcceptedValidStatuses(oldStatus),
                TaskExecutionStatus.Succeeded => newStatus.SucceededValidStatuses(oldStatus),
                TaskExecutionStatus.Failed => newStatus.FailedValidStatuses(oldStatus),
                TaskExecutionStatus.PartialFail => newStatus.PartialFailValidStatuses(oldStatus),
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
                oldStatus != TaskExecutionStatus.Succeeded;

        private static bool DispatchedValidStatuses(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus) =>
            newStatus == TaskExecutionStatus.Dispatched &&
                oldStatus != TaskExecutionStatus.Dispatched &&
                oldStatus != TaskExecutionStatus.Canceled &&
                oldStatus != TaskExecutionStatus.Failed &&
                oldStatus != TaskExecutionStatus.Succeeded;

        private static bool CreatedValidStatuses(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus) =>
            newStatus == TaskExecutionStatus.Created &&
                oldStatus != TaskExecutionStatus.Created &&
                oldStatus != TaskExecutionStatus.Dispatched &&
                oldStatus != TaskExecutionStatus.Failed &&
                oldStatus != TaskExecutionStatus.Canceled &&
                oldStatus != TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Accepted;

        private static bool SucceededValidStatuses(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus) =>
            newStatus == TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Failed;

        private static bool FailedValidStatuses(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus) =>
            newStatus == TaskExecutionStatus.Failed &&
                oldStatus != TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Failed;

        private static bool PartialFailValidStatuses(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus) =>
            newStatus == TaskExecutionStatus.PartialFail &&
                oldStatus != TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Failed;

        private static bool CanceledValidStatuses(this TaskExecutionStatus newStatus, TaskExecutionStatus oldStatus) =>
            newStatus == TaskExecutionStatus.Canceled &&
                oldStatus != TaskExecutionStatus.Succeeded &&
                oldStatus != TaskExecutionStatus.Failed;
    }
}
