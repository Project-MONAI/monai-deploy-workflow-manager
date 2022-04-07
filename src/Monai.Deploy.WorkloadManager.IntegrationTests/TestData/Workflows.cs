using Monai.Deploy.WorkloadManager.IntegrationTests.Models;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.TestData
{
    public class WorkflowTestData
    {
        public string TestName { get; set; }

        public Workflow Workflow { get; set; }
    }

    public static class Workflows
    {
        public static List<WorkflowTestData> WorkflowTestData = new List<WorkflowTestData>()
        {
            new WorkflowTestData
            {
                TestName = "WorkflowEvent_1",
                Workflow = new Workflow
                {
                    Description = "WorkflowEvent_1_Description",
                    Name = "WorkflowEvent_1_Name",
                    Version = "WorkflowEvent_1_Version"
                }
            }
        };
    }
}
