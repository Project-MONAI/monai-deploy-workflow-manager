// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    public abstract class AsynchronousRunnerBase : RunnerBase, ITaskRunner
    {
        protected TaskDispatchEvent Event { get; }

        protected AsynchronousRunnerBase(TaskDispatchEvent taskDispatchEvent)
        {
            Event = taskDispatchEvent ?? throw new ArgumentNullException(nameof(taskDispatchEvent));
        }

        public abstract Task<ExecutionStatus> ExecuteTask();

        public abstract Task<ExecutionStatus> GetStatus(string identity);
    }
}
