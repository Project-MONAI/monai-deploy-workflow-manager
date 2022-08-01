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

using BoDi;
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
            _outputHelper = outputHelper;
            Assertions = new Assertions();
            DataHelper = objectContainer.Resolve<DataHelper>();
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
