using Monai.Deploy.WorkloadManager.IntegrationTests.Models;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.TestData
{
    public class WorkflowTestData
    {
        public string TestDescription { get; set; }

        public Workflow Workflow { get; set; }
    }

    public static class Workflows
    {
        public static List<Workflow> WorkflowTestData = new List<Workflow>()
        {
            new Workflow
            {
                Description = "Test_Description",
                Name = "Test_Name",
                Version = "Test_Version"
            }
        };
    }
}
