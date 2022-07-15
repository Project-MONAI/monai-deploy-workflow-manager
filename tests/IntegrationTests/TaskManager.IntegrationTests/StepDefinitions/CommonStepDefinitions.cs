// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Reflection;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class CommonStepDefinitions
    {
        private DataHelper DataHelper { get; set; }
        private readonly ISpecFlowOutputHelper _outputHelper;
        private RabbitPublisher TaskDispatchPublisher { get; set; }
        private RabbitPublisher TaskCallbackPublisher { get; set; }
        private MinioClientUtil MinioClient { get; set; }

        public CommonStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            TaskDispatchPublisher = objectContainer.Resolve<RabbitPublisher>("TaskDispatchPublisher");
            TaskCallbackPublisher = objectContainer.Resolve<RabbitPublisher>("TaskCallbackPublisher");
            MinioClient = objectContainer.Resolve<MinioClientUtil>();
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

        [Then(@"A Task Callback event is published (.*)")]
        public void ATaskCallbackEventIsPublished(string name)
        {
            _outputHelper.WriteLine($"Creating json for TaskCallbackEvent with name={name}");
            var message = new JsonMessage<TaskCallbackEvent>(
                DataHelper.GetTaskCallbackTestData(name),
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                Guid.NewGuid().ToString(),
                string.Empty);

            TaskCallbackPublisher.PublishMessage(message.ToMessage());
            _outputHelper.WriteLine($"Successfully published TaskCallbackEvent with name={name}");
        }

        [When(@"A Task Dispatch event is published (.*)")]
        public void ATaskDispatchEventIsPublished(string name)
        {
            _outputHelper.WriteLine($"Creating json for TaskDispatchEvent with name={name}");
            var message = new JsonMessage<TaskDispatchEvent>(
                DataHelper.GetTaskDispatchTestData(name),
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                Guid.NewGuid().ToString(),
                string.Empty);

            TaskDispatchPublisher.PublishMessage(message.ToMessage());
            _outputHelper.WriteLine($"Successfully published TaskDispatchEvent with name={name}");
        }

        private string GetDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
