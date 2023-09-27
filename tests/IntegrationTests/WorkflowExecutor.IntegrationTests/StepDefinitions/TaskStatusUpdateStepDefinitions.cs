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
using Polly;
using Polly.Retry;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TaskStatusUpdateStepDefinitions
    {
        private MongoClientUtil MongoClient { get; set; }
        private RabbitPublisher TaskUpdatePublisher { get; set; }
        private RabbitPublisher ExportCompletePublisher { get; set; }
        private RetryPolicy RetryPolicy { get; set; }
        private DataHelper DataHelper { get; set; }
        private readonly ISpecFlowOutputHelper _outputHelper;
        public MinioDataSeeding MinioDataSeeding { get; }
        private Assertions Assertions { get; set; }

        public TaskStatusUpdateStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            TaskUpdatePublisher = objectContainer.Resolve<RabbitPublisher>("TaskUpdatePublisher");
            ExportCompletePublisher = objectContainer.Resolve<RabbitPublisher>("ExportCompletePublisher");
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            RetryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
            _outputHelper = outputHelper;
            MinioDataSeeding = new MinioDataSeeding(objectContainer.Resolve<MinioClientUtil>(), DataHelper, _outputHelper);
            Assertions = new Assertions(objectContainer, outputHelper);
        }

        [When(@"I publish a Task Update Message (.*) with status (.*)")]
        public void WhenIPublishATaskUpdateMessageWithStatus(string name, string updateStatus)
        {
            var message = new JsonMessage<TaskUpdateEvent>(
                DataHelper.GetTaskUpdateTestData(name, updateStatus),
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                Guid.NewGuid().ToString(),
                string.Empty);

            TaskUpdatePublisher.PublishMessage(message.ToMessage());
        }

        [When(@"I publish a Export Complete Message (.*)")]
        public void WhenIPublishAExportComplete(string name)
        {
            var message = new JsonMessage<ExportCompleteEvent>(
                DataHelper.GetExportCompleteTestData(name),
                "0733f003-b718-4341-9f9d-8c62f28716dd",
                Guid.NewGuid().ToString(),
                string.Empty);

            ExportCompletePublisher.PublishMessage(message.ToMessage());
        }

        [Then(@"Metadata is added to task (.*)")]
        public void ThenTheNumberOfSuccessfulExportsAre(string completeExportData)
        {
            RetryPolicy.Execute(() =>
            {
                var exportCompleteExpected = DataHelper.GetExportCompleteTestData(completeExportData);

                var workflow = DataHelper.GetAllWorkflowInstance(exportCompleteExpected.WorkflowInstanceId);

                var task = workflow.Tasks.First(f => f.TaskId == exportCompleteExpected.ExportTaskId);

                var resultMetadata = exportCompleteExpected.FileStatuses.ToDictionary(f => f.Key, f => f.Value.ToString() as object);

                if (task.ResultMetadata.Any())
                {
                    task.ResultMetadata.Should().BeEquivalentTo(resultMetadata);
                }
            });
        }

        [When(@"I publish a Task Update Message (.*) with artifacts (.*) in minio")]
        public async Task WhenIPublishATaskUpdateMessageWithArtifacts(string name, string folderName)
        {
            var taskUpdateMessage = DataHelper.GetTaskUpdateTestData(name, "succeeded");

            foreach (var workflowInstance in DataHelper.WorkflowInstances)
            {
                var task = workflowInstance.Tasks.FirstOrDefault(x => x.Status == TaskExecutionStatus.Accepted);

                if (task != null)
                {
                    _outputHelper.WriteLine("Seeding minio with task output artifacts");
                    await MinioDataSeeding.SeedTaskOutputArtifacts(workflowInstance.PayloadId, workflowInstance.Id, task.ExecutionId, folderName);
                }
            }

            var message = new JsonMessage<TaskUpdateEvent>(
                taskUpdateMessage,
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                Guid.NewGuid().ToString(),
                string.Empty);

            TaskUpdatePublisher.PublishMessage(message.ToMessage());
        }

        [When(@"I publish a Task Update Message (.*) with no artifacts")]
        public void WhenIPublishATaskUpdateMessageWithNoArtifacts(string name)
        {
            var taskUpdateMessage = DataHelper.GetTaskUpdateTestData(name, "succeeded");

            var message = new JsonMessage<TaskUpdateEvent>(
                taskUpdateMessage,
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                Guid.NewGuid().ToString(),
                string.Empty);

            TaskUpdatePublisher.PublishMessage(message.ToMessage());
        }

        [Then(@"I can see the status of the Tasks are updated")]
        [Then(@"I can see the status of the Task is updated")]
        public void ThenICanSeeTheStatusOfTheTaskIsUpdated()
        {
            RetryPolicy.Execute(() =>
            {
                _outputHelper.WriteLine($"Retrieving workflow instance by id={DataHelper.TaskUpdateEvent.WorkflowInstanceId}");
                var workflowInstance = MongoClient.GetWorkflowInstanceById(DataHelper.TaskUpdateEvent.WorkflowInstanceId);
                _outputHelper.WriteLine("Retrieved workflow instance");

                var taskUpdated = workflowInstance.Tasks.FirstOrDefault(x => x.TaskId.Equals(DataHelper.TaskUpdateEvent.TaskId));

                taskUpdated.Should().NotBeNull();

                taskUpdated?.Status.Should().Be(DataHelper.TaskUpdateEvent.Status);
                taskUpdated?.Reason.Should().Be(DataHelper.TaskUpdateEvent.Reason);

                if (DataHelper.TaskDispatchEvents.Count > 0)
                {
                    foreach (var e in DataHelper.TaskDispatchEvents)
                    {
                        var taskDispatched = workflowInstance.Tasks.FirstOrDefault(x => x.TaskId.Equals(e.TaskId));

                        taskDispatched?.Status.Should().Be(TaskExecutionStatus.Dispatched);
                    }
                }
            });
        }

        [Then(@"Clinical Review Metadata is added to workflow instance")]
        public void ClinicalReviewMetadataIsAddedtoWorkflowInstance()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
            RetryPolicy.Execute(() =>
            {
                _outputHelper.WriteLine($"Retrieving workflow instance by id={DataHelper.TaskUpdateEvent.WorkflowInstanceId}");
                var workflowInstance = MongoClient.GetWorkflowInstanceById(DataHelper.TaskUpdateEvent.WorkflowInstanceId);
                _outputHelper.WriteLine("Retrieved workflow instance");

                var taskUpdated = workflowInstance.Tasks.FirstOrDefault(x => x.TaskId.Equals(DataHelper.TaskUpdateEvent.TaskId));

                taskUpdated.Should().NotBeNull();

                taskUpdated?.ResultMetadata.Should().ContainKey("acceptance");

                var acceptance = (bool)taskUpdated.ResultMetadata["acceptance"];

                if (acceptance is false)
                {
                    taskUpdated?.ResultMetadata.Should().ContainKey("reason");
                }

                taskUpdated?.ResultMetadata.Should().ContainKey("user_id");
            });
        }

        [Then(@"I can see the status of the Task is not updated")]
        public void ThenICanSeeTheStatusOfTheTaskIsNotUpdated()
        {
            if (DataHelper.TaskUpdateEvent.TaskId != "")
            {
                RetryPolicy.Execute(() =>
                {
                    _outputHelper.WriteLine($"Retrieving workflow instance by id={DataHelper.TaskUpdateEvent.WorkflowInstanceId}");
                    var workflowInstance = MongoClient.GetWorkflowInstanceById(DataHelper.TaskUpdateEvent.WorkflowInstanceId);
                    _outputHelper.WriteLine("Retrieved workflow instance");

                    var taskUpdated = workflowInstance.Tasks.FirstOrDefault(x => x.TaskId.Equals(DataHelper.TaskUpdateEvent.TaskId));

                    if (taskUpdated != null)
                    {
                        taskUpdated.Status.Should().NotBe(DataHelper.TaskUpdateEvent.Status);
                    }
                    else
                    {
                        throw new Exception($"Task Update Event could not be found with task ID {DataHelper.TaskUpdateEvent.TaskId}");
                    }
                });
            }
            else
            {
                throw new Exception("A task update has not been sent which is required for this assertion");
            }
        }

        [Then(@"I can see the status of the Task (.*) is (.*)")]
        public void ThenICanSeeTheStatusOfTheTaskIsSucceeded(string taskId, string taskStatus)
        {
            string? workflowInstanceId = null;
            if (DataHelper.TaskUpdateEvent.WorkflowInstanceId != "")
            {
                workflowInstanceId = DataHelper.TaskUpdateEvent.WorkflowInstanceId;
            }
            else if (DataHelper.ExportCompleteEvent.WorkflowInstanceId != "")
            {
                workflowInstanceId = DataHelper.ExportCompleteEvent.WorkflowInstanceId;
            }
            else
            {
                throw new Exception("Workflow instance ID could not be found");
            };

            _outputHelper.WriteLine($"Retrieving workflow instance by id={workflowInstanceId}");
            var updatedWorkflowInstance = MongoClient.GetWorkflowInstanceById(workflowInstanceId);
            _outputHelper.WriteLine("Retrieved workflow instance");
            TaskExecutionStatus executionStatus;
            executionStatus = taskStatus.ToLower() switch
            {
                "accepted" => TaskExecutionStatus.Accepted,
                "succeeded" => TaskExecutionStatus.Succeeded,
                "failed" => TaskExecutionStatus.Failed,
                "canceled" => TaskExecutionStatus.Canceled,
                "created" => TaskExecutionStatus.Created,
                "dispatched" => TaskExecutionStatus.Dispatched,
                _ => throw new Exception($"Task Execution Status {taskStatus} is not recognised. Please check and try again."),
            };

            RetryPolicy.Execute(() =>
                {
                    if (updatedWorkflowInstance.Tasks.FirstOrDefault(x => x.TaskId == taskId)?.Status != executionStatus)
                    {
                        updatedWorkflowInstance = MongoClient.GetWorkflowInstanceById(workflowInstanceId);
                        throw new Exception($"Task Update Status for the task is {updatedWorkflowInstance.Tasks.FirstOrDefault(x => x.TaskId == taskId)?.Status} and it should be {executionStatus}");
                    }
                });

            updatedWorkflowInstance.Tasks.FirstOrDefault(x => x.TaskId == taskId)?.Status.Should().Be(executionStatus);
        }

        [Then(@"I can see the Metadata is copied to the workflow instance")]
        public void ThenTheMetadataIsCopied()
        {
            RetryPolicy.Execute(() =>
            {
                _outputHelper.WriteLine($"Retrieving workflow instance by id={DataHelper.TaskUpdateEvent.WorkflowInstanceId}");
                var workflowInstance = MongoClient.GetWorkflowInstanceById(DataHelper.TaskUpdateEvent.WorkflowInstanceId);
                _outputHelper.WriteLine("Retrieved workflow instance");

                var taskUpdated = workflowInstance.Tasks.FirstOrDefault(x => x.TaskId.Equals(DataHelper.TaskUpdateEvent.TaskId));

                if (DataHelper.TaskUpdateEvent.Metadata.Count == 0)
                {
                    taskUpdated?.ResultMetadata.Should().BeEmpty();
                }
                else
                {
                    taskUpdated?.ResultMetadata.Should().BeEquivalentTo(DataHelper.TaskUpdateEvent.Metadata);
                }
            });
        }

        [Then(@"(.*) Export Request message is published")]
        [Then(@"(.*) Export Request messages are published")]
        public void ThenExportRequestMessageIsPublished(int count)
        {
            if (count == 0)
            {
                _outputHelper.WriteLine("Retrieving all export request events");
                var allExportRequestEvents = DataHelper.GetAllExportRequestEvents(DataHelper.WorkflowInstances);
                _outputHelper.WriteLine($"Retrieved {allExportRequestEvents.Count()} export request events");
                allExportRequestEvents.Count().Should().Be(count);
            }
            else
            {
                _outputHelper.WriteLine($"Retrieving {count} export request event/s");
                var exportRequestEvents = DataHelper.GetExportRequestEvents(count, DataHelper.WorkflowInstances);
                _outputHelper.WriteLine($"Retrieved {count} export request event/s");

                RetryPolicy.Execute(() =>
                {
                    foreach (var exportRequestEvent in exportRequestEvents)
                    {
                        var workflowInstance = MongoClient.GetWorkflowInstanceById(exportRequestEvent.WorkflowInstanceId);

                        var workflowRevision = DataHelper.WorkflowRevisions.OrderByDescending(x => x.Revision).FirstOrDefault(x => x.WorkflowId.Equals(workflowInstance.WorkflowId));

                        if (string.IsNullOrEmpty(DataHelper.TaskUpdateEvent.ExecutionId))
                        {
                            Assertions.AssertExportRequestEvent(exportRequestEvent, workflowInstance, workflowRevision, DataHelper.WorkflowRequestMessage, null);
                        }
                        else
                        {
                            Assertions.AssertExportRequestEvent(exportRequestEvent, workflowInstance, workflowRevision, null, DataHelper.TaskUpdateEvent);
                        }
                    }
                });
            }
        }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
