/*
 * Copyright 2021-2022 MONAI Consortium
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
using FluentAssertions;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class CommonApiDefinitions
    {
        private MongoClientUtil MongoClient { get; }

        public CommonApiDefinitions(ObjectContainer objectContainer)
        {
            ApiHelper = objectContainer.Resolve<ApiHelper>();
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
        }

        [Given(@"I have a TaskManager endpoint (.*)")]
        public void GivenIHaveATaskmanagerEndpoint(string endpoint) => ApiHelper.SetUrl(new Uri(TestExecutionConfig.ApiConfig.TaskManagerBaseUrl + endpoint));

        public ApiHelper ApiHelper { get; }

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
            var names = MongoClient.Database.ListCollectionNames().ToList();
            Console.WriteLine("Mongo Collections={0}", names.Count);
            names.ForEach(p => Console.WriteLine("Mong Collection={0}", p));
            ApiHelper.Response?.StatusCode.Should().Be((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), expectedCode));
        }

        [Then(@"I will get a status message (.*)")]
        public void ThenIWillGetAMessage(string expectedMessage)
        {
            ApiHelper.Response?.Content.ReadAsStringAsync().Result.Should().Be(expectedMessage);
        }

        [Then(@"I will get a health check response status message (.*)")]
        public void ThenIWillGetAHealthCheckResponseMessage(string expectedMessage)
        {
            var contentMessage = ApiHelper.Response?.Content.ReadAsStringAsync().Result;

            contentMessage.Should().NotBeNull();
            contentMessage.Should().Contain(expectedMessage);
        }
    }
}