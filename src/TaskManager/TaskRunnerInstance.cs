// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.API;

namespace Monai.Deploy.WorkflowManager.TaskManager
{
    internal class TaskRunnerInstance
    {
        public ITaskPlugin Runner { get; }
        public TaskDispatchEvent Event { get; }
        public DateTime Started { get; }

        public TaskRunnerInstance(ITaskPlugin runner, TaskDispatchEvent taskDispatchEvent)
        {
            Runner = runner;
            Event = taskDispatchEvent;
            Started = DateTime.UtcNow;
        }

        public bool HasTimedOut(TimeSpan taskTimeout) => DateTime.UtcNow.Subtract(Started) >= taskTimeout;
    }
}
