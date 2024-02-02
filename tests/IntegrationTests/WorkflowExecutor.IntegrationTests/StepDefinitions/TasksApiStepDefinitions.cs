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

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TasksApiStepDefinitions
    {
        public TasksApiStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            DataHelper = objectContainer.Resolve<DataHelper>();
            ApiHelper = objectContainer.Resolve<ApiHelper>();
            Assertions = new Assertions(objectContainer, outputHelper);
        }

        public DataHelper DataHelper { get; }
        public ApiHelper ApiHelper { get; }
        public Assertions Assertions { get; }

        [Then(@"I can see an individual task is returned")]
        public void ThenICanSeeAnIndividualTaskIsReturned()
        {
            var response = JsonConvert.DeserializeObject<TaskExecution>(ApiHelper.Response.Content.ReadAsStringAsync().Result);
            Assertions.AssertTaskPayload(DataHelper.WorkflowInstances, response);
        }

        [Then(@"I can see (.*) tasks are returned")]
        public void ThenICanSeeTasksAreReturned(int number)
        {
            var result = ApiHelper.Response.Content.ReadAsStringAsync().Result;
            var response = JsonConvert.DeserializeObject<PagedResponse<IList<TaskExecution>>>(result!);
            number.Should().Be(response!.Data!.Count);
        }
    }
}
