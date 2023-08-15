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
using FluentAssertions;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support;
using Newtonsoft.Json;
using Snapshooter.NUnit;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class CommonApiDefinitions
    {
        public ApiHelper ApiHelper { get; }

        public DataHelper DataHelper { get; }

        public CommonApiDefinitions(ObjectContainer objectContainer)
        {
            ApiHelper = objectContainer.Resolve<ApiHelper>();
            DataHelper = objectContainer.Resolve<DataHelper>();
        }

        [Given(@"I have a TaskManager endpoint (.*)")]
        public void GivenIHaveATaskmanagerEndpoint(string endpoint) => ApiHelper.SetUrl(new Uri(TestExecutionConfig.ApiConfig.TaskManagerBaseUrl + endpoint));

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
            var content = ApiHelper.Response?.Content.ReadAsStringAsync().Result;
            ApiHelper.Response?.StatusCode.Should().Be((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), expectedCode));
        }

        [Then(@"I will get a health check response message")]
        public void ThenIWillGetAHealthCheckResponseMessage()
        {
            Snapshot.Match(DataHelper.FormatResponse(ApiHelper.Response?.Content.ReadAsStringAsync().Result ?? string.Empty));
        }

        [Then(@"I will get a status message (.*)")]
        public void ThenIWillGetAMessage(string expectedMessage)
        {
            ApiHelper.Response?.Content.ReadAsStringAsync().Result.Should().Be(expectedMessage);
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
            response!.Checks.Should().ContainEquivalentOf<Component>(new Component { Check = "Task Manager Services", Result = expectedMessage });
            response!.Checks.Should().ContainEquivalentOf<Component>(new Component { Check = "mongodb", Result = expectedMessage });
        }
    }
}
