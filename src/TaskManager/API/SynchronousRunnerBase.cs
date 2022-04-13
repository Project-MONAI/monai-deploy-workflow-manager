// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    public abstract class SynchronousRunnerBase : RunnerBase, ITaskRunner
    {
        protected SynchronousRunnerBase()
        {
        }

        public abstract Task<ExecutionStatus> ExecuteTask();

        public abstract Task<ExecutionStatus> GetStatus(string identity);
    }
}
