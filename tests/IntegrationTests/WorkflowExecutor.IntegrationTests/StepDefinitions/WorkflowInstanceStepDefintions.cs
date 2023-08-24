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
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.Support;
using Polly;
using Polly.Retry;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.StepDefinitions
{
    [Binding]
    internal class WorkflowInstanceStepDefintions
    {
        private RabbitPublisher WorkflowPublisher { get; set; }
        private RabbitConsumer TaskDispatchConsumer { get; set; }
        private MongoClientUtil MongoClient { get; set; }
        private Assertions Assertions { get; set; }
        private DataHelper DataHelper { get; set; }
        private readonly ISpecFlowOutputHelper _outputHelper;
        public MinioDataSeeding MinioDataSeeding { get; }
        private RetryPolicy RetryPolicy { get; set; }

        public WorkflowInstanceStepDefintions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
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

        [Given(@"I have (\d*) failed workflow Instances")]
        public void GivenIHaveWorkflowInstancesWithUnacknowledgedWorkflowErrors(int count)
        {
            _outputHelper.WriteLine($"Retrieving {count} workflow instances");
            var listOfWorkflowInstance = new List<WorkflowInstance>();

            foreach (var index in Enumerable.Range(0, count))
            {
                _outputHelper.WriteLine($"Retrieving workflow instances with index={index}");
                var wi = DataHelper.GetWorkflowInstanceTestDataByIndex(index);
                wi.Status = Status.Failed;
                wi.AcknowledgedWorkflowErrors = null;

                listOfWorkflowInstance.Add(wi);
                MongoClient.CreateWorkflowInstanceDocument(wi);
                _outputHelper.WriteLine("Retrieved workflow instance");
            }
            DataHelper.SeededWorkflowInstances = listOfWorkflowInstance;
        }

        [Given(@"I have an acknowledged failed workflow Instances")]
        public void GivenIHaveWorkflowInstancesWithAcknowledgedWorkflowErrors()
        {
            var listOfWorkflowInstance = new List<WorkflowInstance>();
            var wi = DataHelper.GetWorkflowInstanceTestData("Workflow_Instance_For_Failed_Partial");
            wi.Status = Status.Failed;
            wi.AcknowledgedWorkflowErrors = DateTime.UtcNow;
            listOfWorkflowInstance.Add(wi);
            MongoClient.CreateWorkflowInstanceDocument(wi);
            DataHelper.SeededWorkflowInstances = listOfWorkflowInstance;
        }

        [Given(@"I have (\d*) partial failed Workflow Instance")]
        public void GivenIHavePartialFailedWorkflowInstance(int count)
        {
            _outputHelper.WriteLine($"Retrieving {count} workflow instances");
            var listOfWorkflowInstance = new List<WorkflowInstance>();

            foreach (var index in Enumerable.Range(0, count))
            {
                _outputHelper.WriteLine($"Retrieving workflow instances with index={index}");
                var wi = DataHelper.GetWorkflowInstanceTestDataByIndex(index);
                wi.Status = Status.Succeeded;
                wi.Tasks[0].Status = TaskExecutionStatus.PartialFail;
                wi.AcknowledgedWorkflowErrors = null;

                listOfWorkflowInstance.Add(wi);
                MongoClient.CreateWorkflowInstanceDocument(wi);
                _outputHelper.WriteLine("Retrieved workflow instance");
            }
            DataHelper.SeededWorkflowInstances = listOfWorkflowInstance;
        }

        [Given(@"I have an acknowledged partially failed workflow Instances")]
        public void GivenIHavePartialFailedWorkflowInstancesWithAcknowledgedWorkflowErrors()
        {
            var listOfWorkflowInstance = new List<WorkflowInstance>();

            var wi = DataHelper.GetWorkflowInstanceTestData("Workflow_Instance_For_Failed_Partial");
            wi.Status = Status.Succeeded;
            wi.AcknowledgedWorkflowErrors = DateTime.UtcNow;
            wi.Tasks[0].Status = TaskExecutionStatus.PartialFail;
            listOfWorkflowInstance.Add(wi);
            MongoClient.CreateWorkflowInstanceDocument(wi);
            DataHelper.SeededWorkflowInstances = listOfWorkflowInstance;
        }

        [Then(@"I can see ([1-9]*) Workflow Instances are created")]
        [Then(@"I can see ([0-9]*) Workflow Instance is created")]
        public void ThenICanSeeAWorkflowInstanceIsCreated(int count)
        {
            _outputHelper.WriteLine($"Retrieving {count} workflow instance/s using the payloadid={DataHelper.WorkflowRequestMessage.PayloadId.ToString()}");
            var workflowInstances = DataHelper.GetWorkflowInstances(count, DataHelper.WorkflowRequestMessage.PayloadId.ToString());
            _outputHelper.WriteLine($"Retrieved {count} workflow instance/s");

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

        [Then(@"I can see no Workflow Instances are created")]
        public void ThenICanSeeNoWorkflowInstanceIsCreated()
        {
            for (int i = 0; i < 10; i++)
            {
                _outputHelper.WriteLine($"Trying to Retreive 0 workflow instance/s using the payloadid={DataHelper.WorkflowRequestMessage.PayloadId.ToString()}");
                var workflowInstances = DataHelper.GetWorkflowInstances(0, DataHelper.WorkflowRequestMessage.PayloadId.ToString());
                _outputHelper.WriteLine($"Retrieved {workflowInstances.Count} workflow instance/s");
                Thread.Sleep(300);
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
                        if (DataHelper.TaskUpdateEvent != null)
                        {
                            var workflowInstanceTask = workflowInstance.Tasks.FirstOrDefault(x => x.TaskId.Equals(DataHelper.TaskUpdateEvent.TaskId));
                            if (workflowInstanceTask != null)
                            {
                                workflowInstanceTask.Status.Should().Be(DataHelper.TaskUpdateEvent.Status);
                                Assertions.AssertOutputArtifactsForTaskUpdate(workflowInstanceTask.OutputArtifacts, DataHelper.TaskUpdateEvent.Outputs);
                            }
                        }
                    }
                }
            });
        }

        [Then(@"I can see Workflow Instance is updated with Task Update Information")]
        public void ThenICanSeeAWorkflowInstanceIsUpdatedWithTaskUpdateInformation()
        {
            RetryPolicy.Execute(() =>
            {
                _outputHelper.WriteLine($"Retrieving workflow instance using workflowInstanceId={DataHelper.TaskUpdateEvent.WorkflowInstanceId}");
                var workflowInstance = DataHelper.GetAllWorkflowInstance(DataHelper.TaskUpdateEvent.WorkflowInstanceId);

                if (workflowInstance == null)
                {
                    throw new Exception($"Workflow Instance not found using workflowInstanceId={DataHelper.TaskUpdateEvent.WorkflowInstanceId}");
                }

                _outputHelper.WriteLine($"Retrieved workflow instance");

                Assertions.AssertWorkflowInstanceAfterTaskUpdate(workflowInstance, DataHelper.TaskUpdateEvent);
            });
        }

        [Then(@"I can see Workflow Instance is updated with Task Dispatch Information")]
        public void ThenICanSeeAWorkflowInstanceIsUpdatedWithTaskDispatchInformation()
        {
            foreach (var taskDispatch in DataHelper.TaskDispatchEvents)
            {
                RetryPolicy.Execute(() =>
                {
                    _outputHelper.WriteLine($"Retrieving workflow instance using workflowInstanceId={taskDispatch.WorkflowInstanceId}");
                    var workflowInstance = DataHelper.GetAllWorkflowInstance(taskDispatch.WorkflowInstanceId);
                    var workflowRevision = DataHelper.GetWorkflowRevision(workflowInstance.WorkflowId);

                    if (workflowInstance == null)
                    {
                        throw new Exception($"Workflow Instance not found using workflowInstanceId={taskDispatch.WorkflowInstanceId}");
                    }

                    _outputHelper.WriteLine($"Retrieved workflow instance");

                    Assertions.AssertWorkflowInstanceAfterTaskDispatch(workflowInstance, taskDispatch, workflowRevision[0]);
                });
            }
        }

        [Then(@"I can see an additional Workflow Instance is not created")]
        public void ThenICanSeeAnAdditionalWorkflowInstanceIsNotCreated()
        {
            _outputHelper.WriteLine($"Retrieving workflow instance with payloadid={DataHelper.WorkflowRequestMessage.PayloadId}");
            var workflowInstances = MongoClient.GetWorkflowInstancesByPayloadId(DataHelper.WorkflowRequestMessage.PayloadId.ToString());
            _outputHelper.WriteLine("Retrieved workflow instance");

            workflowInstances.Count.Should().Be(1);
        }
    }
}
