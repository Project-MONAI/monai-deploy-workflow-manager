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

using BoDi;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support;
using Polly;
using Polly.Retry;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TaskDestinationsStepDefinitions
    {
        private MongoClientUtil MongoClient { get; set; }
        private RetryPolicy RetryPolicy { get; set; }
        private DataHelper DataHelper { get; set; }
        private Assertions Assertions { get; set; }
        private readonly ISpecFlowOutputHelper _outputHelper;

        public TaskDestinationsStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            RetryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
            Assertions = new Assertions(objectContainer, outputHelper);
            _outputHelper = outputHelper;
        }

        [Then(@"Workflow Instance is updated with the new Tasks")]
        [Then(@"Workflow Instance is updated with the new Task")]
        public void ThenWorkflowInstanceIsUpdatedWithTheNewTask()
        {
            RetryPolicy.Execute(() =>
            {
                _outputHelper.WriteLine($"Retrieving workflow instance by id={DataHelper.TaskUpdateEvent.WorkflowInstanceId}");
                var workflowInstance = MongoClient.GetWorkflowInstanceById(DataHelper.TaskUpdateEvent.WorkflowInstanceId);
                _outputHelper.WriteLine("Retrieved workflow instance");
                Assertions.WorkflowInstanceIncludesTaskDetails(DataHelper.TaskDispatchEvents, workflowInstance, DataHelper.WorkflowRevisions[0]);
            });
        }

        [Then(@"Workflow Instance status is (.*)")]
        public void ThenWorkflowInstanceStatusIs(string status)
        {
            RetryPolicy.Execute(() =>
            {
                Contracts.Models.WorkflowInstance workflowInstance;
                if (string.IsNullOrWhiteSpace(DataHelper.TaskUpdateEvent.WorkflowInstanceId) is false)
                {
                    _outputHelper.WriteLine($"Retrieving workflow instance by id={DataHelper.TaskUpdateEvent.WorkflowInstanceId}");
                    workflowInstance = MongoClient.GetWorkflowInstanceById(DataHelper.TaskUpdateEvent.WorkflowInstanceId);
                }
                else if (string.IsNullOrWhiteSpace(DataHelper.ExportCompleteEvent.WorkflowInstanceId) is false)
                {
                    _outputHelper.WriteLine($"Retrieving workflow instance by Workflow Instance Id={DataHelper.ExportCompleteEvent.WorkflowInstanceId}");
                    workflowInstance = MongoClient.GetWorkflowInstanceById(DataHelper.ExportCompleteEvent.WorkflowInstanceId);
                }
                else if (DataHelper.WorkflowRequestMessage.PayloadId != Guid.Empty)
                {
                    _outputHelper.WriteLine($"Retrieving workflow instance by PayloadId={DataHelper.WorkflowRequestMessage.PayloadId}");
                    workflowInstance = MongoClient.GetWorkflowInstance(DataHelper.WorkflowRequestMessage.PayloadId.ToString());
                }
                else
                {
                    throw new Exception("Workflow Instance cannot be found using Id or PayloadId");
                }
                _outputHelper.WriteLine("Retrieved workflow instance");
                Assertions.WorkflowInstanceStatus(status, workflowInstance);
            });
        }

        [Then(@"The Task Dispatch event is for Task Id (.*)")]
        public void ThenTheTaskDispatchEventIsForTaskId(string taskId)
        {
            var taskDispatchEvents = DataHelper.TaskDispatchEvents;
            taskDispatchEvents[0].TaskId.Should().Be(taskId);
        }

        [Then(@"Task Dispatch events for TaskIds (.*) are published")]
        public void ThenTaskDispatchEventsForTasksArePublished(List<string> taskIds)
        {
            DataHelper.GetTaskDispatchEventByTaskId(taskIds);
        }

        [StepArgumentTransformation]
        public List<String> TransformToListOfString(string commaSeparatedList)
        {
            return commaSeparatedList.Split(",")
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }
    }
}
