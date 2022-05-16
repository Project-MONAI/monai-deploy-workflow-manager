// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using BoDi;
using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.IntegrationTests.TestData;
using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TaskStatusUpdateStepDefinitions
    {
        private MongoClientUtil MongoClient { get; set; }
        private Assertions Assertions { get; set; }
        private RabbitPublisher TaskUpdatePublisher { get; set; }
        private ScenarioContext ScenarioContext { get; set; }
        private TaskUpdateEvent TaskUpdateEvent { get; set; }
        private WorkflowInstance WorkflowInstance { get; set; }

        public TaskStatusUpdateStepDefinitions(ObjectContainer objectContainer, ScenarioContext scenarioContext)
        {
            TaskUpdatePublisher = objectContainer.Resolve<RabbitPublisher>("TaskUpdatePublisher");
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            Assertions = new Assertions();
            ScenarioContext = scenarioContext;
        }

        [When(@"I publish a Task Update Message (.*) with status (.*)")]
        public void WhenIPublishATaskUpdateMessageTaskUpdateMessage(string name, string updateStatus)
        {
            var taskUpdateTestData = TaskUpdatesTestData.TestData.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (taskUpdateTestData != null && taskUpdateTestData.TaskUpdateEvent != null)
            {
                if (!taskUpdateTestData.Name.Contains("Missing_Status"))
                {
                    taskUpdateTestData.TaskUpdateEvent.Status = updateStatus.ToLower() switch
                    {
                        "accepted" => TaskExecutionStatus.Accepted,
                        "succeeded" => TaskExecutionStatus.Succeeded,
                        "failed" => TaskExecutionStatus.Failed,
                        "canceled" => TaskExecutionStatus.Canceled,
                        _ => throw new Exception($"updateStatus {updateStatus} is not recognised. Please check and try again."),
                    };
                }

                TaskUpdateEvent = taskUpdateTestData.TaskUpdateEvent;

                var message = new JsonMessage<TaskUpdateEvent>(
                    TaskUpdateEvent,
                    "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                    Guid.NewGuid().ToString(),
                    string.Empty);

                TaskUpdatePublisher.PublishMessage(message.ToMessage());
            }
            else
            {
                throw new Exception($"Task update message not found for {name}");
            }
        }

        [Then(@"I can see the status of the Task is updated")]
        public void ThenICanSeeTheStatusOfTheTaskIsUpdated()
        {
            WorkflowInstance = (WorkflowInstance)ScenarioContext["OriginalWorkflowInstance"];

            for (int i = 0; i < 10; i++)
            {
                var updatedWorkflowInstance = MongoClient.GetWorkflowInstanceById(WorkflowInstance.Id);

                try
                {
                    updatedWorkflowInstance.Tasks[0].Status.Should().Be(TaskUpdateEvent.Status);
                    break;
                }
                catch (Exception e)
                {
                    Console.Write($"Task status for workflow instance does not match {TaskUpdateEvent.Status}. Trying again");

                    Thread.Sleep(1000);

                    if (i == 9)
                    {
                        throw e;
                    }
                }
            }
        }

        [Then(@"I can see the status of the Task is not updated")]
        public void ThenICanSeeTheStatusOfTheTaskIsNotUpdated()
        {
            WorkflowInstance = (WorkflowInstance)ScenarioContext["OriginalWorkflowInstance"];

            for (int i = 0; i < 3; i++)
            {
                var updatedWorkflowInstance = MongoClient.GetWorkflowInstanceById(WorkflowInstance.Id);

                updatedWorkflowInstance.Tasks[0].Status.Should().Be(WorkflowInstance.Tasks[0].Status);

                Thread.Sleep(1000);
            }
        }

        [Scope(Tag = "TaskUpdate")]
        [AfterScenario(Order = 1)]
        public void DeleteTestData()
        {
            if (WorkflowInstance != null)
            {
                MongoClient.DeleteWorkflowInstance(WorkflowInstance.Id);
            }
        }

    }
}
