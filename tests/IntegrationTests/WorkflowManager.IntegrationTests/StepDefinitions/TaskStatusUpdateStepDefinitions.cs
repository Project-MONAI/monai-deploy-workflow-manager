// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using BoDi;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TaskStatusUpdateStepDefinitions
    {
        private MongoClientUtil MongoClient { get; set; }
        private Assertions Assertions { get; set; }
        private RabbitPublisher TaskUpdatePublisher { get; set; }
        private ScenarioContext ScenarioContext { get; set; }

        public TaskStatusUpdateStepDefinitions(ObjectContainer objectContainer, ScenarioContext scenarioContext)
        {
            TaskUpdatePublisher = objectContainer.Resolve<RabbitPublisher>("TaskUpdatePublisher");
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            Assertions = new Assertions();
            ScenarioContext = scenarioContext;
        }

        [When(@"I publish a Task Update Message <taskUpdateMessage>")]
        public void WhenIPublishATaskUpdateMessageTaskUpdateMessage()
        {
            throw new PendingStepException();
        }

        [Then(@"I can see the status of the Task is updated")]
        public void ThenICanSeeTheStatusOfTheTaskIsUpdated()
        {
            var workflowIntsance = ScenarioContext["WorkflowInstance"];
            throw new PendingStepException();
        }
    }
}
