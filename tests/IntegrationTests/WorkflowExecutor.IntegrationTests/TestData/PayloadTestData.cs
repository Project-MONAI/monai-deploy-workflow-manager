using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.WorkflowExecutor.IntegrationTests.TestData
{
    public class PayloadTestData
    {
        public string? Name { get; set; }

        public Payload? Payload { get; set; }
    }

    public static class PayloadsTestData
    {
        public static List<PayloadTestData> TestData = new List<PayloadTestData>()
        {
            new PayloadTestData()
            {
                Name = "Payload_Full_Patient",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    CalledAeTitle = "MIG",
                    CallingAeTitle = "Basic_AE",
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = "c5c3636b-81dd-44a9-8c4b-71adec7d47b2",
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05),
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Steve Jobs",
                        PatientSex = "male"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_Partial_Patient",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    CalledAeTitle = "MIG",
                    CallingAeTitle = "Basic_AE",
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = "86c0f117-4021-412e-b163-0dc621df672a",
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 3,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = null,
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Jane Doe",
                        PatientSex = "female"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_Null_Patient",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_2",
                    CalledAeTitle = "MIG",
                    CallingAeTitle = "Basic_AE",
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = "30a8e0c6-e6c4-458f-aa4d-b224b493d3c0",
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 3,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = null,
                        PatientId = null,
                        PatientName = null,
                        PatientSex = null
                    }
                }
            }
        };
    }
}
