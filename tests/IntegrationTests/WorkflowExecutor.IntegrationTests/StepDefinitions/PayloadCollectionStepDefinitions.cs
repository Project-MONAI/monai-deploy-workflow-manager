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

using BoDi;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData;
using Polly;
using Polly.Retry;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.StepDefinitions
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
        private RetryPolicy RetryPolicy { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public PayloadCollectionStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _outputHelper = outputHelper;
            Assertions = new Assertions(objectContainer, outputHelper);
            DataHelper = objectContainer.Resolve<DataHelper>();
            RetryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 5, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
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

        [Then(@"Patient details (.*) have been added to task dispatch message")]
        public void ThenPatientDetailsHaveBeenAddedToTaskDispatchMessage(string name)
        {
            var expected = PatientsTestData.TestData.First();
            var taskDispatchEvents = DataHelper.GetTaskDispatchEvents(1, DataHelper.WorkflowInstances).First();

            var patientId = taskDispatchEvents.TaskPluginArguments["patient_id"];
            var patientAge = taskDispatchEvents.TaskPluginArguments["patient_age"];
            var patientSex = taskDispatchEvents.TaskPluginArguments["patient_sex"];
            var patientDOB = taskDispatchEvents.TaskPluginArguments["patient_dob"];
            var patientHospital = taskDispatchEvents.TaskPluginArguments["patient_hospital_id"];
            var patientName = taskDispatchEvents.TaskPluginArguments["patient_name"];

            var actual = new PatientTestData
            {
                Name = name,
                Patient = new PatientDetails()
                {
                    PatientId = patientId,
                    PatientName = patientName,
                    PatientSex = patientSex,
                    PatientDob = DateTime.Parse(patientDOB),
                    PatientAge = patientAge,
                    PatientHospitalId = patientHospital
                }
            };
            expected.Should().BeEquivalentTo(actual);
        }

        [Then(@"A payload collection is created with (.*) workflow instance id")]
        public void ThenAPayloadCollectionIsCreatedWithWorkflowInstanceId(int count)
        {
            RetryPolicy.Execute(() =>
            {
                _outputHelper.WriteLine($"Retrieving payload collection using the payloadid={DataHelper.WorkflowRequestMessage.PayloadId}");
                var payloadCollections = DataHelper.GetPayloadCollections(DataHelper.WorkflowRequestMessage.PayloadId.ToString());
                _outputHelper.WriteLine($"Retrieved payload collection");

                if (payloadCollections != null)
                {
                    foreach (var payloadCollection in payloadCollections)
                    {
                        var workflowInstances = DataHelper.GetWorkflowInstances(count, DataHelper.WorkflowRequestMessage.PayloadId.ToString());
                        if (count != 0)
                        {
                            if (workflowInstances != null)
                            {
                                Assertions.AssertPayloadWorkflowInstanceId(payloadCollection, workflowInstances);
                            }
                            else
                            {
                                throw new Exception($"Workflow Instance not found");
                            }
                        }
                        else
                        {
                            payloadCollection.WorkflowInstanceIds.Should().BeEmpty();
                        }
                    }
                }
            });
        }
    }
}
