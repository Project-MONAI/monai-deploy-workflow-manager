using Monai.Deploy.WorkloadManager.IntegrationTests.Models;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.TestData
{
    public class WorkflowRequest
    {
        public string TestName { get; set; }

        public ExportMessageRequest ExportMessageRequest { get; set; }
    }

    public static class WorkflowRequests
    {
        public static List<WorkflowRequest> TestData = new List<WorkflowRequest>()
        {
            new WorkflowRequest
            {
                TestName = "ExportMessageRequest_1",
                ExportMessageRequest = new ExportMessageRequest
                {
                    Bucket = "bucket_1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { "workflow_1", "workflow_2" },
                    FileCount = 1,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "aetitlecalling_1",
                    CallingAeTitle = "aetitlecalled_1",
                }
            },
            new WorkflowRequest
            {
                TestName = "ExportMessageRequest_2",
                ExportMessageRequest = new ExportMessageRequest
                {
                    Bucket = "bucket_2",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { "workflow_3", "workflow_4" },
                    FileCount = 1,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "aetitlecalling_2",
                    CallingAeTitle = "aetitlecalled_2",
                }
            }
        };
    }
}
