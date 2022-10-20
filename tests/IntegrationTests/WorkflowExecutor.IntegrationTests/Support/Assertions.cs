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

using System.Collections;
using BoDi;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public class Assertions
    {
        private static MinioClientUtil? MinioClient { get; set; }

        public Assertions(ObjectContainer objectContainer)
        {
            MinioClient = objectContainer.Resolve<MinioClientUtil>();
        }

        public void AssertTaskPayload(List<WorkflowInstance> workflowInstances, TaskExecution? response)
        {
            foreach (var workflowInstance in workflowInstances)
            {
                var taskExecution = workflowInstance.Tasks.First(x => x.TaskId.Equals(response?.TaskId));

                if (taskExecution != null)
                {
                    AssertionOptions.AssertEquivalencyUsing(options =>
                      options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(
                          ctx.Expectation,
                          TimeSpan.FromSeconds(0.1))).WhenTypeIs<DateTime>()
                    );

#pragma warning disable CS8604 // Possible null reference argument.
                    taskExecution.Should().BeEquivalentTo<TaskExecution>(response);
                    return;
#pragma warning restore CS8604 // Possible null reference argument.
                }
            }

            throw new Exception($"TaskId={response.TaskId} was not found in any workflow instances");
        }

        public void AssertWorkflowInstanceMatchesExpectedWorkflow(WorkflowInstance workflowInstance, WorkflowRevision workflowRevision, WorkflowRequestMessage workflowRequestMessage)
        {
            workflowInstance.PayloadId.Should().Match(workflowRequestMessage.PayloadId.ToString());
            workflowInstance.WorkflowId.Should().Match(workflowRevision.WorkflowId);
            workflowInstance.AeTitle.Should().Match(workflowRevision?.Workflow?.InformaticsGateway?.AeTitle);

            foreach (var task in workflowInstance.Tasks)
            {
                var workflowTask = workflowRevision?.Workflow?.Tasks.FirstOrDefault(x => x.Id.Equals(task.TaskId));
                if (workflowTask != null)
                {
                    task.TaskId.Should().Match(workflowTask.Id);
                    task.TaskType.Should().Match(workflowTask.Type);
                    AssertOutputDirectory(task, workflowInstance.PayloadId, workflowInstance.Id);
                }
                else
                {
                    throw new Exception($"Workflow Revision Task or {task.TaskId} not found!");
                }
            }
        }

        public void AssertInputArtifactsForWorkflowInstance(TaskObject workflowRevisionTask, string payloadId, TaskExecution workflowInstanceTask, TaskExecution previousTaskExecution = null)
        {
            foreach (var workflowArtifact in workflowRevisionTask.Artifacts.Input)
            {
                if (workflowArtifact.Value == "{{ context.input.dicom }}")
                {
                    workflowInstanceTask.InputArtifacts[workflowArtifact.Name].Should().Match($"{payloadId}/dcm");
                }
                else if (workflowArtifact.Value.Contains("artifacts"))
                {
                    var taskInputArtifact = workflowInstanceTask.InputArtifacts.FirstOrDefault(x => x.Key.Equals(workflowArtifact.Name));
                    var previousTaskOutputArtifact = previousTaskExecution.OutputArtifacts.FirstOrDefault(x => x.Key.Equals(workflowArtifact.Name));
                    taskInputArtifact.Should().BeEquivalentTo(previousTaskOutputArtifact);
                }
                else if (workflowArtifact.Value.Contains("output"))
                {
                    workflowInstanceTask.InputArtifacts[workflowArtifact.Name].Should().Match(previousTaskExecution.OutputDirectory);
                }
            }
        }

        public void AssertInputArtifactsForTaskDispatch(TaskObject workflowRevisionTask, string payloadId, List<Messaging.Common.Storage> taskDispatchInput, TaskUpdateEvent taskUpdateEvent = null, string taskOutputDirectory = null)
        {
            foreach (var workflowArtifact in workflowRevisionTask.Artifacts.Input)
            {
                var taskDispatchArtifact = taskDispatchInput.First(x => x.Name.Equals(workflowArtifact.Name));
                taskDispatchArtifact.Bucket.Should().Match(TestExecutionConfig.MinioConfig.Bucket);

                if (workflowArtifact.Value == "{{ context.input.dicom }}")
                {
                    taskDispatchArtifact.RelativeRootPath.Should().Match($"{payloadId}/dcm");
                }
                else if (workflowArtifact.Value.Contains("artifacts"))
                {
                    var previousTaskOutput = taskUpdateEvent.Outputs.First(x => x.Name.Equals(taskDispatchArtifact.Name));
                    taskDispatchArtifact.RelativeRootPath.Should().Match($"{previousTaskOutput.RelativeRootPath}");
                }
                else if (workflowArtifact.Value.Contains("output"))
                {
                    taskDispatchArtifact.RelativeRootPath.Should().Match($"{taskOutputDirectory}");
                }
                else
                {
                    throw new Exception("Input artifact variable not recognised!");
                }
            }
        }

        public void AssertOutputDirectory(TaskExecution task, string payloadId, string workflowInstanceId)
        {
            task.OutputDirectory.Should().Match($"{payloadId}/workflows/{workflowInstanceId}/{task.ExecutionId}");
        }

        public void AssertOutputArtifactsForTaskUpdate(Dictionary<string, string> workflowInstanceTaskOutput, List<Messaging.Common.Storage> taskUpdateOutput)
        {
            foreach (var artifact in taskUpdateOutput)
            {
                var workflowInstanceTaskArtifact = workflowInstanceTaskOutput.First(x => x.Key.Equals(artifact.Name));

                if (workflowInstanceTaskArtifact.Key != null)
                {
                    workflowInstanceTaskArtifact.Key.Should().Be(artifact.Name);
                    workflowInstanceTaskArtifact.Value.Should().Be(artifact.RelativeRootPath);
                }
                else
                {
                    throw new Exception($"Output Artifact name={artifact.Name} was not found in Task!");
                }
            }
        }

        public void AssertOutputArtifactsForTaskDispatch(List<Messaging.Common.Storage> taskDispatchOuput, string payloadId, string workflowInstanceId, string executionId)
        {
            foreach (var output in taskDispatchOuput)
            {
                output.RelativeRootPath.Should().Match($"{payloadId}/workflows/{workflowInstanceId}/{executionId}");
            }
        }

        public void AssertTaskDispatchEvent(TaskDispatchEvent taskDispatchEvent, WorkflowInstance workflowInstance, WorkflowRevision workflowRevision, WorkflowRequestMessage? workflowRequestMessage = null, TaskUpdateEvent? taskUpdateEvent = null)
        {
            var workflowInstanceTask = workflowInstance.Tasks.FirstOrDefault(x => x.TaskId.Equals(taskDispatchEvent.TaskId, StringComparison.OrdinalIgnoreCase));

            var workflowRevisionTask = workflowRevision?.Workflow?.Tasks.FirstOrDefault(x => x.Id.Equals(taskDispatchEvent.TaskId, StringComparison.OrdinalIgnoreCase));

            if (workflowRequestMessage != null)
            {
                taskDispatchEvent.CorrelationId.Should().Match(workflowRequestMessage.CorrelationId);
            }
            else
            {
                taskDispatchEvent.CorrelationId.Should().Match(taskUpdateEvent?.CorrelationId);
            }

            taskDispatchEvent.WorkflowInstanceId.Should().Match(workflowInstance.Id);
            taskDispatchEvent.TaskId.Should().Match(workflowInstanceTask?.TaskId);
            taskDispatchEvent.PayloadId.Should().Match(workflowInstance.PayloadId);
            taskDispatchEvent.ExecutionId.Should().Match(workflowInstanceTask?.ExecutionId);

            var previousTaskExecution = workflowInstance.Tasks.FirstOrDefault(x => x.TaskId.Equals
                (workflowInstance?.Tasks?.FirstOrDefault(x => !string.IsNullOrEmpty(x.PreviousTaskId))?.PreviousTaskId));

            if (taskDispatchEvent.Inputs.Count > 0)
            {
                if (taskUpdateEvent != null)
                {
                    AssertInputArtifactsForTaskDispatch(workflowRevisionTask,
                        workflowInstance.PayloadId,
                        taskDispatchEvent.Inputs,
                        taskUpdateEvent,
                        previousTaskExecution?.OutputDirectory);
                }
                else
                {
                    AssertInputArtifactsForTaskDispatch(workflowRevisionTask,
                        workflowInstance.PayloadId,
                        taskDispatchEvent.Inputs,
                        null,
                        previousTaskExecution?.OutputDirectory);
                }
            }

            if (taskDispatchEvent.Outputs.Count > 0)
            {
                AssertOutputArtifactsForTaskDispatch(taskDispatchEvent.Outputs, workflowInstance.PayloadId, workflowInstance.Id, workflowInstanceTask?.ExecutionId);
            }

            workflowInstanceTask?.Status.Should().Be(TaskExecutionStatus.Dispatched);
        }

        public void AssertExportRequestEvent(ExportRequestEvent exportRequestEvent, WorkflowInstance workflowInstance, WorkflowRevision workflowRevision, WorkflowRequestMessage? workflowRequestMessage = null, TaskUpdateEvent? taskUpdateEvent = null)
        {
            var workflowInstanceTask = workflowInstance.Tasks.FirstOrDefault(x => x.TaskId.Equals(exportRequestEvent.ExportTaskId, StringComparison.OrdinalIgnoreCase));
            var workflowRevisionTask = workflowRevision?.Workflow?.Tasks.FirstOrDefault(x => x.Id.Equals(exportRequestEvent.ExportTaskId, StringComparison.OrdinalIgnoreCase));

            if (workflowRequestMessage != null)
            {
                exportRequestEvent.CorrelationId.Should().Match(workflowRequestMessage.CorrelationId);
            }
            else
            {
                exportRequestEvent.CorrelationId.Should().Match(taskUpdateEvent?.CorrelationId);
            }

            workflowRevisionTask.ExportDestinations.Select(x => x.Name).ToArray().Should().BeEquivalentTo(exportRequestEvent.Destinations);
            var outputPath = taskUpdateEvent.Outputs[0].RelativeRootPath;
            var outputExtension = Path.GetExtension(taskUpdateEvent.Outputs[0].RelativeRootPath);

            var filesList = MinioClient.ListFilesFromDir(TestExecutionConfig.MinioConfig.Bucket, taskUpdateEvent.Outputs[0].RelativeRootPath).Result;
            var filteredFileList = filesList.Where(f => f.FilePath.EndsWith(".dcm")
            || f.FilePath.EndsWith(".DCM")
            || (string.IsNullOrWhiteSpace(Path.GetExtension(f.FilePath)) && string.IsNullOrWhiteSpace(f.FilePath) is false));
            exportRequestEvent.Files.Count().Should().Be(filteredFileList.Count());

            exportRequestEvent.WorkflowInstanceId.Should().Match(workflowInstance.Id);
            exportRequestEvent.ExportTaskId.Should().Match(workflowInstanceTask?.TaskId);
        }

        internal void AssertWorkflowInstanceAfterTaskUpdate(WorkflowInstance workflowInstance, TaskUpdateEvent taskUpdateEvent)
        {
            var workflowInstanceTask = workflowInstance.Tasks.Find(x => x.TaskId == taskUpdateEvent.TaskId);
            workflowInstanceTask?.Status.Should().Be(taskUpdateEvent.Status);
            workflowInstanceTask?.Reason.Should().Be(taskUpdateEvent.Reason);
            workflowInstanceTask?.OutputArtifacts.Should().BeEquivalentTo(workflowInstanceTask.OutputArtifacts);
            workflowInstanceTask?.ExecutionId.Should().Be(taskUpdateEvent.ExecutionId);
        }

        internal void AssertWorkflowInstanceAfterTaskDispatch(WorkflowInstance workflowInstance, TaskDispatchEvent taskDispatchEvent, WorkflowRevision workflowRevision)
        {
            var workflowInstanceTask = workflowInstance.Tasks.Find(x => x.TaskId == taskDispatchEvent.TaskId);
            var previousTaskExecution = workflowInstance.Tasks.Find(x => x.TaskId == workflowInstanceTask?.PreviousTaskId);
            var workflowRevisionTask = workflowRevision.Workflow.Tasks.FirstOrDefault(x => x.Id == taskDispatchEvent.TaskId);
            workflowInstanceTask?.Status.Should().Be(TaskExecutionStatus.Dispatched);
            workflowInstanceTask?.TaskType.Should().Be(workflowRevisionTask?.Type);
            AssertOutputDirectory(workflowInstanceTask, taskDispatchEvent.PayloadId, workflowInstance.Id);
            if (workflowInstanceTask.InputArtifacts.Any())
            {
                AssertInputArtifactsForWorkflowInstance(workflowRevisionTask, taskDispatchEvent.PayloadId, workflowInstanceTask, previousTaskExecution);
            }
        }

        public void AssertPayload(Payload payload, Payload? actualPayload)
        {
            actualPayload.Should().NotBeNull();
            actualPayload?.Should().BeEquivalentTo(payload, options => options.Excluding(x => x.Timestamp).Excluding(x => x.PatientDetails.PatientDob));
            actualPayload?.Timestamp.ToString("u").Should().Be(payload.Timestamp.ToString("u"));
            actualPayload?.PatientDetails.PatientDob?.ToString("u").Should().Be(payload.PatientDetails.PatientDob?.ToString("u"));
        }

        public void AssertPayloadList(List<Payload> payload, List<Payload>? actualPayloads)
        {
            actualPayloads.Should().NotBeNull();
            actualPayloads?.Count.Should().Be(payload.Count);

            foreach (var p in payload)
            {
                var actualPayload = actualPayloads?.FirstOrDefault(x => x.PayloadId.Equals(p.PayloadId));

                AssertPayload(p, actualPayload);
            }
        }

        public void AssertPayloadCollection(Payload payloadCollection, PatientDetails patientDetails, WorkflowRequestMessage workflowRequestMessage)
        {
            payloadCollection.PayloadId.Should().Be(workflowRequestMessage.PayloadId.ToString());
            payloadCollection.Bucket.Should().Be(workflowRequestMessage.Bucket);
            payloadCollection.CallingAeTitle.Should().Be(workflowRequestMessage.CallingAeTitle);
            payloadCollection.CalledAeTitle.Should().Be(workflowRequestMessage.CalledAeTitle);
            payloadCollection.CorrelationId.Should().Be(workflowRequestMessage.CorrelationId);
            payloadCollection.Timestamp.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromMinutes(1));
            payloadCollection.PatientDetails.Should().BeEquivalentTo(patientDetails);
        }

        public void AssertPayloadWorkflowInstanceId(Payload payloadCollection, List<WorkflowInstance> workflowInstances)
        {
            foreach (var workflowInstance in workflowInstances)
            {
                payloadCollection.WorkflowInstanceIds.Should().Contain(workflowInstance.Id.ToString());
            }
        }

        public void AssertWorkflowIstanceMatchesExpectedTaskStatusUpdate(WorkflowInstance updatedWorkflowInstance, TaskExecutionStatus taskExecutionStatus)
        {
            updatedWorkflowInstance.Tasks[0].Status.Should().Be(taskExecutionStatus);
        }

        public static void AssertPagination<T>(int count, string? queries, T? Response)
        {
            var responseType = Response?.GetType();
            GetPropertyValues(Response, responseType, out var data, out var totalPages, out var pageSize, out var totalRecords, out var pageNumber);
            var pageNumberQuery = 1;
            var pageSizeQuery = 10;

            if (string.IsNullOrWhiteSpace(queries) is false)
            {
                var splitQuery = queries?.Split("&") ?? Array.Empty<string>();
                foreach (var query in splitQuery)
                {
                    if (query.Contains("status=") || query.Contains("payloadId="))
                    {
                        continue;
                    }
                    else if (query.Contains("pageNumber") && int.TryParse(query.Split("=")[1], out var pageNumberResult))
                    {
                        pageNumberQuery = pageNumberResult;
                    }
                    else if (query.Contains("pageSize") && int.TryParse(query.Split("=")[1], out var pageSizeResult))
                    {
                        pageSizeQuery = pageSizeResult;
                    }
                    else
                    {
                        throw new Exception($"Pagination query {query} is not valid");
                    }
                }
            }

            AssertDataCount(data, pageNumberQuery, pageSizeQuery, count);
            AssertTotalPages(totalPages, count, pageSizeQuery);
            totalRecords.Should().Be(count);
            pageNumber.Should().Be(pageNumberQuery);
            pageSize.Should().Be(pageSizeQuery);
        }

        private static void GetPropertyValues<T>(T? Response, Type? responseType, out ICollection? data, out object? totalPages, out object? pageSize, out object? totalRecords, out object? pageNumber)
        {
            data = responseType?.GetProperty("Data")?.GetValue(Response, null) as ICollection;
            totalPages = responseType?.GetProperty("TotalPages")?.GetValue(Response, null);
            pageSize = responseType?.GetProperty("PageSize")?.GetValue(Response, null);
            totalRecords = responseType?.GetProperty("TotalRecords")?.GetValue(Response, null);
            pageNumber = responseType?.GetProperty("PageNumber")?.GetValue(Response, null);
        }

        public void WorkflowInstanceIncludesTaskDetails(List<TaskDispatchEvent> taskDispatchEvents, WorkflowInstance workflowInstance, WorkflowRevision workflowRevision)
        {
            foreach (var taskDispatchEvent in taskDispatchEvents)
            {
                var workflowInstanceTaskDetails = workflowInstance.Tasks.FirstOrDefault(c => c.TaskId.Equals(taskDispatchEvent.TaskId));
                var workflowTaskDetails = workflowRevision?.Workflow?.Tasks.FirstOrDefault(c => c.Id.Equals(taskDispatchEvent.TaskId));
                workflowInstanceTaskDetails.Should().NotBeNull();
                workflowTaskDetails.Should().NotBeNull();
                workflowInstanceTaskDetails?.ExecutionId.Should().Be(taskDispatchEvent.ExecutionId);
                workflowInstanceTaskDetails?.Status.Should().Be(TaskExecutionStatus.Dispatched);
                workflowInstanceTaskDetails?.TaskType.Should().Be(workflowTaskDetails?.Type);
            }
        }

        public void WorkflowInstanceStatus(string status, WorkflowInstance workflowInstance)
        {
            workflowInstance.Status.Should().Be((Status)Enum.Parse(typeof(Status), status));
        }

        public void AssertWorkflowList(List<WorkflowRevision> expectedWorkflowRevisions, List<WorkflowRevision> actualWorkflowRevisions)
        {
            actualWorkflowRevisions.Should().HaveCount(expectedWorkflowRevisions.Count);
            expectedWorkflowRevisions.OrderBy(x => x.Id).Should().BeEquivalentTo(actualWorkflowRevisions.OrderBy(x => x.Id));
        }

        public void AssertWorkflowRevisionDetailsAfterUpdateRequest(List<WorkflowRevision> actualWorkflowRevisions, List<Workflow> workflowUpdate, List<WorkflowRevision> originalWorkflowRevisions)
        {
            actualWorkflowRevisions.Count.Should().Be(originalWorkflowRevisions.Count + 1);

            foreach (var originalWorkflowRevision in originalWorkflowRevisions)
            {
                var actualWorkflowRevision = actualWorkflowRevisions.FirstOrDefault(x => x.Revision.Equals(originalWorkflowRevision.Revision));
                actualWorkflowRevision.Should().BeEquivalentTo(originalWorkflowRevision, o => o.Excluding(x => x.Deleted).Excluding(x => x.IsDeleted));
                actualWorkflowRevision?.Deleted.Should().NotBeNull();
                actualWorkflowRevision?.IsDeleted.Should().BeTrue();
            }

            var actualWorkflow = actualWorkflowRevisions[actualWorkflowRevisions.Count - 1].Workflow;

            actualWorkflowRevisions[actualWorkflowRevisions.Count - 1].Revision.Should().Be(originalWorkflowRevisions[originalWorkflowRevisions.Count - 1].Revision + 1);

            actualWorkflow.Should().BeEquivalentTo(workflowUpdate[0]);
        }

        internal void AssertWorkflowMarkedAsDeleted(List<WorkflowRevision> workflowRevisions)
        {
            foreach (var workflowRevision in workflowRevisions)
            {
                workflowRevision.IsDeleted.Should().BeTrue();
            }
        }

        public void AssertWorkflowInstanceList(List<WorkflowInstance> expectedWorkflowInstances, List<WorkflowInstance> actualWorkflowInstances)
        {
            actualWorkflowInstances.Should().HaveCount(expectedWorkflowInstances.Count);
            foreach (var actualWorkflowInstance in actualWorkflowInstances)
            {
                var expectedWorkflowInstance = expectedWorkflowInstances.FirstOrDefault(x => x.Id.Equals(actualWorkflowInstance.Id));
                actualWorkflowInstance.StartTime.ToString(format: "yyyy-MM-dd hh:mm:ss").Should().Be(expectedWorkflowInstance?.StartTime.ToString(format: "yyyy-MM-dd hh:mm:ss"));
            }
            actualWorkflowInstances.OrderBy(x => x.Id).Should().BeEquivalentTo(expectedWorkflowInstances.OrderBy(x => x.Id),
                options => options.Excluding(x => x.StartTime));
        }

        public void AssertWorkflowInstance(List<WorkflowInstance> expectedWorkflowInstances, WorkflowInstance? actualWorkflowInstance)
        {
            var expectedWorkflowInstance = expectedWorkflowInstances.FirstOrDefault(x => x.Id.Equals(actualWorkflowInstance?.Id));
            actualWorkflowInstance?.StartTime.ToString(format: "yyyy-MM-dd hh:mm:ss").Should().Be(expectedWorkflowInstance?.StartTime.ToString(format: "yyyy-MM-dd hh:mm:ss"));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            actualWorkflowInstance.Should().BeEquivalentTo(expectedWorkflowInstance, options => options.Excluding(x => x.StartTime));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        private static void AssertDataCount(ICollection? Data, int pageNumberQuery, int pageSizeQuery, int count)
        {
            if ((pageNumberQuery * pageSizeQuery) > count)
            {
                Data?.Count.Should().Be(Math.Max(count - ((pageNumberQuery - 1) * pageSizeQuery), 0));
            }
            else if ((pageNumberQuery * pageSizeQuery) < count)
            {
                Data?.Count.Should().Be(pageSizeQuery);
            }
            else if (pageNumberQuery > 1)
            {
                Data?.Count.Should().Be(Math.Max(count - (pageSizeQuery * (pageNumberQuery - 1)), 0));
            }
            else
            {
                Data?.Count.Should().Be(count);
            }
        }

        private static void AssertTotalPages(object? TotalPages, int count, int pageSizeQuery)
        {
            int remainder;
            int quotient = Math.DivRem(count, pageSizeQuery, out remainder);

            if (remainder == 0)
            {
                TotalPages.Should().Be(quotient);
            }
            else
            {
                TotalPages.Should().Be(quotient + 1);
            }
        }
    }
}
