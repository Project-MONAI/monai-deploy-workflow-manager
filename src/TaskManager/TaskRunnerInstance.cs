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
