// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using BoDi;
using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.IntegrationTests.TestData;
using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowRequestStepDefinitions
    {
        private RabbitPublisher WorkflowPublisher { get; set; }
        private RabbitConsumer TaskDispatchConsumer { get; set; }
        private MongoClientUtil MongoClient { get; set; }
        private Assertions Assertions { get; set; }
        private readonly List<WorkflowRevision> _workflowRevisions = new List<WorkflowRevision>();
        private WorkflowRequestMessage WorkflowRequestMessage { get; set; }
        private List<WorkflowInstance> WorkflowInstances { get; set; }
        private ScenarioContext ScenarioContext { get; set; }

        public WorkflowRequestStepDefinitions(ObjectContainer objectContainer, ScenarioContext scenarioContext)
        {
            WorkflowPublisher = objectContainer.Resolve<RabbitPublisher>("WorkflowPublisher");
            TaskDispatchConsumer = objectContainer.Resolve<RabbitConsumer>("TaskDispatchConsumer");
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            Assertions = new Assertions();
            ScenarioContext = scenarioContext;
        }

        [Given(@"I have a clinical workflow (.*)")]
        public void GivenIHaveAClinicalWorkflow(string name)
        {
            var workflowRevision = WorkflowRevisionsTestData.TestData.FirstOrDefault(c => c.Name.Equals(name));

            if (workflowRevision != null)
            {
                if (workflowRevision.WorkflowRevision != null)
                {
                    _workflowRevisions.Add(workflowRevision.WorkflowRevision);
                    MongoClient.CreateWorkflowRevisionDocument(workflowRevision.WorkflowRevision);
                }
                else
                {
                    throw new Exception($"Workflow {name} does not have any applicable test data, please check and try again!");
                }
            }
            else
            {
                throw new Exception($"Workflow {name} does not have any applicable test data, please check and try again!");
            }

        }

        [Given(@"I have a Workflow Instance (.*)")]
        public void GivenIHaveAWorkflowInstance(string name)
        {
            var workflowInstance = WorkflowInstancesTestData.TestData.FirstOrDefault(c => c.Name.Contains(name));

            if (workflowInstance != null)
            {
                if (workflowInstance.WorkflowInstance != null)
                {
                    ScenarioContext["WorkflowInstance"] = workflowInstance.WorkflowInstance;
                    MongoClient.CreateWorkflowInstanceDocument(workflowInstance.WorkflowInstance);
                }
                else
                {
                    throw new Exception($"Workflow Intance {name} does not have any applicable test data, please check and try again!");
                }
            }
            else
            {
                throw new Exception($"Workflow Intance {name} does not have any applicable test data, please check and try again!");
            }
        }

        [When(@"I publish a Workflow Request Message (.*)")]
        public void WhenIPublishAWorkflowRequestMessage(string name)
        {
            var workflowRequest = WorkflowRequestsTestData.TestData.FirstOrDefault(c => c.Name.Contains(name));

            if (workflowRequest != null)
            {
                if (workflowRequest.WorkflowRequestMessage != null)
                {
                    WorkflowRequestMessage = workflowRequest.WorkflowRequestMessage;

                    var message = new JsonMessage<WorkflowRequestMessage>(
                        workflowRequest.WorkflowRequestMessage,
                        "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                        Guid.NewGuid().ToString(),
                        string.Empty);

                    WorkflowPublisher.PublishMessage(message.ToMessage());
                }
                else
                {
                    throw new Exception($"Workflow request {name} does not have any applicable test data, please check and try again!");
                }
            }
            else
            {
                throw new Exception($"Workflow request {name} does not have any applicable test data, please check and try again!");
            }
        }

        [Then(@"I can see (.*) Workflow Instances are created")]
        [Then(@"I can see (.*) Workflow Instance is created")]
        public void ThenICanSeeAWorkflowInstanceIsCreated(int count)
        {
            WorkflowInstances = GetWorkflowInstances(count, WorkflowRequestMessage.PayloadId.ToString());

            if (WorkflowInstances != null)
            {
                foreach (var workflowInstance in WorkflowInstances)
                {
                    var workflow = _workflowRevisions.FirstOrDefault(x => x.WorkflowId.Equals(workflowInstance.WorkflowId));

                    if (workflow != null)
                    {
                        Assertions.AssertWorkflowInstanceDetails(workflowInstance, workflow, WorkflowRequestMessage);
                    }
                    else
                    {
                        throw new Exception($"Workflow not found for workflowId {workflowInstance.WorkflowId}");
                    }
                }
            }
        }

        [Then(@"(.*) Task Dispatch event is published")]
        [Then(@"(.*) Task Dispatch events are published")]
        public void TaskDispatchEventIsPublished(int count)
        {
            if (WorkflowInstances == null)
            {
                WorkflowInstances = GetWorkflowInstances(count, WorkflowRequestMessage.PayloadId.ToString());
            }

            var taskDispatchEvents = GetTaskDispatchEvents(count, WorkflowInstances);

            foreach (var instance in WorkflowInstances)
            {
                var workflow = _workflowRevisions.FirstOrDefault(x => x.WorkflowId.Equals(instance.WorkflowId));

                if (workflow != null)
                {
                    var taskDispatchEvent = taskDispatchEvents.FirstOrDefault(x => x.ExecutionId.Equals(instance.Tasks[0].ExecutionId));

                    if (taskDispatchEvent != null)
                    {
                        var workflowInstance = MongoClient.GetWorkflowInstanceById(instance.Id);

                        Assertions.AssertTaskDispatchEvent(taskDispatchEvent, workflowInstance, workflow, WorkflowRequestMessage);
                    }
                    else
                    {
                        throw new Exception($"Task dispatch evenet for ExecutionId {instance.Tasks[0].ExecutionId} cannot be found");
                    }
                }
                else
                {
                    throw new Exception($"Workflow for instance workflowId {instance.WorkflowId} cannot be found");
                }
            }
        }

        [Then(@"I can see an additional Workflow Instance is not created")]
        public void ThenICanSeeAnAdditionalWorkflowInstanceIsNotCreated()
        {
            var workflowInstances = MongoClient.GetWorkflowInstances(WorkflowRequestMessage.PayloadId.ToString());

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
                    if (WorkflowInstances == null)
                    {
                        WorkflowInstances = GetWorkflowInstances(1, WorkflowRequestMessage.PayloadId.ToString());
                    }

                    foreach (var instance in WorkflowInstances)
                    {
                        if (taskDispatchEvent.ExecutionId == instance.Tasks[0].ExecutionId)
                        {
                            throw new Exception($"Task Dispatch Event has been published when workflowInstance status was {instance.Tasks[0].Status}");
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }


        private List<WorkflowInstance> GetWorkflowInstances(int count, string payloadId)
        {
            for (var i = 0; i < 20; i++)
            {
                var workflowInstances = MongoClient.GetWorkflowInstances(payloadId);

                if (workflowInstances.Count == count)
                {
                    return workflowInstances;
                }

                if (i == 9)
                {
                    throw new Exception($"{count} workflow instances could not be found for payloadId {payloadId}");
                }

                Thread.Sleep(1000);
            }

            throw new Exception($"{count} workflow instances could not be found for payloadId {payloadId}");
        }

        private List<TaskDispatchEvent> GetTaskDispatchEvents(int count, List<WorkflowInstance> workflowInstances)
        {
            var taskDispatchEvent = new List<TaskDispatchEvent>();

            for (var i = 0; i < 10; i++)
            {
                var message = TaskDispatchConsumer.GetMessage<TaskDispatchEvent>();

                if (message != null)
                {
                    foreach (var workflowInstance in workflowInstances)
                    {
                        if (message.ExecutionId == workflowInstance.Tasks[0].ExecutionId)
                        {
                            taskDispatchEvent.Add(message);
                        }
                    }
                }

                if (taskDispatchEvent.Count == count)
                {
                    return taskDispatchEvent;
                }

                if (i == 9)
                {
                    throw new Exception($"{count} task dispatch events could not be found");
                }

                Thread.Sleep(1000);
            }

            throw new Exception($"{count} task dispatch events could not be found");
        }

        [AfterScenario(Order = 1)]
        public void DeleteWorkflows()
        {
            foreach (var workflow in _workflowRevisions)
            {
                MongoClient.DeleteWorkflowDocument(workflow.WorkflowId);
            }

            _workflowRevisions.Clear();

            if (WorkflowInstances != null)
            {
                WorkflowInstances.Clear();
            }
        }
    }
}
