using Monai.Deploy.WorkloadManager.IntegrationTests.Models;
using MongoDB.Bson;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.TestData
{
    public class DummyDagModel
    {
        public string? TestName { get; set; }

        public DummyDag? DummyDag { get; set; }
    }

    public static class DummyDagTestData
    {
        public static DummyDagModel TestData = new DummyDagModel()
        {
            TestName = "Dag_Mongo_Connection",
            DummyDag = new DummyDag()
            {
                Id = new ObjectId("5c6ebfec6fd07954a4890683").ToString(),
                WorkflowName = "workflow_1",
                TaskName = "task_1"
            }
        };
    }
}
