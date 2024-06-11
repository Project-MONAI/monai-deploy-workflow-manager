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
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Models;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.Support;
using MongoDB.Driver;
using Polly;
using Polly.Retry;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowRequestStepDefinitions
    {
        private RabbitPublisher WorkflowPublisher { get; set; }
        private RabbitConsumer TaskDispatchConsumer { get; set; }
        private MongoClientUtil MongoClient { get; set; }
        private Assertions Assertions { get; set; }
        private DataHelper DataHelper { get; set; }
        private readonly ISpecFlowOutputHelper _outputHelper;
        private RetryPolicy RetryPolicy { get; set; }
        private MinioDataSeeding MinioDataSeeding { get; set; }

        public WorkflowRequestStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            WorkflowPublisher = objectContainer.Resolve<RabbitPublisher>("WorkflowPublisher");
            TaskDispatchConsumer = objectContainer.Resolve<RabbitConsumer>("TaskDispatchConsumer");
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            Assertions = new Assertions(objectContainer, outputHelper);
            DataHelper = objectContainer.Resolve<DataHelper>();
            _outputHelper = outputHelper;
            MinioDataSeeding = new MinioDataSeeding(objectContainer.Resolve<MinioClientUtil>(), DataHelper, _outputHelper);
            RetryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 20, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
        }

        [Given(@"I have a clinical workflow (?!.* )(.*)")]
        public void GivenIHaveClinicalWorkflows(string name)
        {
            _outputHelper.WriteLine($"Retrieving workflow revision with name={name}");
            MongoClient.CreateWorkflowRevisionDocument(DataHelper.GetWorkflowRevisionTestData(name));
            _outputHelper.WriteLine("Retrieved workflow revision");
        }

        [Given(@"I have (.*) clinical workflows")]
        public void GivenIHaveClinicalWorkflows(int count)
        {
            _outputHelper.WriteLine($"Retrieving {count} workflow revisions");
            foreach (int index in Enumerable.Range(0, count))
            {
                _outputHelper.WriteLine($"Retrieving workflow revision with index={index}");
                MongoClient.CreateWorkflowRevisionDocument(DataHelper.GetWorkflowRevisionTestDataByIndex(index));
                _outputHelper.WriteLine("Retrieved workflow revision");
            }
        }

        [Given(@"I publish a Workflow Request Message (.*) with artifacts (.*) in minio")]
        [When(@"I publish a Workflow Request Message (.*) with artifacts (.*) in minio")]
        public async Task WhenIPublishAWorkflowRequestMessageWithObjects(string name, string folderName)
        {
            var workflowRequestMessage = DataHelper.GetWorkflowRequestTestData(name);
            _outputHelper.WriteLine("Seeding minio with workflow input artifacts");
            await MinioDataSeeding.SeedWorkflowInputArtifacts(workflowRequestMessage.PayloadId.ToString(), folderName);

            var message = new JsonMessage<WorkflowRequestMessage>(
                workflowRequestMessage,
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                Guid.NewGuid().ToString(),
                string.Empty);

            _outputHelper.WriteLine($"Publishing WorkflowRequestEvent with name={name}");
            WorkflowPublisher.PublishMessage(message.ToMessage());
            _outputHelper.WriteLine($"Event published");
        }

        [When(@"I publish a Workflow Request Message (.*) with no artifacts")]
        public void WhenIPublishAWorkflowRequestMessageWithNoObjects(string name)
        {
            var message = new JsonMessage<WorkflowRequestMessage>(
                DataHelper.GetWorkflowRequestTestData(name),
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                Guid.NewGuid().ToString(),
                string.Empty);

            _outputHelper.WriteLine($"Publishing WorkflowRequestEvent with name={name}");
            WorkflowPublisher.PublishMessage(message.ToMessage());
            _outputHelper.WriteLine($"Event published");
        }

        [Then(@"([1-9]*) Task Dispatch event is published")]
        [Then(@"([1-9]*) Task Dispatch events are published")]
        public void TaskDispatchEventIsPublished(int count)
        {
            _outputHelper.WriteLine($"Retrieving {count} task dispatch event/s");
            var taskDispatchEvents = DataHelper.GetTaskDispatchEvents(count, DataHelper.WorkflowInstances);
            _outputHelper.WriteLine($"Retrieved {count} task dispatch event/s");

            RetryPolicy.Execute(() =>
            {
                foreach (var taskDispatchEvent in taskDispatchEvents)
                {
                    var workflowInstance = MongoClient.GetWorkflowInstanceById(taskDispatchEvent.WorkflowInstanceId);

                    var workflowRevision = DataHelper.WorkflowRevisions.OrderByDescending(x => x.Revision).FirstOrDefault(x => x.WorkflowId.Equals(workflowInstance.WorkflowId));

                    if (string.IsNullOrEmpty(DataHelper.TaskUpdateEvent.ExecutionId))
                    {
#pragma warning disable CS8604 // Possible null reference argument.
                        Assertions.AssertTaskDispatchEvent(taskDispatchEvent, workflowInstance, workflowRevision, DataHelper.WorkflowRequestMessage, null);
                    }
                    else
                    {
                        Assertions.AssertTaskDispatchEvent(taskDispatchEvent, workflowInstance, workflowRevision, null, DataHelper.TaskUpdateEvent);
                    }
#pragma warning restore CS8604 // Possible null reference argument.
                }
            });
        }

        [Then(@"A Task Dispatch event is not published")]
        public void ThenATaskDispatchEventIsNotPublished()
        {
            for (var i = 0; i < 5; i++)
            {
                _outputHelper.WriteLine($"Retrieving task dispatch event/s");
                var taskDispatchEvents = DataHelper.GetTaskDispatchEvents(0, DataHelper.WorkflowInstances);
                _outputHelper.WriteLine($"Retrieved task dispatch event/s");

                foreach (var taskDispatchEvent in taskDispatchEvents)
                {
                    if (taskDispatchEvent != null)
                    {
                        var workflowInstance = MongoClient.GetWorkflowInstanceById(taskDispatchEvent.WorkflowInstanceId);

                        if (workflowInstance != null)
                        {
                            if (workflowInstance.Tasks.FirstOrDefault(x => x.ExecutionId.Equals(taskDispatchEvent.ExecutionId)) != null)
                            {
                                throw new Exception($"Task Dispatch Event has been published when workflowInstance status was {workflowInstance.Tasks[0].Status}");
                            }
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        [Then(@"No workflow instances will be created")]
        public void ThenTheWorkflowWillNotTriggerAnyNewWorkflowInstances()
        {
            foreach (var workflowRevision in DataHelper.WorkflowRevisions)
            {
                for (int i = 0; i < 5; i++)
                {
                    var workflowInstance = MongoClient.GetWorkflowInstanceByWorkflowId(workflowRevision.WorkflowId);
                    workflowInstance.Should().BeNull();
                    Thread.Sleep(500);
                }
            }
        }
    }
}
