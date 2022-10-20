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
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using MongoDB.Driver;
using Newtonsoft.Json;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
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
            ApiHelper.Response.StatusCode.Should().Be((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), expectedCode));
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

        [Then(@"I will receive the error message (.*)")]
        public void ThenIWillReceiveTheCorrectErrorMessage(string message)
        {
            ApiHelper.Response.Content.ReadAsStringAsync().Result.Should().ContainAll("type", "title", "status", "traceId");
            ApiHelper.Response.Content.ReadAsStringAsync().Result.Should().Contain(message);
        }

        [Then(@"I will get a health check response status message")]
        public async Task ThenIWillGetAHealthCheckResponseMessage()
        {
            var contentMessage = await ApiHelper.Response?.Content.ReadAsStringAsync();
            contentMessage.Should().NotBeNull();
            var response = JsonConvert.DeserializeObject<HealthCheckResponse>(contentMessage);
            response.Should().NotBeNull();
            response!.Checks.Select(p => p.Check).Should().Contain("minio", "Rabbit MQ Publisher", "Rabbit MQ Subscriber", "Workflow Manager Services", "mongodb");
        }
    }
}
