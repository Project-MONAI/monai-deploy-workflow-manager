// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using BoDi;
using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TaskStatusUpdateStepDefinitions
    {
        private MongoClientUtil MongoClient { get; set; }
        private RabbitPublisher TaskUpdatePublisher { get; set; }
        private RetryPolicy RetryPolicy { get; set; }
        private DataHelper DataHelper { get; set; }

        public TaskStatusUpdateStepDefinitions(ObjectContainer objectContainer)
        {
            TaskUpdatePublisher = objectContainer.Resolve<RabbitPublisher>("TaskUpdatePublisher");
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            RetryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
        }

        [When(@"I publish a Task Update Message (.*) with status (.*)")]
        public void WhenIPublishATaskUpdateMessageTaskUpdateMessage(string name, string updateStatus)
        {
            var message = new JsonMessage<TaskUpdateEvent>(
                DataHelper.GetTaskUpdateTestData(name, updateStatus),
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                Guid.NewGuid().ToString(),
                string.Empty);

            TaskUpdatePublisher.PublishMessage(message.ToMessage());
        }

        [Then(@"I can see the status of the Tasks are updated")]
        [Then(@"I can see the status of the Task is updated")]
        public void ThenICanSeeTheStatusOfTheTaskIsUpdated()
        {
            RetryPolicy.Execute(() =>
            {
                var workflowInstance = MongoClient.GetWorkflowInstanceById(DataHelper.TaskUpdateEvent.WorkflowInstanceId);

                var taskUpdated = workflowInstance.Tasks.FirstOrDefault(x => x.TaskId.Equals(DataHelper.TaskUpdateEvent.TaskId));

                taskUpdated.Status.Should().Be(DataHelper.TaskUpdateEvent.Status);

                if (DataHelper.TaskDispatchEvents.Count > 0)
                {
                    foreach (var e in DataHelper.TaskDispatchEvents)
                    {
                        var taskDispatched = workflowInstance.Tasks.FirstOrDefault(x => x.TaskId.Equals(e.TaskId));

                        taskDispatched.Status.Should().Be(TaskExecutionStatus.Dispatched);
                    }
                }
            });
        }

        [Then(@"I can see the status of the Task is not updated")]
        public void ThenICanSeeTheStatusOfTheTaskIsNotUpdated()
        {
            for (int i = 0; i < 2; i++)
            {
                var updatedWorkflowInstance = MongoClient.GetWorkflowInstanceById(DataHelper.TaskUpdateEvent.WorkflowInstanceId);

                var orignalWorkflowInstance = DataHelper.WorkflowInstances.FirstOrDefault(x => x.Id.Equals(DataHelper.TaskUpdateEvent.WorkflowInstanceId));

                updatedWorkflowInstance.Tasks[0].Status.Should().Be(orignalWorkflowInstance.Tasks[0].Status);

                Thread.Sleep(1000);
            }
        }

        //[Scope(Tag = "TaskUpdate")]
        //[AfterScenario(Order = 1)]
        //public void DeleteTestData()
        //{
        //    if (DataHelper.WorkflowRevisions.Count > 0)
        //    {
        //        foreach (var workflowRevision in DataHelper.WorkflowRevisions)
        //        {
        //            MongoClient.DeleteWorkflowRevisionDocument(workflowRevision.Id);
        //        }
        //    }

        //    if (DataHelper.WorkflowInstances.Count > 0)
        //    {
        //        foreach (var workflowInstance in DataHelper.WorkflowInstances)
        //        {
        //            MongoClient.DeleteWorkflowInstance(workflowInstance.Id);
        //        }
        //    }
        //}

    }
}
