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
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Models;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.Support;
using MongoDB.Driver;
using NUnit.Framework;
using Polly;
using Polly.Retry;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.StepDefinitions
{
    [Binding]
    public class ArtifactReceivedEventStepDefinitions
    {
        private RabbitPublisher WorkflowPublisher { get; set; }
        private RabbitPublisher ArtifactsPublisher { get; set; }
        private RabbitConsumer TaskDispatchConsumer { get; set; }
        private MongoClientUtil MongoClient { get; set; }
        private Assertions Assertions { get; set; }
        private DataHelper DataHelper { get; set; }

        private readonly ISpecFlowOutputHelper _outputHelper;
        private RetryPolicy RetryPolicy { get; set; }
        private MinioDataSeeding MinioDataSeeding { get; set; }

        public ArtifactReceivedEventStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            ArtifactsPublisher = objectContainer.Resolve<RabbitPublisher>("ArtifactsPublisher");
            TaskDispatchConsumer = objectContainer.Resolve<RabbitConsumer>("TaskDispatchConsumer");
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            Assertions = new Assertions(objectContainer, outputHelper);
            DataHelper = objectContainer.Resolve<DataHelper>();
            _outputHelper = outputHelper;
            MinioDataSeeding =
                new MinioDataSeeding(objectContainer.Resolve<MinioClientUtil>(), DataHelper, _outputHelper);
            RetryPolicy = Policy.Handle<Exception>()
                .WaitAndRetry(retryCount: 20, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
        }

        [When(@"I publish a Artifact Received Event (.*)")]
        public async Task WhenIPublishAArtifactReceivedEvent(string name)
        {
            var message = new JsonMessage<ArtifactsReceivedEvent>(
                DataHelper.GetArtifactsReceivedEventTestData(name),
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                Guid.NewGuid().ToString(),
                string.Empty);

            _outputHelper.WriteLine($"Publishing WorkflowRequestEvent with name={name}");
            ArtifactsPublisher.PublishMessage(message.ToMessage());
            _outputHelper.WriteLine($"Event published");
        }

        [Given(@"I have a clinical workflow (.*) I have a Workflow Instance (.*)")]
        public async Task GivenIHaveAClinicalWorkflowIHaveAWorkflowInstance(string clinicalWorkflowName, string wfiName)
        {
            var (artifactReceivedItems, workflowInstance, workflowRevision) =
                DataHelper.GetArtifactsEventTestData(clinicalWorkflowName, wfiName);

            _outputHelper.WriteLine("Seeding minio with workflow input artifacts");
            await MinioDataSeeding.SeedWorkflowInputArtifacts(workflowInstance.PayloadId);
            await MinioDataSeeding.SeedArtifactRecieviedArtifact(workflowInstance.PayloadId);

            _outputHelper.WriteLine($"Retrieving workflow instance with name={wfiName}");
            await MongoClient.CreateWorkflowInstanceDocumentAsync(workflowInstance);

            _outputHelper.WriteLine($"Retrieving workflow revision with name={clinicalWorkflowName}");
            await MongoClient.CreateWorkflowRevisionDocumentAsync(workflowRevision);

            try
            {
                await MongoClient.CreateArtifactsEventsDocumentAsync(artifactReceivedItems);
            }
            catch (Exception e)
            {
            }

            _outputHelper.WriteLine("Seeding Data Tasks complete");
        }

        [Then(@"I can see a Artifact Received Item is created")]
        public void ThenICanSeeAArtifactReceivedItemIsCreated()
        {
            ThenICanSeeXArtifactReceivedItemIsCreated(1);
        }

        [Then(@"I can see ([1-9]*) Artifact Received Items are created")]
        [Then(@"I can see ([0-9]*) Artifact Received Items is created")]
        public void ThenICanSeeXArtifactReceivedItemIsCreated(int count)
        {
            _outputHelper.WriteLine($"Retrieving {count} workflow instance/s using the payloadid={DataHelper.WorkflowRequestMessage.PayloadId.ToString()}");
            RetryPolicy.Execute(() =>
            {
                var artifactsReceivedItems = DataHelper.GetArtifactsReceivedItemsFromDB(count, DataHelper.ArtifactsReceivedEvent);
                if (artifactsReceivedItems.Any())
                {
                    foreach (var artifactsReceivedItem in artifactsReceivedItems)
                    {
                        var wfiId = artifactsReceivedItems.FirstOrDefault().WorkflowInstanceId;
                        var wfi = DataHelper.WorkflowInstances.FirstOrDefault(a => a.Id == wfiId);
                        var workflow = DataHelper.WorkflowRevisions.FirstOrDefault(w => w.WorkflowId == wfi.WorkflowId);
                        if (workflow is null)
                        {
                            throw new Exception("Failing Test");
                        }
                        var wfitest = MongoClient.GetWorkflowInstanceById(artifactsReceivedItems.FirstOrDefault().WorkflowInstanceId);
                        Assertions.AssertArtifactsReceivedItemMatchesExpectedWorkflow(artifactsReceivedItem, workflow, wfi);
                        Assert.AreEqual(wfitest.Tasks[1].OutputArtifacts.First().Value, "path"); // this was passed in the message
                    }
                }
            });
            _outputHelper.WriteLine($"Retrieved {count} workflow instance/s");
        }
    }
}
