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
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support;
using Newtonsoft.Json;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.StepDefinitions
{
    [Binding]
    public class CommonStepDefinitions
    {
        public CommonStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            ApiHelper = objectContainer.Resolve<ApiHelper>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            _outputHelper = outputHelper;
        }

        private ApiHelper ApiHelper { get; }

        private DataHelper DataHelper { get; }

        private MongoClientUtil MongoClient { get; }

        private readonly ISpecFlowOutputHelper _outputHelper;

        [Given(@"I have an endpoint (.*)")]
        public void GivenIHaveAnEndpoint(string endpoint)
        {
            var apiUri = new Uri(TestExecutionConfig.ApiConfig.BaseUrl + endpoint);
            ApiHelper.SetUrl(apiUri);
            _outputHelper.WriteLine($"API Url set to {apiUri}");
        }

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
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;

            ApiHelper.Response.StatusCode.Should().Be((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), expectedCode));
        }

        [When(@"I have a workflow body (.*)")]
        [Given(@"I have a workflow body (.*)")]
        public void GivenIHaveAWorkflowBody(string name)
        {
            var body = DataHelper.GetWorkflowObjectTestData(name);
            Support.HttpRequestMessageExtensions.AddJsonBody(ApiHelper.Request, body);
        }

        [When(@"I have a body (.*)")]
        [Given(@"I have a body (.*)")]
        public void GivenIHaveABody(string name)
        {
            WorkflowUpdateRequest body = new();
            body.Workflow = DataHelper.GetWorkflowObjectTestData(name);
            body.OriginalWorkflowName = body.Workflow.Name;
            Support.HttpRequestMessageExtensions.AddJsonBody(ApiHelper.Request, body);
        }

        [When(@"I have a Task Request body (.*)")]
        [Given(@"I have a Task Request body (.*)")]
        public void GivenIHaveTaskRequestBody(string name)
        {
            Support.HttpRequestMessageExtensions.AddJsonBody(ApiHelper.Request, DataHelper.GetTaskRequestTestData(name));
        }

        [Then(@"I will receive the error message (.*)")]
        public void ThenIWillReceiveTheCorrectErrorMessage(string message)
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            result.Should().ContainAll("type", "title", "status", "traceId");
            result.Should().Contain(message);
        }

        [Then(@"I will get a health check response status message (.*)")]
        public async Task ThenIWillGetAHealthCheckResponseMessage(string expectedMessage)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var contentMessage = await ApiHelper.Response?.Content.ReadAsStringAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            contentMessage.Should().NotBeNull();
            var response = JsonConvert.DeserializeObject<HealthCheckResponse>(contentMessage);
            response.Should().NotBeNull();
            response!.Status.Should().Be(expectedMessage);
            response!.Checks.Should().ContainEquivalentOf<Component>(new Component { Check = "minio", Result = expectedMessage });
            response!.Checks.Should().ContainEquivalentOf<Component>(new Component { Check = "Rabbit MQ Publisher", Result = expectedMessage });
            response!.Checks.Should().ContainEquivalentOf<Component>(new Component { Check = "Rabbit MQ Subscriber", Result = expectedMessage });
            response!.Checks.Should().ContainEquivalentOf<Component>(new Component { Check = "Workflow Manager Services", Result = expectedMessage });
            response!.Checks.Should().ContainEquivalentOf<Component>(new Component { Check = "mongodb", Result = expectedMessage });
        }
    }
}
