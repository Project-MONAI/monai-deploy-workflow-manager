// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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
            Assertions = new Assertions();
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
            var request = ApiHelper.Request.RequestUri.Query;
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var deserializedResult = JsonConvert.DeserializeObject<PagedResponse<List<WorkflowInstance>>>(result);
            Assertions.AssertPagination<PagedResponse<List<WorkflowInstance>>>(count, request, deserializedResult);
        }
    }
}
