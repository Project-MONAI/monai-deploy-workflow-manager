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
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;
using Newtonsoft.Json;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.StepDefinitions
{
    [Binding]
    public class PayloadApiStepDefinitions
    {
        public PayloadApiStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            ApiHelper = objectContainer.Resolve<ApiHelper>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            _outputHelper = outputHelper;
            Assertions = new Assertions(objectContainer, outputHelper);
        }

        private Assertions Assertions { get; }

        private ApiHelper ApiHelper { get; }

        private DataHelper DataHelper { get; }

        private MongoClientUtil MongoClient { get; }

        private readonly ISpecFlowOutputHelper _outputHelper;

        [Given(@"I have a payload saved in mongo (.*)")]
        public void GivenIHaveAPayloadSavedInMongo(string name)
        {
            _outputHelper.WriteLine($"Retrieving payload with name={name}");
            MongoClient.CreatePayloadDocument(DataHelper.GetPayloadTestData(name));
            _outputHelper.WriteLine($"Seeded payload with name={name} into Mongo");
        }

        [Given(@"I have (.*) Payloads")]
        public void GivenIHaveWorkflowInstances(int count)
        {
            _outputHelper.WriteLine($"Retrieving {count} payloads");
            foreach (int index in Enumerable.Range(0, count))
            {
                _outputHelper.WriteLine($"Retrieving payload with index={index}");
                MongoClient.CreatePayloadDocument(DataHelper.GetPayloadsTestDataByIndex(index));
                _outputHelper.WriteLine("Retrieved payload");
            }
        }

        [Then(@"I can see expected Payloads are returned")]
        public void ThenICanSeeExpectedPayloadsAreReturned()
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;

            var actualPayloads = JsonConvert.DeserializeObject<PagedResponse<List<Payload>>>(result);
            actualPayloads.Should().NotBeNull();
            Assertions.AssertPayloadList(DataHelper.Payload, actualPayloads?.Data);
        }

        [Then(@"I can see expected Payloads are returned with PayloadStatus (.*)")]
        public void ThenICanSeeExpectedPayloadsAreReturnedWithPayloadStatus(string payloadStatus)
        {
            PayloadStatus status;

            switch (payloadStatus)
            {
                case "InProgress":
                    status = PayloadStatus.InProgress;
                    break;
                case "Complete":
                    status = PayloadStatus.Complete;
                    break;
                default:
                    throw new Exception($"Invalid payload status '{payloadStatus}'. Must be one of: InProgress, Complete");
            }

            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;

            var actualPayloads = JsonConvert.DeserializeObject<PagedResponse<List<PayloadDto>>>(result);
            actualPayloads.Should().NotBeNull();
            Assertions.AssertPayloadListWithPayloadStatus(
                DataHelper.Payload.Select(p => new PayloadDto(p)).ToList(),
                actualPayloads?.Data,
                status);
        }

        [Then(@"Search is working correctly for the (.*) payload")]
        [Then(@"Search is working correctly for the (.*) payloads")]
        public void ThenSearchIsWorkingCorrectlyForThepayloads(int count)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var request = ApiHelper.Request.RequestUri.Query;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var deserializedResult = JsonConvert.DeserializeObject<PagedResponse<List<Payload>>>(result);
            deserializedResult.Should().NotBeNull();
            Assertions.AssertSearch(count, request, deserializedResult);
        }

        [Then(@"Pagination is working correctly for the (.*) payload")]
        [Then(@"Pagination is working correctly for the (.*) payloads")]
        public void ThenPaginationIsWorkingCorrectlyForThepayloads(int count)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var request = ApiHelper.Request.RequestUri.Query;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var deserializedResult = JsonConvert.DeserializeObject<PagedResponse<List<Payload>>>(result);
            deserializedResult.Should().NotBeNull();
            Assertions.AssertPagination(count, request, deserializedResult);
        }

        [Then(@"I can see expected Payload is returned")]
        public void ThenICanSeeExpectedPayloadIsReturned()
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var actualPayload = JsonConvert.DeserializeObject<Payload>(result);
            Assertions.AssertPayload(DataHelper.Payload[0], actualPayload);
        }

        [Then(@"I can see no Payloads are returned")]
        public void ThenICanSeeNoPayloadsAreReturned()
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var payloads = JsonConvert.DeserializeObject<PagedResponse<List<Payload>>>(result);

            payloads.Should().NotBeNull();
            payloads?.Data.Should().BeNullOrEmpty();
        }
    }
}
