﻿// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using BoDi;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.Wrappers;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowApiStepDefinitions
    {
        public WorkflowApiStepDefinitions(ObjectContainer objectContainer)
        {
            DataHelper = objectContainer.Resolve<DataHelper>();
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            ApiHelper = objectContainer.Resolve<ApiHelper>();
            Assertions = new Assertions();
        }

        private ApiHelper ApiHelper { get; }
        private Assertions Assertions { get; }
        private DataHelper DataHelper { get; }
        private MongoClientUtil MongoClient { get; }

        [Then(@"I can see (.*) workflows are returned")]
        [Then(@"I can see (.*) workflow is returned")]
        public void ThenICanSeeWorkflowsAreReturned(int count)
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var workflowRevisions = JsonConvert.DeserializeObject<PagedResponse<List<WorkflowRevision>>>(result);
            Assertions.AssertWorkflowList(DataHelper.WorkflowRevisions, workflowRevisions.Data);
        }

        [Then(@"Pagination is working correctly for the (.*) workflow")]
        [Then(@"Pagination is working correctly for the (.*) workflows")]
        public void ThenPaginationIsWorkingCorrectlyForTheWorkflow(int count)
        {
            var request = ApiHelper.Request.RequestUri.Query;
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var deserializedResult = JsonConvert.DeserializeObject<PagedResponse<List<WorkflowRevision>>>(result);
            Assertions.AssertPagination<PagedResponse<List<WorkflowRevision>>>(count, request, deserializedResult);
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

            workflowRevisions.Data.Should().BeNullOrEmpty();
        }
    }
}
