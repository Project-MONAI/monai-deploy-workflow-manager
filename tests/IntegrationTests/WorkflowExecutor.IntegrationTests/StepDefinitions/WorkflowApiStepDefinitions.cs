/*
 * Copyright 2023 MONAI Consortium
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
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;
using Newtonsoft.Json;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowApiStepDefinitions
    {
        public WorkflowApiStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            DataHelper = objectContainer.Resolve<DataHelper>();
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            ApiHelper = objectContainer.Resolve<ApiHelper>();
            Assertions = new Assertions(objectContainer, outputHelper);
        }

        private ApiHelper ApiHelper { get; }
        private Assertions Assertions { get; }
        private DataHelper DataHelper { get; }
        private MongoClientUtil MongoClient { get; }

        [Then(@"I can see the expected workflows are returned")]
        public void ThenICanSeeWorkflowsAreReturned()
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var workflowRevisions = JsonConvert.DeserializeObject<PagedResponse<List<WorkflowRevision>>>(result);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
            Assertions.AssertWorkflowList(DataHelper.WorkflowRevisions, workflowRevisions.Data);

        }

        [Then(@"Pagination is working correctly for the (.*) workflow")]
        [Then(@"Pagination is working correctly for the (.*) workflows")]
        public void ThenPaginationIsWorkingCorrectlyForTheWorkflow(int count)
        {
            var request = ApiHelper.Request.RequestUri.Query;
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var deserializedResult = JsonConvert.DeserializeObject<PagedResponse<List<WorkflowRevision>>>(result);
            Assertions.AssertPagination(count, request, deserializedResult);
        }

        [Then(@"the Workflow Id (.*) is returned in the response body")]
        public void ThenTheWorkflowIdIsReturned(string id)
        {
            ApiHelper.Response.Content.ReadAsStringAsync().Result.Should().Be($"{{\"workflow_id\":\"{id}\"}}");
        }

        [Then(@"multiple workflow revisions now exist with correct details")]
        public void ThenMultipleWorkflowRevisionNowExistWithCorrectDetails()
        {
            var workflowRevisions = MongoClient.GetWorkflowRevisionsByWorkflowId(DataHelper.WorkflowRevisions[0].WorkflowId);
            Assertions.AssertWorkflowRevisionDetailsAfterUpdateRequest(workflowRevisions, DataHelper.Workflows, DataHelper.WorkflowRevisions);
        }

        [Then(@"all revisions of the workflow are marked as deleted")]
        public void ThenAllRevisionsOfTheWorkflowAreMarkedAsDeleted()
        {
            var workflowRevisions = MongoClient.GetWorkflowRevisionsByWorkflowId(DataHelper.WorkflowRevisions[0].WorkflowId);
            Assertions.AssertWorkflowMarkedAsDeleted(workflowRevisions);
        }

        [Then(@"the deleted workflow is not returned")]
        public void ThenTheDeletedWorkflowIsNotReturned()
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var workflowRevisions = JsonConvert.DeserializeObject<PagedResponse<List<WorkflowRevision>>>(result);
            workflowRevisions?.Data.Should().BeNullOrEmpty();
        }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
