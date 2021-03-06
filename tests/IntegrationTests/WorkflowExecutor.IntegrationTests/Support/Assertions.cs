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
using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Models;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public class Assertions
    {
        public void AssertWorkflowInstanceMatchesExpectedWorkflow(WorkflowInstance workflowInstance, WorkflowRevision workflowRevision, WorkflowRequestMessage workflowRequestMessage)
        {
            workflowInstance.PayloadId.Should().Match(workflowRequestMessage.PayloadId.ToString());
            workflowInstance.WorkflowId.Should().Match(workflowRevision.WorkflowId);
            workflowInstance.AeTitle.Should().Match(workflowRevision.Workflow.InformaticsGateway.AeTitle);

            foreach (var task in workflowInstance.Tasks)
            {
                var workflowTask = workflowRevision.Workflow.Tasks.FirstOrDefault(x => x.Id.Equals(task.TaskId));
                if (workflowTask != null)
                {
                    task.TaskId.Should().Match(workflowTask.Id);
                    task.TaskType.Should().Match(workflowTask.Type);
                }
                else
                {
                    throw new Exception($"Workflow Revision Task or {task.TaskId} not found!");
                }
            }
        }

        public void AssertTaskDispatchEvent(TaskDispatchEvent taskDispatchEvent, WorkflowInstance workflowInstance, WorkflowRevision workflowRevision, WorkflowRequestMessage workflowRequestMessage = null, TaskUpdateEvent taskUpdateEvent = null)
        {
            var taskDetails = workflowInstance.Tasks.FirstOrDefault(c => c.TaskId.Equals(taskDispatchEvent.TaskId));

            if (workflowRequestMessage != null)
            {
                taskDispatchEvent.CorrelationId.Should().Match(workflowRequestMessage.CorrelationId);
            }
            else
            {
                taskDispatchEvent.CorrelationId.Should().Match(taskUpdateEvent.CorrelationId);
            }

            taskDispatchEvent.WorkflowInstanceId.Should().Match(workflowInstance.Id);

            taskDispatchEvent.TaskId.Should().Match(taskDetails.TaskId);

            taskDetails.Status.Should().Be(TaskExecutionStatus.Dispatched);
        }

        public void AssertPayload(Payload payload, Payload? actualPayload)
        {
            actualPayload.Should().BeEquivalentTo(payload, options => options.Excluding(x => x.Timestamp));
            actualPayload.Timestamp.ToString(format: "yyyy-MM-dd hh:mm:ss").Should().Be(payload.Timestamp.ToString(format: "yyyy-MM-dd hh:mm:ss"));
        }

        public void AssertPayloadList(List<Payload> payload, List<Payload>? actualPayloads)
        {
            actualPayloads.Count.Should().Be(payload.Count);

            foreach (var p in payload)
            {
                var actualPayload = actualPayloads.FirstOrDefault(x => x.PayloadId.Equals(p.PayloadId));

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

        public void AssertWorkflowIstanceMatchesExpectedTaskStatusUpdate(WorkflowInstance updatedWorkflowInstance, TaskExecutionStatus taskExecutionStatus)
        {
            updatedWorkflowInstance.Tasks[0].Status.Should().Be(taskExecutionStatus);
        }

        public void AssertPagination<T>(int count, string queries, T Response)
        {
            var data = Response.GetType().GetProperty("Data").GetValue(Response, null) as ICollection;
            var totalPages = Response.GetType().GetProperty("TotalPages").GetValue(Response, null);
            var pageSize = Response.GetType().GetProperty("PageSize").GetValue(Response, null);
            var totalRecords = Response.GetType().GetProperty("TotalRecords").GetValue(Response, null);
            var pageNumber = Response.GetType().GetProperty("PageNumber").GetValue(Response, null);
            int pageNumberQuery = 1;
            int pageSizeQuery = 10;
            List<string> splitQuery = queries.Split("&").ToList();
            if (queries != "")
            {
                foreach (var query in splitQuery)
                {
                    if (query.Contains("pageNumber"))
                    {
                        pageNumberQuery = Int32.Parse(query.Split("=")[1]);
                    }
                    else if (query.Contains("pageSize"))
                    {
                        pageSizeQuery = Int32.Parse(query.Split("=")[1]);
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

        public void WorkflowInstanceIncludesTaskDetails(List<TaskDispatchEvent> taskDispatchEvents, WorkflowInstance workflowInstance, WorkflowRevision workflowRevision)
        {
            foreach (var taskDispatchEvent in taskDispatchEvents)
            {
                var workflowInstanceTaskDetails = workflowInstance.Tasks.FirstOrDefault(c => c.TaskId.Equals(taskDispatchEvent.TaskId));
                var workflowTaskDetails = workflowRevision.Workflow.Tasks.FirstOrDefault(c => c.Id.Equals(taskDispatchEvent.TaskId));
                workflowInstanceTaskDetails.ExecutionId.Should().Be(taskDispatchEvent.ExecutionId);
                workflowInstanceTaskDetails.Status.Should().Be(TaskExecutionStatus.Dispatched);
                workflowInstanceTaskDetails.TaskType.Should().Be(workflowTaskDetails.Type);
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
                actualWorkflowRevision.Should().BeEquivalentTo(originalWorkflowRevision);
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
                actualWorkflowInstance.StartTime.ToString(format: "yyyy-MM-dd hh:mm:ss").Should().Be(expectedWorkflowInstance.StartTime.ToString(format: "yyyy-MM-dd hh:mm:ss"));
            }
            actualWorkflowInstances.OrderBy(x => x.Id).Should().BeEquivalentTo(expectedWorkflowInstances.OrderBy(x => x.Id),
                options => options.Excluding(x => x.StartTime));
        }

        public void AssertWorkflowInstance(List<WorkflowInstance> expectedWorkflowInstances, WorkflowInstance? actualWorkflowInstance)
        {
            var expectedWorkflowInstance = expectedWorkflowInstances.FirstOrDefault(x => x.Id.Equals(actualWorkflowInstance.Id));
            actualWorkflowInstance.StartTime.ToString(format: "yyyy-MM-dd hh:mm:ss").Should().Be(expectedWorkflowInstance.StartTime.ToString(format: "yyyy-MM-dd hh:mm:ss"));
            actualWorkflowInstance.Should().BeEquivalentTo(expectedWorkflowInstance, options => options.Excluding(x => x.StartTime));
        }

        private static void AssertDataCount(ICollection Data, int pageNumberQuery, int pageSizeQuery, int count)
        {
            if ((pageNumberQuery * pageSizeQuery) > count)
            {
                Data.Count.Should().Be(Math.Max(count - ((pageNumberQuery - 1) * pageSizeQuery), 0));
            }
            else if ((pageNumberQuery * pageSizeQuery) < count)
            {
                Data.Count.Should().Be(pageSizeQuery);
            }
            else if (pageNumberQuery > 1)
            {
                Data.Count.Should().Be(Math.Max(count - (pageSizeQuery * (pageNumberQuery - 1)), 0));
            }
            else
            {
                Data.Count.Should().Be(count);
            }
        }

        private static void AssertTotalPages(object TotalPages, int count, int pageSizeQuery)
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
