// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using BoDi;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.IntegrationTests.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class PayloadCollectionStepDefinitions
    {
        private RabbitPublisher WorkflowPublisher { get; set; }
        private RabbitConsumer TaskDispatchConsumer { get; set; }
        private MongoClientUtil MongoClient { get; set; }
        private Assertions Assertions { get; set; }
        private DataHelper DataHelper { get; set; }
        private readonly ISpecFlowOutputHelper _outputHelper;

        public PayloadCollectionStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            WorkflowPublisher = objectContainer.Resolve<RabbitPublisher>("WorkflowPublisher");
            TaskDispatchConsumer = objectContainer.Resolve<RabbitConsumer>("TaskDispatchConsumer");
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            Assertions = new Assertions();
            DataHelper = objectContainer.Resolve<DataHelper>();
            _outputHelper = outputHelper;
        }

        [Then(@"A payload collection is created with patient details (.*)")]
        public void ThenPayloadCollectionIsCreated(string patientDetailsName)
        {
            _outputHelper.WriteLine($"Retrieving payload collection using the payloadid={DataHelper.WorkflowRequestMessage.PayloadId}");
            var payloadCollections = DataHelper.GetPayloadCollections(DataHelper.WorkflowRequestMessage.PayloadId.ToString());
            _outputHelper.WriteLine($"Retrieved payload collection");

            if (payloadCollections != null)
            {
                foreach (var payloadCollection in payloadCollections)
                {
                    var workflowRequest = DataHelper.WorkflowRequestMessage;
                    var patientDetails = DataHelper.GetPatientDetailsTestData(patientDetailsName);

                    if (workflowRequest != null)
                    {
                        Assertions.AssertPayloadCollection(payloadCollection, patientDetails, workflowRequest);
                    }
                    else
                    {
                        throw new Exception($"Workflow Request not found");
                    }
                }
            }
        }
    }
}
