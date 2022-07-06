// SPDX-FileCopyrightText: � 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Reflection;
using BoDi;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowTaskArtefactStepDefinitions
    {

        private RabbitPublisher WorkflowPublisher { get; set; }
        private RabbitConsumer TaskDispatchConsumer { get; set; }
        private MongoClientUtil MongoClient { get; set; }
        private MinioClientUtil MinioClient { get; set; }
        private Assertions Assertions { get; set; }
        private DataHelper DataHelper { get; set; }
        private readonly ISpecFlowOutputHelper _outputHelper;

        public WorkflowTaskArtefactStepDefinitions(ObjectContainer objectContainer, ScenarioContext scenarioContext, ISpecFlowOutputHelper outputHelper)
        {
            WorkflowPublisher = objectContainer.Resolve<RabbitPublisher>("WorkflowPublisher");
            TaskDispatchConsumer = objectContainer.Resolve<RabbitConsumer>("TaskDispatchConsumer");
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            MinioClient = objectContainer.Resolve<MinioClientUtil>();
            Assertions = new Assertions();
            DataHelper = objectContainer.Resolve<DataHelper>();
            _outputHelper = outputHelper;
        }

        [Given(@"I have a bucket in MinIO (.*)")]
        public async Task GivenIHaveABucketInMinIO(string name)
        {
            _outputHelper.WriteLine($"Creating bucket {name}");
            await MinioClient.CreateBucket(name);
            _outputHelper.WriteLine($"{name} bucket created");
        }

        [Given(@"I have a payload (.*) and bucket in MinIO (.*)")]
        public async Task GivenIHaveABucketInMinIOAndPayloadId(string payloadId, string name)
        {
            _outputHelper.WriteLine($"Creating bucket {name}");
            await MinioClient.CreateBucket(name);
            _outputHelper.WriteLine($"{name} bucket created");
            _outputHelper.WriteLine("Retrieving pathname");
            var pathname = Path.Combine(GetDirectory(), "DICOMs", "dcm");
            _outputHelper.WriteLine($"Retrieved pathname {pathname}");
            _outputHelper.WriteLine($"Adding files to bucket from {pathname}");
            await MinioClient.AddFileToStorage(pathname, name, DataHelper.GetPayloadId(payloadId));
            _outputHelper.WriteLine($"Files added");
        }

        [Then(@"I can see a task dispatch event with a path to the DICOM bucket")]
        public void ThenICanSeeATaskDispatchEventWithAPathToTheDICOMBucket()
        {
            throw new PendingStepException();
        }

        [Then(@"The workflow instance fails")]
        public void ThenTheWorkflowInstanceFails()
        {
            throw new PendingStepException();
        }

        [When(@"I publish a task update message (.*)")]
        public void WhenIPublishATaskUpdateMessage(string name)
        {
            throw new PendingStepException();
        }

        [Then(@"The workflow instance is updated with correct file path")]
        public void ThenTheWorkflowInstanceIsUpdatedWithCorrectFilePath()
        {
            throw new PendingStepException();
        }

        [Then(@"The task dispatch message is updated with correct file path")]
        public void ThenTheTaskDispatchMessageIsUpdatedWithCorrectFilePath()
        {
            throw new PendingStepException();
        }

        [Given(@"I have a workflow instance")]
        public void GivenIHaveAWorkflowInstance()
        {
            throw new PendingStepException();
        }

        [When(@"I publish a task dispatch Message (.*)")]
        public void WhenIPublishATaskDispatchMessage()
        {
            throw new PendingStepException();
        }

        [When(@"I publish a workflow instance (.*)")]
        public void WhenIPublishAWorkflowInstance()
        {
            throw new PendingStepException();
        }

        private string GetDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /*[AfterScenario]
        public async Task DeleteObjects()
        {
            await MinioClient.RemoveObjects(TestExecutionConfig.MinioConfig.Bucket, DataHelper.PayloadId);
        }*/

    }
}
