using System.Net;
using BoDi;
using FluentAssertions;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class ApiStepDefinitions
    {
        public ApiStepDefinitions(ObjectContainer objectContainer)
        {
            var httpClient = objectContainer.Resolve<HttpClient>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            ApiHelper = new ApiHelper(httpClient);
            Assertions = new Assertions();
        }

        private ApiHelper ApiHelper { get; }
        private Assertions Assertions { get; }
        private DataHelper DataHelper { get; }
        private MongoClientUtil MongoClient { get; }

        [Given(@"I have an endpoint (.*)")]
        public void GivenIHaveAnEndpoint(string endpoint) => ApiHelper.SetUrl(new Uri(TestExecutionConfig.ApiConfig.BaseUrl + endpoint));

        [When(@"I send a (.*) request")]
        public void WhenISendARequest(string verb)
        {
            ApiHelper.SetRequestVerb(verb);
            _ = ApiHelper.GetResponseAsync().Result;
        }

        [Then(@"I will get a (.*) response")]
        public void ThenIWillGetAResponse(string expectedCode)
        {
            ApiHelper.Response.StatusCode.Should().Be((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), expectedCode));
        }

        [Then(@"I can see (.*) workflows are returned")]
        [Then(@"I can see (.*) workflow is returned")]
        public void ThenICanSeeWorkflowsAreReturned(int count)
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var workflowRevisions = JsonConvert.DeserializeObject<List<WorkflowRevision>>(result);
            Assertions.AssertWorkflowList(DataHelper.WorkflowRevisions, workflowRevisions);
        }

        [Then(@"I can see expected workflow instances are returned")]
        public void ThenICanSeeExpectedWorkflowInstancesAreReturned()
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var actualWorkflowInstances = JsonConvert.DeserializeObject<List<WorkflowInstance>>(result);
            Assertions.AssertWorkflowInstanceList(DataHelper.WorkflowInstances, actualWorkflowInstances);
        }

        [Then(@"I can see expected workflow instance is returned")]
        public void ThenICanSeeExpectedWorkflowInstanceIsReturned()
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var actualWorkflowInstance = JsonConvert.DeserializeObject<WorkflowInstance>(result);
            Assertions.AssertWorkflowInstance(DataHelper.WorkflowInstances, actualWorkflowInstance);
        }

        [Scope(Tag = "WorkflowInstanceApi")]
        [AfterScenario(Order = 1)]
        public void DeleteTestData()
        {
            if (DataHelper.WorkflowInstances.Count > 0)
            {
                foreach (var workflowInstance in DataHelper.WorkflowInstances)
                {
                    MongoClient.DeleteWorkflowInstance(workflowInstance.Id);
                }
            }
        }
    }
}
