using BoDi;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Polly;
using Polly.Retry;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
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
            Assertions = new Assertions();
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
                _outputHelper.WriteLine($"Retrieving workflow instance by id={DataHelper.TaskUpdateEvent.WorkflowInstanceId}");
                var workflowInstance = MongoClient.GetWorkflowInstanceById(DataHelper.TaskUpdateEvent.WorkflowInstanceId);
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
    }
}
