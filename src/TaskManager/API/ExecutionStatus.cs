// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    public class ExecutionStatus
    {
        /// <summary>
        /// Gets or sets the status of the execution.
        /// </summary>
        public TaskExecutionStatus Status { get; set; } = TaskExecutionStatus.Unknown;

        /// <summary>
        /// Gets or sets the reason of a failure.
        /// </summary>
        public FailureReason FailureReason { get; set; } = FailureReason.None;

        /// <summary>
        /// Gets or sets any errors of the execution.
        /// </summary>
        public string Errors { get; set; } = string.Empty;
    }
}
