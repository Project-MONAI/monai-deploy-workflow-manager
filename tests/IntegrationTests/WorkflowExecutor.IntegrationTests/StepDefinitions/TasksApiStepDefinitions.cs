using BoDi;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.WorkflowExecutor.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TasksApiStepDefinitions
    {
        public TasksApiStepDefinitions(ObjectContainer objectContainer)
        {
            DataHelper = objectContainer.Resolve<DataHelper>();
            ApiHelper = objectContainer.Resolve<ApiHelper>();
            Assertions = new Assertions(objectContainer);
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
    }
}
