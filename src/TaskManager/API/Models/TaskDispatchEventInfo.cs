// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.TaskManager.API.Models
{
    public class TaskDispatchEventInfo
    {
        public TaskDispatchEvent Event { get; }
        public DateTime Started { get; }

        public TaskDispatchEventInfo(TaskDispatchEvent taskDispatchEvent)
        {
            Event = taskDispatchEvent;
            Started = DateTime.UtcNow;
        }

        public bool HasTimedOut(TimeSpan taskTimeout) => DateTime.UtcNow.Subtract(Started) >= taskTimeout;
    }
}
