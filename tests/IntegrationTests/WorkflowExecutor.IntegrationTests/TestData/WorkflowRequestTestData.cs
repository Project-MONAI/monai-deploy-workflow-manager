/*
 * Copyright 2022 MONAI Consortium
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

using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Models;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "PACS", Destination = "MONAI" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger= new Messaging.Events.DataOrigin{Destination = "No_AE" ,Source = "MWM" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger= new Messaging.Events.DataOrigin{Destination = "No_AE" ,Source = "MWM" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger= new Messaging.Events.DataOrigin{Destination = "Basic_AE_3" ,Source = "Basic_AE" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "MONAI_2" ,Source = "MWM" },
                }
            },
            new WorkflowRequestTestData
            {
                Name = "AE_Title_From_Old_Version",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "Multi_Rev_Old", Source = "MWM" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "No_AE", Source = "MWM" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "Basic_AE_3" ,Source = "MWM" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "Basic_AE_3", Source = "MWM" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "Basic_AE_3", Source = "MWM" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "Basic_AE_3" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Source = "MWM" }
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "Multi_Created", Source = "MWM" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "MWM" ,Source = "Multi_Dispatch" },
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Complete_WF_Dispatched",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                        DataTrigger = new Messaging.Events.DataOrigin { Destination = "MWM", Source = "Complete_Dispatch" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "Same_Ae", Source = "MWM" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "MONAI", Source = "PACS" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "MONAI", Source = "PACS" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "MONAI", Source = "PACS" },
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
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "Non_Existant_AE", Source = "MWM" },
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Routing_Workflow_Request",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "Router_1", Source = "MWM" },
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Artifact_AeTitle_Request_1",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = new Guid("3d22bf41-eacd-4e43-9161-d00735b31a2e"),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "Artifact_AE", Source = "MWM" },
                }
            },
            new WorkflowRequestTestData
            {
                Name = "No_Matching_AE_Title",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "Non_Existent_Called_AE", Source = "Non_Existent_Calling_AE" },
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Called_AET_AIDE_Calling_AET_TEST",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "AIDE", Source = "TEST" },
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Called_AET_AIDE_Calling_AET_PACS1",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "AIDE", Source = "PACS1" },
                }
            },
            new WorkflowRequestTestData
            {
                Name = "Called_AET_AIDE_Calling_AET_PACS2",
                WorkflowRequestMessage = new WorkflowRequestMessage
                {
                    Bucket = "bucket1",
                    PayloadId = Guid.NewGuid(),
                    Workflows = new List<string>() { },
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    DataTrigger = new Messaging.Events.DataOrigin { Destination = "AIDE", Source = "PACS2" },
                }
            },
        };
    }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
