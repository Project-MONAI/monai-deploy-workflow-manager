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

using System.Globalization;
using BoDi;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.Wrappers;
using MongoDB.Driver;
using Newtonsoft.Json;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowInstancesApiStepDefinitions
    {
        public WorkflowInstancesApiStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            DataHelper = objectContainer.Resolve<DataHelper>();
            ApiHelper = objectContainer.Resolve<ApiHelper>();
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            Assertions = new Assertions(objectContainer);
            _outputHelper = outputHelper;
        }

        private ApiHelper ApiHelper { get; }
        private Assertions Assertions { get; }
        private DataHelper DataHelper { get; }
        private readonly ISpecFlowOutputHelper _outputHelper;
        private MongoClientUtil MongoClient { get; set; }

        [Then(@"I can see expected workflow instances are returned")]
        public void ThenICanSeeExpectedWorkflowInstancesAreReturned()
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var actualWorkflowInstances = JsonConvert.DeserializeObject<PagedResponse<List<WorkflowInstance>>>(result);
            Assertions.AssertWorkflowInstanceList(DataHelper.WorkflowInstances, actualWorkflowInstances.Data);
        }

        [Then(@"I can see expected workflow instance is returned")]
        public void ThenICanSeeExpectedWorkflowInstanceIsReturned()
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var actualWorkflowInstance = JsonConvert.DeserializeObject<WorkflowInstance>(result);
            Assertions.AssertWorkflowInstance(DataHelper.WorkflowInstances, actualWorkflowInstance);
        }

        [Then(@"I can see (.*) expected workflow instance is returned")]
        public void ThenICanSeeExpectedWorkflowInstanceIsReturned(int count)
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var actualWorkflowInstance = JsonConvert.DeserializeObject<WorkflowInstance>(result);
            Assertions.AssertWorkflowInstance(DataHelper.WorkflowInstances, actualWorkflowInstance);
        }

        [Then(@"Pagination is working correctly for the (.*) workflow instance")]
        [Then(@"Pagination is working correctly for the (.*) workflow instances")]
        public void ThenPaginationIsWorkingCorrectlyForTheWorkflowInstance(int count)
        {
            var request = ApiHelper.Request.RequestUri?.Query;
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var deserializedResult = JsonConvert.DeserializeObject<PagedResponse<List<WorkflowInstance>>>(result);
            Assertions.AssertPagination(count, request, deserializedResult);
        }

        [Then(@"All results have correct (.*) and (.*)")]
        public void AllResultsHaveExpectedStatusOrPayloadId(int? expected_status, string? expected_payloadId)
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var deserializedResult = JsonConvert.DeserializeObject<PagedResponse<List<WorkflowInstance>>>(result);

            Action<WorkflowInstance> func = wi => { };
            if (string.IsNullOrWhiteSpace(expected_payloadId) is false)
            {
                func += wi => wi.PayloadId.Should().Be(expected_payloadId);
            }
            if (expected_status is not null)
            {
                func += wi => wi.Status.Should().Be((Status)expected_status);
            }

            deserializedResult.Should().NotBeNull();
            deserializedResult?.Data.ForEach(func);
        }

        [Then(@"I can see (.*) triggered workflow instances from payload id (.*)")]
        public void ThenICanSeeTriggeredWorkflowInstancesFromPayloadId(int count, string payloadId)
        {
            _outputHelper.WriteLine($"Retrieving {count} workflow instance/s using the payloadid={payloadId}");
            var workflowInstances = DataHelper.GetWorkflowInstances(count, payloadId);
            _outputHelper.WriteLine($"Retrieved {count} workflow instance/s");
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;

            if (workflowInstances != null)
            {
                if (ApiHelper.Request.RequestUri.ToString().Contains("disablePagination"))
                {
                    var actualWorkflowInstances = JsonConvert.DeserializeObject<List<WorkflowInstance>>(result);
                    if (actualWorkflowInstances != null)
                    {
                        Assertions.AssertWorkflowInstanceList(workflowInstances, actualWorkflowInstances);
                    }
                    else
                    {
                        throw new Exception("Api response could not be deserialized by the <List<WorkflowInstance>> object");
                    }
                }
                else
                {
                    var actualWorkflowInstances = JsonConvert.DeserializeObject<PagedResponse<List<WorkflowInstance>>>(result);
                    if (actualWorkflowInstances != null)
                    {
                        Assertions.AssertWorkflowInstanceList(workflowInstances, actualWorkflowInstances.Data);
                    }
                    else
                    {
                        throw new Exception("Api response could not be deserialized by the <PagedResponseList<WorkflowInstance>> object");
                    }
                }
            }
            else
            {
                throw new Exception($"Workflow Instance not found for payloadId {payloadId}");
            }
        }

        [Then(@"I can see (.*) failed workflow instances since (.*)")]
        public void ThenICanSeeFailedWorkflowInstancesSince(int count, string dateTime)
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var parseResult = DateTime.TryParse(dateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dateTimeParsed);
            if (parseResult is false)
            {
                throw new Exception("Bad date time provided");
            }

            var expectedData = DataHelper.SeededWorkflowInstances.Where(wfInstance => wfInstance.Status == Status.Failed
                                      && wfInstance.AcknowledgedWorkflowErrors.HasValue
                                      && wfInstance.AcknowledgedWorkflowErrors.Value > dateTimeParsed).ToList();
            expectedData.Count.Should().Be(count);

            var actualWorkflowInstances = JsonConvert.DeserializeObject<List<WorkflowInstance>>(result);

            actualWorkflowInstances.Should().NotBeNull();
            actualWorkflowInstances?.Count.Should().Be(count);
            Assertions.AssertWorkflowInstanceList(expectedData, actualWorkflowInstances
                ?? throw new Exception("No workflow instance data returned"));
        }

        [Then(@"I will receive no pagination response")]
        public void ThenIWillReceiveNoPaginationResponse()
        {
            var response = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            response.Should().NotContainAny(new List<string>
            {
                "pageNumber",
                "pageSize",
                "firstPage",
                "lastPage",
                "totalPages",
                "totalRecords",
                "nextPage",
                "previousPage"
            });
        }

        [Then(@"I can see the Task (.*) error is acknowledged on workflow instance (.*)")]
        public void ThenICanSeeTheTaskIsAcknowledgedOnWorkflowInstance(string taskId, string workflowInstanceName)
        {
            string? workflowInstanceId = null;
            WorkflowInstance workflowInstance = DataHelper.GetWorkflowInstanceTestData(workflowInstanceName);
            if (workflowInstance != null)
            {
                workflowInstanceId = workflowInstance.Id;
            }
            else
            {
                throw new Exception($"Workflow instance {workflowInstanceName} does not exist");
            };

            _outputHelper.WriteLine($"Retrieving workflow instance by id={workflowInstanceId}");
            var updatedWorkflowInstance = MongoClient.GetWorkflowInstanceById(workflowInstanceId);
            _outputHelper.WriteLine("Retrieved workflow instance");

            var task = updatedWorkflowInstance.Tasks.FirstOrDefault(i => i.TaskId.Equals(taskId));
            task.AcknowledgedTaskErrors.Should().BeCloseTo(DateTime.UtcNow, new TimeSpan(50000000));
        }

        [Then(@"I can see the workflow Instance (.*) error is acknowledged")]
        public void ThenICanSeeTheWorkflowInstanceErrorIsAcknowledged(string workflowInstanceName)
        {
            string? workflowInstanceId = null;
            WorkflowInstance workflowInstance = DataHelper.GetWorkflowInstanceTestData(workflowInstanceName);
            if (workflowInstance != null)
            {
                workflowInstanceId = workflowInstance.Id;
            }
            else
            {
                throw new Exception($"Workflow instance {workflowInstanceName} does not exist");
            };

            _outputHelper.WriteLine($"Retrieving workflow instance by id={workflowInstanceId}");
            var updatedWorkflowInstance = MongoClient.GetWorkflowInstanceById(workflowInstanceId);
            _outputHelper.WriteLine("Retrieved workflow instance");
            updatedWorkflowInstance.AcknowledgedWorkflowErrors.Should().BeCloseTo(DateTime.UtcNow, new TimeSpan(50000000));
        }

        [Then(@"I can see the workflow Instance (.*) error is not acknowledged")]
        public void ThenICanSeeTheWorkflowInstanceErrorIsNotAcknowledged(string workflowInstanceName)
        {
            string? workflowInstanceId = null;
            WorkflowInstance workflowInstance = DataHelper.GetWorkflowInstanceTestData(workflowInstanceName);
            if (workflowInstance != null)
            {
                workflowInstanceId = workflowInstance.Id;
            }
            else
            {
                throw new Exception($"Workflow instance {workflowInstanceName} does not exist");
            };

            _outputHelper.WriteLine($"Retrieving workflow instance by id={workflowInstanceId}");
            var updatedWorkflowInstance = MongoClient.GetWorkflowInstanceById(workflowInstanceId);
            _outputHelper.WriteLine("Retrieved workflow instance");
            updatedWorkflowInstance.AcknowledgedWorkflowErrors.Should().Be(null);
        }
    }
}
