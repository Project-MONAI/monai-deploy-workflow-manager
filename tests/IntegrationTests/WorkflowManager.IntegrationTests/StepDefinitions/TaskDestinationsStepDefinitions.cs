using BoDi;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TaskDestinationsStepDefinitions
    {
        private MongoClientUtil MongoClient { get; set; }
        private RetryPolicy RetryPolicy { get; set; }
        private DataHelper DataHelper { get; set; }
        private Assertions Assertions { get; set; }

        public TaskDestinationsStepDefinitions(ObjectContainer objectContainer)
        {
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            RetryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
            Assertions = new Assertions();
        }

        [Then(@"Workflow Instance is updated with the new Tasks")]
        [Then(@"Workflow Instance is updated with the new Task")]
        public void ThenWorkflowInstanceIsUpdatedWithTheNewTask()
        {
            RetryPolicy.Execute(() =>
            {
                var workflowInstance = MongoClient.GetWorkflowInstanceById(DataHelper.TaskUpdateEvent.WorkflowInstanceId);

                Assertions.WorkflowInstanceIncludesTaskDetails(DataHelper.TaskDispatchEvents, workflowInstance, DataHelper.WorkflowRevisions[0]);
            });
        }

        [Then(@"Workflow Instance status is (.*)")]
        public void ThenWorkflowInstanceStatusIs(string status)
        {
            RetryPolicy.Execute(() =>
            {
                var workflowInstance = MongoClient.GetWorkflowInstanceById(DataHelper.TaskUpdateEvent.WorkflowInstanceId);

                Assertions.WorkflowInstanceStatus(status, workflowInstance);
            });
        }
    }
}
