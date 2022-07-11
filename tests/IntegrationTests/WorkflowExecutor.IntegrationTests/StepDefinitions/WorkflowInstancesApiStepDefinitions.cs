// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using BoDi;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowInstancesApiStepDefinitions
    {
        public WorkflowInstancesApiStepDefinitions(ObjectContainer objectContainer)
        {
            var httpClient = objectContainer.Resolve<HttpClient>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            ApiHelper = new ApiHelper(httpClient);
            Assertions = new Assertions();
        }

        private ApiHelper ApiHelper { get; }
        private Assertions Assertions { get; }
        private DataHelper DataHelper { get; }

        [Then(@"I can see expected workflow instances are returned")]
        public void ThenICanSeeExpectedWorkflowInstancesAreReturned()
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var actualWorkflowInstances = JsonConvert.DeserializeObject<List<WorkflowInstance>>(result);
            Assertions.AssertWorkflowInstanceList(DataHelper.WorkflowInstances, actualWorkflowInstances);
        }

        [Then(@"I can see expected workflow instance is returned")]
        public void ThenICanSeeExpectedWorkflowInstanceIsReturned()
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var actualWorkflowInstance = JsonConvert.DeserializeObject<WorkflowInstance>(result);
            Assertions.AssertWorkflowInstance(DataHelper.WorkflowInstances, actualWorkflowInstance);
        }
    }
}
