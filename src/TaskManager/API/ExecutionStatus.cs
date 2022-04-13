// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;
using TaskStatus = Monai.Deploy.Messaging.Events.TaskStatus;

namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    public class ExecutionStatus
    {
        /// <summary>
        /// Gets or set the status of the execution.
        /// </summary>
        public TaskStatus Status { get; set; } = TaskStatus.Unknown;

        /// <summary>
        /// Gets or set the reason of a failure.
        /// </summary>
        public FailureReason FailureReason { get; set; } = FailureReason.None;

        /// <summary>
        /// Gets or set any errors of the execution.
        /// </summary>
        public string Errors { get; set; } = string.Empty;


    }
}
