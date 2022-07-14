// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using BoDi;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.Wrappers;
using Newtonsoft.Json;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.WorkflowExecutor.IntegrationTests.StepDefinitions
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
            Assertions = new Assertions();
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

        [Then(@"I can see expected Payloads are returned")]
        public void ThenICanSeeExpectedPayloadsAreReturned()
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;

            var actualPayloads = JsonConvert.DeserializeObject<PagedResponse<List<Payload>>>(result);
            Assertions.AssertPayloadList(DataHelper.Payload, actualPayloads.Data);
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

            payloads.Data.Should().BeNullOrEmpty();

        }
    }
}
