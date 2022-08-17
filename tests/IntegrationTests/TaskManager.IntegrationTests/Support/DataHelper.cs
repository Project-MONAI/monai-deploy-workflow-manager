/*
 * Copyright 2021-2022 MONAI Consortium
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
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Events;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support
{
    public class DataHelper
    {
        private RetryPolicy<ClinicalReviewRequestEvent> RetryClinincalReview { get; set; }
        private RetryPolicy<TaskUpdateEvent> RetryTaskUpdate { get; set; }
        private RabbitConsumer TaskUpdateConsumer { get; set; }
        private RabbitConsumer ClinicalReviewConsumer { get; set; }
        public TaskDispatchEvent TaskDispatchEvent { get; set; }
        public TaskUpdateEvent TaskUpdateEvent { get; set; }
        public TaskCallbackEvent TaskCallbackEvent { get; set; }
        public ClinicalReviewRequestEvent ClinicalReviewRequestEvent { get; set; }
        private ISpecFlowOutputHelper OutputHelper { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DataHelper(IObjectContainer objectContainer)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            ClinicalReviewConsumer = objectContainer.Resolve<RabbitConsumer>("ClinicalReviewConsumer") ?? throw new ArgumentNullException(nameof(RabbitConsumer));
            TaskUpdateConsumer = objectContainer.Resolve<RabbitConsumer>("TaskUpdateConsumer") ?? throw new ArgumentNullException(nameof(RabbitConsumer));
            RetryClinincalReview = Policy<ClinicalReviewRequestEvent>.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
            RetryTaskUpdate = Policy<TaskUpdateEvent>.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
            OutputHelper = objectContainer.Resolve<ISpecFlowOutputHelper>();
        }

        public TaskCallbackEvent GetTaskCallbackTestData(string name)
        {
            var taskCallback = TaskCallbacksTestData.TestData.FirstOrDefault(c => c.Name.Equals(name));

            if (taskCallback != null)
            {
                if (taskCallback.TaskCallbackEvent != null)
                {
                    TaskCallbackEvent = taskCallback.TaskCallbackEvent;
                    return (TaskCallbackEvent);
                }
                else
                {
                    throw new Exception($"TaskCallbackEvent {name} does not have any applicable test data, please check and try again!");
                }
            }
            else
            {
                throw new Exception($"TaskCallbackEvent {name} does not have any applicable test data, please check and try again!");
            }
        }

        public TaskDispatchEvent GetTaskDispatchTestData(string name)
        {
            var taskDispatch = TaskDispatchesTestData.TestData.FirstOrDefault(c => c.Name.Equals(name));

            if (taskDispatch != null)
            {
                if (taskDispatch.TaskDispatchEvent != null)
                {
                    TaskDispatchEvent = taskDispatch.TaskDispatchEvent;
                    return (TaskDispatchEvent);
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

        public TaskUpdateEvent GetTaskUpdateEvent()
        {
            var res = RetryTaskUpdate.Execute(() =>
            {
                var message = TaskUpdateConsumer.GetMessage<TaskUpdateEvent>();

                if (message != null)
                {
                    if (message.ExecutionId.Equals(TaskDispatchEvent?.ExecutionId))
                    {
                        TaskUpdateEvent = message;

                        return TaskUpdateEvent;
                    }
                }

                throw new Exception($"TaskUpdateEvent not published for execution id {TaskDispatchEvent?.ExecutionId}");
            });

            return res;
        }

        public ClinicalReviewRequestEvent GetClinicalReviewRequestEvent()
        {
            OutputHelper.WriteLine($"Retreiving Clincial Review Event for executionId {TaskDispatchEvent?.ExecutionId}");

            var res = RetryClinincalReview.Execute(() =>
            {
                var message = ClinicalReviewConsumer.GetMessage<ClinicalReviewRequestEvent>();

                if (message != null)
                {
                    if (message.ExecutionId.Equals(TaskDispatchEvent?.ExecutionId))
                    {
                        ClinicalReviewRequestEvent = message;

                        OutputHelper.WriteLine($"Consumed Clincial Review Event for executionId {TaskDispatchEvent?.ExecutionId}");

                        return ClinicalReviewRequestEvent;
                    }
                }

                throw new Exception($"TaskUpdateEvent not published for execution id {TaskDispatchEvent?.ExecutionId}");
            });

            return res;
        }
    }
}
