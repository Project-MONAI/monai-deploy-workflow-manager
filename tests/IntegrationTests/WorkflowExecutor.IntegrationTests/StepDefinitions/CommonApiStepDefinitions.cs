// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Net;
using BoDi;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class CommonStepDefinitions
    {
        public CommonStepDefinitions(ObjectContainer objectContainer)
        {
            ApiHelper = objectContainer.Resolve<ApiHelper>();
            DataHelper = objectContainer.Resolve<DataHelper>();
        }

        private ApiHelper ApiHelper { get; }

        private DataHelper DataHelper { get; }

        [Given(@"I have an endpoint (.*)")]
        public void GivenIHaveAnEndpoint(string endpoint) => ApiHelper.SetUrl(new Uri(TestExecutionConfig.ApiConfig.BaseUrl + endpoint));

        [Given(@"I send a (.*) request")]
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

        [When(@"I have a body (.*)")]
        [Given(@"I have a body (.*)")]
        public void GivenIHaveABody(string name)
        {
            Support.HttpRequestMessageExtensions.AddJsonBody(ApiHelper.Request, DataHelper.GetWorkflowObjectTestData(name));
        }

        [Then(@"I will recieve the error message (.*)")]
        public void ThenIWillRecieveTheCorrectErrorMessage(string message)
        {
            ApiHelper.Response.Content.ReadAsStringAsync().Result.Should().Contain(message);
        }
    }
}
