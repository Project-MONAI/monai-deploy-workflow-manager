using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using BoDi;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowAPIStepDefinitions
    {
        public WorkflowAPIStepDefinitions(ObjectContainer objectContainer)
        {
            var httpClient = objectContainer.Resolve<HttpClient>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            ApiHelper = new ApiHelper(httpClient);
            Assertions = new Assertions();
        }

        private ApiHelper ApiHelper { get; }
        private Assertions Assertions { get; }
        private DataHelper DataHelper { get; }

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

        [Given(@"I have a body (.*)")]
        public void GivenIHaveABody(string name)
        {
            Support.HttpRequestMessageExtensions.AddJsonBody(ApiHelper.Request, DataHelper.GetWorkflowObjectTestData(name));
        }

        [Then(@"the ID (.*) is returned")]
        public void ThenTheIDIsReturned(string id)
        {
            ApiHelper.Response.Content.ReadAsStringAsync().Result.Should().Be($"{{\"workflow_id\":\"{id}\"}}");
        }

        [Then(@"I will recieve the error message (.*)")]
        public void ThenIWillRecieveTheCorrectErrorMessage(string message)
        {
            ApiHelper.Response.Content.ReadAsStringAsync().Result.Should().Contain(message);
        }

        [Then(@"a new revision is created")]
        public void ThenANewRevisionIsCreated()
        {
            return;
        }

    }
}
