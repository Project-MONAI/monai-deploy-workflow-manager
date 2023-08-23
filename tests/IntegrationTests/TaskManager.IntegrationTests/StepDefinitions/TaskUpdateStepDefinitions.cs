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
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TaskUpdateStepDefinitions
    {
        public TaskUpdateStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
            DataHelper = objectContainer.Resolve<DataHelper>() ?? throw new ArgumentNullException(nameof(DataHelper));
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            RetryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 20, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
            Assertions = new Assertions(_outputHelper);
        }

        private readonly ISpecFlowOutputHelper _outputHelper;
        private MongoClientUtil MongoClient { get; }
        private RetryPolicy RetryPolicy { get; set; }
        public DataHelper DataHelper { get; }
        public Assertions Assertions { get; }

        [When(@"A Task Update event with status (.*) is published with Task Dispatch details")]
        [Then(@"A Task Update event with status (.*) is published with Task Dispatch details")]
        public void ATaskUpdateEventIsPublishedWithTaskDispatchDetails(string status)
        {
            var taskUpdateEvent = DataHelper.GetTaskUpdateEvent();

            switch (status.ToLower())
            {
                case "accepted":
                    Assertions.AssertTaskUpdateEventFromTaskDispatch(taskUpdateEvent, DataHelper.TaskDispatchEvent, TaskExecutionStatus.Accepted);
                    break;

                case "failed":
                    Assertions.AssertTaskUpdateEventFromTaskDispatch(taskUpdateEvent, DataHelper.TaskDispatchEvent, TaskExecutionStatus.Failed);
                    break;

                default:
                    throw new Exception($"Status {status} is not supported! Please check and try again!");
            }
        }

        [Then(@"The Task Dispatch event is saved in mongo")]
        public void TheTaskDispatchEventIsSavedInMongo()
        {
            RetryPolicy.Execute(() =>
            {
                _outputHelper.WriteLine($"Retrieving task dispatch by id={DataHelper.TaskDispatchEvent.ExecutionId}");
                var storedTaskDispatchEvent = MongoClient.GetTaskDispatchEventInfoByExecutionId(DataHelper.TaskDispatchEvent.ExecutionId);
                _outputHelper.WriteLine("Retrieved task dispatch");
                Assertions.AssertTaskDispatchEventStoredInMongo(storedTaskDispatchEvent, DataHelper.TaskDispatchEvent);
            });
        }

        [Then(@"A Task Update event with status (.*) is published with Task Callback details")]
        public void ATaskUpdateEventIsPublishedWithTaskCallbackDetails(string status)
        {
            var taskUpdateEvent = DataHelper.GetTaskUpdateEvent();

            switch (status.ToLower())
            {
                case "succeeded":
                    Assertions.AssertTaskUpdateEventFromTaskCallback(taskUpdateEvent, DataHelper.TaskCallbackEvent, TaskExecutionStatus.Succeeded);
                    break;

                case "failed":
                    Assertions.AssertTaskUpdateEventFromTaskCallback(taskUpdateEvent, DataHelper.TaskCallbackEvent, TaskExecutionStatus.Failed);
                    break;

                case "partialfail":
                    Assertions.AssertTaskUpdateEventFromTaskCallback(taskUpdateEvent, DataHelper.TaskCallbackEvent, TaskExecutionStatus.PartialFail);
                    break;

                default:
                    throw new Exception($"Status {status} is not supported! Please check and try again!");
            }
        }
    }
}
