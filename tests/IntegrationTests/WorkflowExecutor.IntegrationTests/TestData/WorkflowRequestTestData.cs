// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.IntegrationTests.Models;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.TestData
{
    public class WorkflowRequestTestData
    {
        public string? Name { get; set; }

        public WorkflowRequestMessage? WorkflowRequestMessage { get; set; }
    }

    public static class WorkflowRequestsTestData
    {
        public static List<WorkflowRequestTestData> TestData = new List<WorkflowRequestTestData>()
        {
            new WorkflowRequestTestData
            {
                Name = "Basic_AeTitle_WF_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Basic_AE",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Routing_Id_WF_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { Helper.GetWorkflowByName("Routing_Workflow_1").WorkflowRevision.WorkflowId },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "No_AE",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Basic_Id_WF_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { Helper.GetWorkflowByName("Basic_Workflow_1").WorkflowRevision.WorkflowId },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "No_AE",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Basic_Multi_Id_WF_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { Helper.GetWorkflowByName("Basic_Workflow_1").WorkflowRevision.WorkflowId, Helper.GetWorkflowByName("Basic_Workflow_2").WorkflowRevision.WorkflowId },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "No_AE",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Mismatch_Id_AeTitle_WF_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { Helper.GetWorkflowByName("Basic_Workflow_1").WorkflowRevision.WorkflowId },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Basic_AE_3",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "AeTitle_Multi_Revision_WF_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Multi_Revision",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "WorkflowID_Multi_Revision_WF_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { Helper.GetWorkflowByName("Basic_Workflow_Multiple_Revisions_2").WorkflowRevision.WorkflowId },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "No_AE",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Missing_PayloadID_Invalid_WF_Request ",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    Workflows = new List<string>() { Helper.GetWorkflowByName("Basic_Workflow_3").WorkflowRevision.WorkflowId },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Basic_AE_3",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Missing_WorkflowID_Invalid_WF_Request ",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Basic_AE_3",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Missing_Bucket_Invalid_WF_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { Helper.GetWorkflowByName("Basic_Workflow_3").WorkflowRevision.WorkflowId },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Basic_AE_3",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Missing_CorrelationID_Invalid_WF_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { Helper.GetWorkflowByName("Basic_Workflow_3").WorkflowRevision.WorkflowId },
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Basic_AE_3",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Missing_CallingAETitle_Invalid_WF_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { Helper.GetWorkflowByName("Basic_Workflow_3").WorkflowRevision.WorkflowId },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Basic_AE_3",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Missing_CalledAETitle_Invalid_WF_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { Helper.GetWorkflowByName("Basic_Workflow_3").WorkflowRevision.WorkflowId },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Multi_WF_Created",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Multi_Created",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Multi_WF_Dispatched",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Multi_Dispatch",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Same_AeTitle",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Same_Ae",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Basic_AeTitle_Payload_Collection_Request_1",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = new Guid("23b96697-0174-465c-b9cb-368b20a4591d"),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Basic_AE",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Basic_AeTitle_Payload_Collection_Request_2",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = new Guid("64a2b260-0379-4614-9f05-ff1279cf9e83"),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Basic_AE",
                    CallingAeTitle = "MWM",
                }
            },
             new WorkflowRequestTestData
            {
                Name = "Basic_AeTitle_Payload_Collection_Request_3",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = new Guid("b91f5559-8ab2-455a-806d-961244ea22af"),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Basic_AE",
                    CallingAeTitle = "MWM",
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Basic_Non_Existant_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = new Guid("c9c1e0f1-5994-4882-b3d4-9e1009729377"),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    CalledAeTitle = "Non_Existant_AE",
                    CallingAeTitle = "MWM",
                }
            },
        };
    }
}
