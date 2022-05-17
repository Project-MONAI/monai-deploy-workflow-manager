// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    public abstract class TaskPluginBase : ITaskPlugin
    {
        protected bool DisposedValue { get; private set; }
        protected TaskDispatchEvent Event { get; }

        protected TaskPluginBase(TaskDispatchEvent taskDispatchEvent)
        {
            Event = taskDispatchEvent ?? throw new ArgumentNullException(nameof(taskDispatchEvent));
        }

        public abstract Task<ExecutionStatus> ExecuteTask(CancellationToken cancellationToken = default);

        public abstract Task<ExecutionStatus> GetStatus(string identity, CancellationToken cancellationToken = default);

        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                DisposedValue = true;
            }
        }

        ~TaskPluginBase() => Dispose(disposing: false);

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
