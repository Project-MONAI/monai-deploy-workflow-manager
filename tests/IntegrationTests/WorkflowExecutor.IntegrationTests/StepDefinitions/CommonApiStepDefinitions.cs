/*
 * Copyright 2022 MONAI Consortium
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

using System.Net;
using BoDi;
using Monai.Deploy.WorkflowManager.HealthChecks;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class HealthCheckStepDefinitions
    {
        public HealthCheckStepDefinitions(ObjectContainer objectContainer)
        {
            ApiHelper = objectContainer.Resolve<ApiHelper>();
        }

        public ApiHelper ApiHelper { get; }

        [Then(@"I will get a health check response status message (.*) ")]
        public void ThenIWillGetAHealthCheckResponseMessage(string expectedMessage)
        {
            var contentMessage = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<HealthCheckResponse>(contentMessage);

            result.Should().NotBeNull();
            result?.Status.Should().Be(expectedMessage);
        }

        [Then(@"I will get a status message (.*)")]
        public void ThenIWillGetAMessage(string expectedMessage)
        {
            ApiHelper.Response.Content.ReadAsStringAsync().Result.Should().Be(expectedMessage);
        }
    }

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

        [Then(@"I will get a health check response status message (.*)")]
        public void ThenIWillGetAHealthCheckResponseMessage(string expectedMessage)
        {
            var contentMessage = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<HealthCheckResponse>(contentMessage);

            result.Should().NotBeNull();
            result?.Status.Should().Be(expectedMessage);
        }

        [When(@"I have a body (.*)")]
        [Given(@"I have a body (.*)")]
        public void GivenIHaveABody(string name)
        {
            Support.HttpRequestMessageExtensions.AddJsonBody(ApiHelper.Request, DataHelper.GetWorkflowObjectTestData(name));
        }

        [When(@"I have a Task Request body (.*)")]
        [Given(@"I have a Task Request body (.*)")]
        public void GivenIHaveTaskRequestBody(string name)
        {
            Support.HttpRequestMessageExtensions.AddJsonBody(ApiHelper.Request, DataHelper.GetTaskRequestTestData(name));
        }

        [Then(@"I will recieve the error message (.*)")]
        public void ThenIWillRecieveTheCorrectErrorMessage(string message)
        {
            ApiHelper.Response.Content.ReadAsStringAsync().Result.Should().Contain(message);
        }
    }
}
