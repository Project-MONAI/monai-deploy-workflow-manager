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

using System.Reflection;
using Monai.Deploy.WorkflowManager.Common.TaskManager.IntegrationTests.Support;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
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
        private MongoClientUtil MongoClient { get; set; }

        public CommonStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            TaskDispatchPublisher = objectContainer.Resolve<RabbitPublisher>("TaskDispatchPublisher");
            TaskCallbackPublisher = objectContainer.Resolve<RabbitPublisher>("TaskCallbackPublisher");
            MinioClient = objectContainer.Resolve<MinioClientUtil>();
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            _outputHelper = outputHelper;
        }

        [Given(@"I have an input DICOM file saved in MinIO for TaskDispatch (.*)")]
        public async Task GivenIHaveAnInputDICOMFileSavedInMinIO(string name)
        {
            var taskDispatch = DataHelper.GetTaskDispatchTestData(name);
            var localPath = Path.Combine(GetDirectory() ?? string.Empty, "DICOMs", "dcm");
            await MinioClient.AddFileToStorage(localPath, taskDispatch.Inputs.First().RelativeRootPath);
        }

        [Then(@"A Task Callback event is published (.*)")]
        [When(@"A Task Callback event is published (.*)")]
        public void ATaskCallbackEventIsPublished(string name)
        {
            _outputHelper.WriteLine($"Creating json for TaskCallbackEvent with name={name}");
            var taskCallback = DataHelper.GetTaskCallbackTestData(name);

            string correlationId;

            if (!string.IsNullOrEmpty(taskCallback.CorrelationId))
            {
                correlationId = taskCallback.CorrelationId;
            }
            else
            {
                throw new Exception($"CorrelationId is null or empty for TaskCallbackEvent with name={name}");
            }

            var message = new JsonMessage<TaskCallbackEvent>(
                taskCallback,
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                correlationId,
                string.Empty);

            TaskCallbackPublisher.PublishMessage(message.ToMessage());
            _outputHelper.WriteLine($"Successfully published TaskCallbackEvent with name={name}");
        }

        [Given(@"I have Task Dispatch Info saved in Mongo (.*)")]
        public void GivenIHaveTaskDispatchInfoSavedInMongo(string name)
        {
            _outputHelper.WriteLine($"Creating json for TaskDispatchEvent with name={name}");
            var taskDispatch = DataHelper.GetTaskDispatchTestData(name);

            var taskDispatchInfo = new TaskDispatchEventInfo(taskDispatch);

            MongoClient.CreateTaskDispatchEventInfo(taskDispatchInfo);
        }

        [Given(@"A Task Dispatch event is published (.*)")]
        [When(@"A Task Dispatch event is published (.*)")]
        public void ATaskDispatchEventIsPublished(string name)
        {
            _outputHelper.WriteLine($"Creating json for TaskDispatchEvent with name={name}");
            var taskDispatch = DataHelper.GetTaskDispatchTestData(name);

            string correlationId;

            if (!string.IsNullOrEmpty(taskDispatch.CorrelationId))
            {
                correlationId = taskDispatch.CorrelationId;
            }
            else
            {
                throw new Exception($"CorrelationId is null or empty for TaskDispatchEvent with name={name}");
            }

            var message = new JsonMessage<TaskDispatchEvent>(
                taskDispatch,
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                correlationId,
                string.Empty);

            TaskDispatchPublisher.PublishMessage(message.ToMessage());
            _outputHelper.WriteLine($"Successfully published TaskDispatchEvent with name={name}");
        }

        private string? GetDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
