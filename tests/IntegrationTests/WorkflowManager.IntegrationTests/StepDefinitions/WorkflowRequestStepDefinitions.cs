// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using BoDi;
using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.IntegrationTests.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;

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

        public WorkflowRequestStepDefinitions(ObjectContainer objectContainer, ScenarioContext scenarioContext)
        {
            WorkflowPublisher = objectContainer.Resolve<RabbitPublisher>("WorkflowPublisher");
            TaskDispatchConsumer = objectContainer.Resolve<RabbitConsumer>("TaskDispatchConsumer");
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            Assertions = new Assertions();
            DataHelper = objectContainer.Resolve<DataHelper>();
        }

        [Given(@"I have a clinical workflow (.*)")]
        public void GivenIHaveAClinicalWorkflow(string name)
        {
            MongoClient.CreateWorkflowRevisionDocument(DataHelper.GetWorkflowRevisionTestData(name));
        }

        [Given(@"I have a Workflow Instance (.*)")]
        public void GivenIHaveAWorkflowInstance(string name)
        {
            MongoClient.CreateWorkflowInstanceDocument(DataHelper.GetWorkflowInstanceTestData(name));
        }

        [When(@"I publish a Workflow Request Message (.*)")]
        public void WhenIPublishAWorkflowRequestMessage(string name)
        {
            var message = new JsonMessage<WorkflowRequestMessage>(
                DataHelper.GetWorkflowRequestTestData(name),
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                Guid.NewGuid().ToString(),
                string.Empty);

            WorkflowPublisher.PublishMessage(message.ToMessage());
        }

        [Then(@"I can see (.*) Workflow Instances are created")]
        [Then(@"I can see (.*) Workflow Instance is created")]
        public void ThenICanSeeAWorkflowInstanceIsCreated(int count)
        {
            var workflowInstances = DataHelper.GetWorkflowInstances(count, DataHelper.WorkflowRequestMessage.PayloadId.ToString());

            if (workflowInstances != null)
            {
                foreach (var workflowInstance in workflowInstances)
                {
                    var workflow = DataHelper.WorkflowRevisions.FirstOrDefault(x => x.WorkflowId.Equals(workflowInstance.WorkflowId));

                    if (workflow != null)
                    {
                        Assertions.AssertWorkflowInstanceMatchesExpectedWorkflow(workflowInstance, workflow, DataHelper.WorkflowRequestMessage);
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
            var taskDispatchEvents = DataHelper.GetTaskDispatchEvents(count, DataHelper.WorkflowInstances);

            foreach (var taskDispatchEvent in taskDispatchEvents)
            {
                var workflowInstance = MongoClient.GetWorkflowInstanceById(taskDispatchEvent.WorkflowInstanceId);

                var workflow = DataHelper.WorkflowRevisions.FirstOrDefault(x => x.WorkflowId.Equals(workflowInstance.WorkflowId));

                if (string.IsNullOrEmpty(DataHelper.TaskUpdateEvent.ExecutionId))
                {
                    Assertions.AssertTaskDispatchEvent(taskDispatchEvent, workflowInstance, workflow, DataHelper.WorkflowRequestMessage);
                }
                else
                {
                    Assertions.AssertTaskDispatchEvent(taskDispatchEvent, workflowInstance, workflow, null, DataHelper.TaskUpdateEvent);
                }
            }
        }

        [Then(@"I can see an additional Workflow Instance is not created")]
        public void ThenICanSeeAnAdditionalWorkflowInstanceIsNotCreated()
        {
            var workflowInstances = MongoClient.GetWorkflowInstancesByPayloadId(DataHelper.WorkflowRequestMessage.PayloadId.ToString());

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

                    if (taskDispatchEvent.ExecutionId == workflowInstance.Tasks[0].ExecutionId)
                    {
                        throw new Exception($"Task Dispatch Event has been published when workflowInstance status was {workflowInstance.Tasks[0].Status}");
                    }
                }

                Thread.Sleep(1000);
            }
        }

        [Scope(Tag = "WorkflowRequest")]
        [AfterScenario(Order = 1)]
        public void DeleteTestData()
        {
            if (DataHelper.WorkflowRevisions.Count > 0)
            {
                foreach (var workflowRevision in DataHelper.WorkflowRevisions)
                {
                    MongoClient.DeleteWorkflowDocument(workflowRevision.Id);
                }
            }
        }
    }
}
