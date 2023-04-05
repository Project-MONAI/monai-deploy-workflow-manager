using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class ExecutionStatsStepDefinitions
    {

        private DataHelper DataHelper { get; set; }
        private readonly ISpecFlowOutputHelper _outputHelper;
        private MongoClientUtil MongoClient { get; set; }
        private Assertions Assertions { get; set; }
        private RetryPolicy RetryExecutionStats { get; set; }
        private ApiHelper ApiHelper { get; set; }

        public ExecutionStatsStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            Assertions = new Assertions(outputHelper);
            _outputHelper = outputHelper;
            RetryExecutionStats = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1000));
            ApiHelper = objectContainer.Resolve<ApiHelper>();
        }

        [Given(@"Execution Stats table is populated with (.*)")]
        public void GivenTheExecutionStatsIsPopulatedWith(List<String> names)
        {
            foreach (var name in names)
            {
                _outputHelper.WriteLine($"Creating TaskExecutionStats with name={name}");
                var executionStat = DataHelper.GetExecutionStatsTestData(name);
                MongoClient.CreateExecutionStats(executionStat);
            }
        }

        [Then(@"the Execution Stats table is populated correctly for a (.*)")]
        public void ThenTheExecutionStatsTableIsPopulatedCorrectly(string type)
        {
            if (type.Equals("TaskDispatchEvent", StringComparison.OrdinalIgnoreCase))
            {
                _outputHelper.WriteLine($"Retrieving Execution Stats for Task Dispatch {DataHelper.TaskDispatchEvent.ExecutionId}");
                RetryExecutionStats.Execute(() =>
                {
                    var executionStats = MongoClient.GetExecutionStatsByExecutionId(DataHelper.TaskDispatchEvent.ExecutionId);
                    Assertions.AssertExecutionStats(executionStats, DataHelper.TaskDispatchEvent);
                });
            }
            else if (type.Equals("TaskCallbackEvent", StringComparison.OrdinalIgnoreCase))
            {
                _outputHelper.WriteLine($"Retrieving Execution Stats for Task Callback {DataHelper.TaskCallbackEvent.ExecutionId}");
                RetryExecutionStats.Execute(() =>
                {
                    var executionStats = MongoClient.GetExecutionStatsByExecutionId(DataHelper.TaskDispatchEvent.ExecutionId);
                    Assertions.AssertExecutionStats(executionStats, null, DataHelper.TaskCallbackEvent);
                });
            }
            else
            {
                throw new Exception($"{type} is not supported. Please review test");
            }
        }

        [Then(@"I can see expected summary execution stats are returned")]
        public void ThenICanSeeExpectedSummaryExecutionStatsAreReturned()
        {
            VerifyJson(ApiHelper.Response?.Content.ReadAsStringAsync().Result);
        }

        [Then(@"I can see expected execution stats are returned")]
        public void ThenICanSeeExpectedExecutionStatsAreReturned()
        {
            VerifyJson(ApiHelper.Response?.Content.ReadAsStringAsync().Result);
        }

        [StepArgumentTransformation]
        private List<String> TransformToListOfString(string commaSeparatedList)
        {
            return commaSeparatedList.Split(",").ToList();
        }
    }
}
