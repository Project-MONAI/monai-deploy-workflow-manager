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

using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    public class ExecutionStatus
    {
        /// <summary>
        /// Gets or sets the status of the execution.
        /// </summary>
        public TaskExecutionStatus Status { get; set; } = TaskExecutionStatus.Created;

        /// <summary>
        /// Gets or sets the reason of a failure.
        /// </summary>
        public FailureReason FailureReason { get; set; } = FailureReason.None;

        /// <summary>
        /// Gets or sets any errors of the execution.
        /// </summary>
        public string Errors { get; set; } = string.Empty;

        /// <summary>
        /// Contains various stats
        /// </summary>
        public Dictionary<string, string> Stats { get; set; } = new();
    }
}
