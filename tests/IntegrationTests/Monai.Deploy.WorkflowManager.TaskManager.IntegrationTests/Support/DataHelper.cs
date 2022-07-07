// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.TestData;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests
{
    public class DataHelper
    {
        private RetryPolicy<List<WorkflowInstance>> RetryWorkflowInstances { get; set; }
        private RetryPolicy<List<TaskDispatchEvent>> RetryTaskDispatches { get; set; }
        private RabbitConsumer TaskDispatchConsumer { get; set; }
        public string PayloadId { get; private set; }
        public List<TaskDispatchEvent> TaskDispatchEvents = new List<TaskDispatchEvent>();

        public DataHelper()
        {
            RetryWorkflowInstances = Policy<List<WorkflowInstance>>.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
            RetryTaskDispatches = Policy<List<TaskDispatchEvent>>.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
        }

        public string GetPayloadId()
        {
            return PayloadId = Guid.NewGuid().ToString();
        }

        public TaskDispatchEvent GetTaskDispatchTestData(string name)
        {
            var taskDispatch = TaskDispatchesTestData.TestData.FirstOrDefault(c => c.Name.Equals(name));

            if (taskDispatch != null)
            {
                if (taskDispatch.TaskDispatchEvent != null)
                {
                    TaskDispatchEvents.Add(taskDispatch.TaskDispatchEvent);
                    return (taskDispatch.TaskDispatchEvent);
                }
                else
                {
                    throw new Exception($"TaskDispatchEvent {name} does not have any applicable test data, please check and try again!");
                }
            }
            else
            {
                throw new Exception($"TaskDispatchEvent {name} does not have any applicable test data, please check and try again!");
            }
        }

        //public TaskDispatchEvent GetTaskDispatchEvent(string name)
        //{
        //}
    }
}
