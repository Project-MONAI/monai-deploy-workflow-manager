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
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.Wrappers;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowInstancesApiStepDefinitions
    {
        public WorkflowInstancesApiStepDefinitions(ObjectContainer objectContainer)
        {
            DataHelper = objectContainer.Resolve<DataHelper>();
            ApiHelper = objectContainer.Resolve<ApiHelper>();
            Assertions = new Assertions(objectContainer);
        }

        private ApiHelper ApiHelper { get; }
        private Assertions Assertions { get; }
        private DataHelper DataHelper { get; }

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
    }
}
