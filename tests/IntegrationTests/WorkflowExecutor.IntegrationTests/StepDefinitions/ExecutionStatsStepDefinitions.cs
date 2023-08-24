/*
 * Copyright 2023 MONAI Consortium
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
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData;
using Polly;
using Polly.Retry;
using Snapshooter.NUnit;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.StepDefinitions
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
        public ExecutionStatsStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            Assertions = new Assertions(objectContainer, outputHelper);
            _outputHelper = outputHelper;
            RetryExecutionStats = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1000));
            ApiHelper = objectContainer.Resolve<ApiHelper>();
        }

        [Given(@"Execution Stats table is populated with (.*)")]
        public void GivenTheExecutionStatsIsPopulatedWith(List<String> names)
        {
            foreach (var name in names)
            {
                _outputHelper.WriteLine($"Creating ExecutionStats with name={name}");
                var executionStat = DataHelper.GetExecutionStatsTestData(name);
                MongoClient.CreateExecutionStats(executionStat);
            }
        }

        [Given(@"Execution Stats table is populated")]
        public void GivenTheExecutionStatsIsPopulated()
        {
            foreach (var testData in ExecutionStatsTestData.TestData)
            {
                _outputHelper.WriteLine($"Creating ExecutionStats with name={testData.Name}");
                var executionStat = DataHelper.GetExecutionStatsTestData(testData.Name);
                MongoClient.CreateExecutionStats(executionStat);
            }
        }

        [Given(@"the Execution Stats table is populated correctly for a (.*)")]
        [Then(@"the Execution Stats table is populated correctly for a (.*)")]
        public void ThenTheExecutionStatsTableIsPopulatedCorrectly(string type)
        {
            if (type.Equals("TaskDispatchEvent", StringComparison.OrdinalIgnoreCase))
            {
                _outputHelper.WriteLine($"Retrieving Execution Stats for Task Dispatch {DataHelper.TaskDispatchEvent.ExecutionId}");
                RetryExecutionStats.Execute(() =>
                {
                    var executionStats = MongoClient.GetExecutionStatsByExecutionId(DataHelper.TaskDispatchEvent.ExecutionId);
                    if (executionStats.Count > 0)
                    {
                        Assertions.AssertExecutionStats(executionStats[0], DataHelper.TaskDispatchEvent);
                    }
                    else
                    {
                        throw new Exception("No execution stats found!");
                    }
                });
            }
            else if (type.Equals("TaskCallbackEvent", StringComparison.OrdinalIgnoreCase))
            {
                _outputHelper.WriteLine($"Retrieving Execution Stats for Task Callback {DataHelper.TaskCallbackEvent.ExecutionId}");
                RetryExecutionStats.Execute(() =>
                {
                    var executionStats = MongoClient.GetExecutionStatsByExecutionId(DataHelper.TaskCallbackEvent.ExecutionId);
                    Assertions.AssertExecutionStats(executionStats[0], null, DataHelper.TaskCallbackEvent);
                });
            }
            else
            {
                throw new Exception($"{type} is not supported. Please review test");
            }
        }

        [Given(@"I set the start time to be UTC (.*)")]
        public void GivenISetTheStartTimeToBeUTC(int days)
        {
            var startTime = DateTime.UtcNow.AddDays(days);
            var endTime = DateTime.UtcNow.AddDays(days).AddDays(10);

            var dict = new Dictionary<string, string>();
            dict.Add("starttime", startTime.ToString("yyyy-MM-ddTHH:mm:ssK"));
            dict.Add("endtime", endTime.ToString("yyyy-MM-ddTHH:mm:ssK"));
            ApiHelper.AddQueryParams(dict);
        }

        [Given(@"I set WorkflowId as (.*) and TaskId as (.*)")]
        public void GivenISetWorkflowIdAsWorkflow_AndTaskIdAsTask_(string workflowId, string taskId)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("workflowid", workflowId);
            dict.Add("taskid", taskId);
            ApiHelper.AddQueryParams(dict);
        }

        [Then(@"I can see expected summary execution stats are returned")]
        public void ThenICanSeeExpectedSummaryExecutionStatsAreReturned()
        {
            Snapshot.Match(DataHelper.FormatResponse(ApiHelper.Response?.Content.ReadAsStringAsync().Result), matchOptions
                => matchOptions.IgnoreField("periodStart").IgnoreField("periodEnd"));
        }

        [Then(@"I can see expected execution stats are returned")]
        public void ThenICanSeeExpectedExecutionStatsAreReturned()
        {
            Snapshot.Match(DataHelper.FormatResponse(ApiHelper.Response?.Content.ReadAsStringAsync().Result), matchOptions =>
            matchOptions.IgnoreAllFields("startedAt").IgnoreAllFields("finishedAt").IgnoreField("periodStart").IgnoreField("periodEnd"));
        }

        [StepArgumentTransformation]
        private List<String> TransformToListOfString(string commaSeparatedList)
        {
            return commaSeparatedList.Split(",").Select(t => t.Trim()).ToList();
        }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
