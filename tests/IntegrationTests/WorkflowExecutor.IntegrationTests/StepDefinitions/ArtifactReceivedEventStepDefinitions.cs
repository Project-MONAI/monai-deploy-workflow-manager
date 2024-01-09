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
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.Support;
using MongoDB.Driver;
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

        private const string FixedGuidPayload = "16988a78-87b5-4168-a5c3-2cfc2bab8e54";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ArtifactReceivedEventStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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
                .WaitAndRetry(retryCount: 2, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(50000));
        }

        [When(@"I publish a Artifact Received Event (.*)")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task WhenIPublishAArtifactReceivedEvent(string name)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var message = new JsonMessage<ArtifactsReceivedEvent>(
                DataHelper.GetArtifactsReceivedEventTestData(name),
                FixedGuidPayload,
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
            await MinioDataSeeding.SeedWorkflowInputArtifacts(FixedGuidPayload, "");

            _outputHelper.WriteLine($"Retrieving workflow instance with name={wfiName}");
            await MongoClient.CreateWorkflowInstanceDocumentAsync(workflowInstance);

            _outputHelper.WriteLine($"Retrieving workflow revision with name={clinicalWorkflowName}");
            await MongoClient.CreateWorkflowRevisionDocumentAsync(workflowRevision);

#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                await MongoClient.CreateArtifactsEventsDocumentAsync(artifactReceivedItems);
            }
            catch (Exception e)
            {
            }
#pragma warning restore CS0168 // Variable is declared but never used

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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        var wfiId = artifactsReceivedItems.FirstOrDefault().WorkflowInstanceId;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                        var wfi = DataHelper.WorkflowInstances.FirstOrDefault(a => a.Id == wfiId);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        var workflow = DataHelper.WorkflowRevisions.FirstOrDefault(w => w.WorkflowId == wfi.WorkflowId);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                        if (workflow is null)
                        {
                            throw new Exception("Failing Test");
                        }
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        var wfitest = MongoClient.GetWorkflowInstanceById(artifactsReceivedItems.FirstOrDefault().WorkflowInstanceId);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                        Assertions.AssertArtifactsReceivedItemMatchesExpectedWorkflow(artifactsReceivedItem, workflow, wfi);
                    }
                }
            });
            _outputHelper.WriteLine($"Retrieved {count} workflow instance/s");
        }
    }
}
