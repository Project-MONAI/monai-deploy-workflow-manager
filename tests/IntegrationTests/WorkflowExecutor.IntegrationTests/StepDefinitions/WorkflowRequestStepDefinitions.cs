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

using BoDi;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.IntegrationTests.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using TechTalk.SpecFlow.Infrastructure;
using Polly;
using Polly.Retry;
using Monai.Deploy.WorkflowManager.WorkflowExecutor.IntegrationTests.Support;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
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
            Assertions = new Assertions(objectContainer);
            DataHelper = objectContainer.Resolve<DataHelper>();
            _outputHelper = outputHelper;
            MinioDataSeeding = new MinioDataSeeding(objectContainer.Resolve<MinioClientUtil>(), DataHelper, _outputHelper);
            RetryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 20, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
        }

        [Given(@"I have a clinical workflow (.*)")]
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

        [Given(@"I have a Workflow Instance (.*) with no artifacts")]
        public void GivenIHaveAWorkflowInstance(string name)
        {
            _outputHelper.WriteLine($"Retrieving workflow instance with name={name}");
            MongoClient.CreateWorkflowInstanceDocument(DataHelper.GetWorkflowInstanceTestData(name));
            _outputHelper.WriteLine("Retrieved workflow instance");
        }

        [Given(@"I have a Workflow Instance (.*) with artifacts (.*) in minio")]
        public async Task GivenIHaveAWorkflowInstanceWithArtifacts(string name, string folderName)
        {
            var workflowInstance = DataHelper.GetWorkflowInstanceTestData(name);
            _outputHelper.WriteLine("Seeding minio with workflow input artifacts");
            await MinioDataSeeding.SeedWorkflowInputArtifacts(workflowInstance.PayloadId, folderName);

            _outputHelper.WriteLine($"Retrieving workflow instance with name={name}");
            MongoClient.CreateWorkflowInstanceDocument(workflowInstance);
            _outputHelper.WriteLine("Retrieved workflow instance");
        }

        [Given(@"I have (.*) Workflow Instances")]
        public void GivenIHaveWorkflowInstances(int count)
        {
            _outputHelper.WriteLine($"Retrieving {count} workflow instances");
            foreach (int index in Enumerable.Range(0, count))
            {
                _outputHelper.WriteLine($"Retrieving workflow instances with index={index}");
                MongoClient.CreateWorkflowInstanceDocument(DataHelper.GetWorkflowInstanceTestDataByIndex(index));
                _outputHelper.WriteLine("Retrieved workflow instance");
            }
        }

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

        [Then(@"I can see (.*) Workflow Instances are created")]
        [Then(@"I can see (.*) Workflow Instance is created")]
        public void ThenICanSeeAWorkflowInstanceIsCreated(int count)
        {
            _outputHelper.WriteLine($"Retrieving {count} workflow instance/s using the payloadid={DataHelper.WorkflowRequestMessage.PayloadId.ToString()}");
            var workflowInstances = DataHelper.GetWorkflowInstances(count, DataHelper.WorkflowRequestMessage.PayloadId.ToString());
            _outputHelper.WriteLine($"Retrieved {count} workflow instance/s");

            if (workflowInstances != null)
            {
                foreach (var workflowInstance in workflowInstances)
                {
                    var workflowRevision = DataHelper.WorkflowRevisions.OrderByDescending(x => x.Revision).FirstOrDefault(x => x.WorkflowId.Equals(workflowInstance.WorkflowId));

                    if (workflowRevision != null)
                    {
                        Assertions.AssertWorkflowInstanceMatchesExpectedWorkflow(workflowInstance, workflowRevision, DataHelper.WorkflowRequestMessage);

                    }
                    else
                    {
                        throw new Exception($"Workflow not found for workflowId {workflowInstance.WorkflowId}");
                    }
                }
            }
        }

        [Then(@"I can see (.*) Workflow Instances are updated")]
        [Then(@"I can see (.*) Workflow Instance is updated")]
        public void ThenICanSeeAWorkflowInstanceIsUpdated(int count)
        {
            RetryPolicy.Execute(() =>
            {
                _outputHelper.WriteLine($"Retrieving {count} workflow instance/s using the payloadid={DataHelper.WorkflowInstances[0].PayloadId}");
                DataHelper.SeededWorkflowInstances = DataHelper.WorkflowInstances;
                var workflowInstances = DataHelper.GetWorkflowInstances(count, DataHelper.WorkflowInstances[0].PayloadId);
                _outputHelper.WriteLine($"Retrieved {count} workflow instance/s");

                if (workflowInstances != null)
                {
                    foreach (var workflowInstance in workflowInstances)
                    {
                        var taskUpdate = DataHelper.TaskUpdateEvent;
                        if (taskUpdate != null)
                        {
                            var workflowInstanceTask = workflowInstance.Tasks.FirstOrDefault(x => x.TaskId.Equals(taskUpdate.TaskId));
                            if (workflowInstanceTask != null)
                            {
                                workflowInstanceTask.Status.Should().Be(taskUpdate.Status);
                                Assertions.AssertOutputArtifactsForTaskUpdate(workflowInstanceTask.OutputArtifacts, DataHelper.TaskUpdateEvent.Outputs);
                            }
                        }
                    }
                }
            });
        }

        [Then(@"(.*) Task Dispatch event is published")]
        [Then(@"(.*) Task Dispatch events are published")]
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
                        Assertions.AssertTaskDispatchEvent(taskDispatchEvent, workflowInstance, workflowRevision, DataHelper.WorkflowRequestMessage);
                    }
                    else
                    {
                        Assertions.AssertTaskDispatchEvent(taskDispatchEvent, workflowInstance, workflowRevision, null, DataHelper.TaskUpdateEvent);
                    }
                }
            });

        }

        [Then(@"I can see an additional Workflow Instance is not created")]
        public void ThenICanSeeAnAdditionalWorkflowInstanceIsNotCreated()
        {
            _outputHelper.WriteLine($"Retrieving workflow instance with payloadid={DataHelper.WorkflowRequestMessage.PayloadId}");
            var workflowInstances = MongoClient.GetWorkflowInstancesByPayloadId(DataHelper.WorkflowRequestMessage.PayloadId.ToString());
            _outputHelper.WriteLine("Retrieved workflow instance");

            workflowInstances.Count.Should().Be(1);
        }

        [Then(@"A Task Dispatch event is not published")]
        public void ThenATaskDispatchEventIsNotPublished()
        {
            for (var i = 0; i < 3; i++)
            {
                var taskDispatchEvent = TaskDispatchConsumer.GetMessage<TaskDispatchEvent>();

                if (taskDispatchEvent != null)
                {
                    var workflowInstance = MongoClient.GetWorkflowInstanceById(taskDispatchEvent.WorkflowInstanceId);

                    if (workflowInstance != null)
                    {
                        if (taskDispatchEvent.ExecutionId == workflowInstance.Tasks[0].ExecutionId)
                        {
                            throw new Exception($"Task Dispatch Event has been published when workflowInstance status was {workflowInstance.Tasks[0].Status}");
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}
