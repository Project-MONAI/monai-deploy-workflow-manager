// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using BoDi;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowApiStepDefinitions
    {
        public WorkflowApiStepDefinitions(ObjectContainer objectContainer)
        {
            var httpClient = objectContainer.Resolve<HttpClient>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            ApiHelper = new ApiHelper(httpClient);
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
            var workflowRevisions = JsonConvert.DeserializeObject<List<WorkflowRevision>>(result);
            Assertions.AssertWorkflowList(DataHelper.WorkflowRevisions, workflowRevisions);
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
            result.Should().Be("[]");
        }
    }
}
