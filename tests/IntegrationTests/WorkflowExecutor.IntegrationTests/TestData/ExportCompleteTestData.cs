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

using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData
{
    public class ExportCompleteTestData
    {
        public string? Name { get; set; }

        public ExportCompleteEvent? ExportCompleteEvent { get; set; }
    }

    public static class ExportCompletesTestData
    {
        public static List<ExportCompleteTestData> TestData = new List<ExportCompleteTestData>()
        {
            new ExportCompleteTestData()
            {
                Name = "Export_Complete_Message_for_export_multi_dest_2_Succeeded",
                ExportCompleteEvent = new ExportCompleteEvent()
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_multi_dest_2").WorkflowInstance.Id,
                    ExportTaskId = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_2").WorkflowRevision.Workflow.Tasks[1].Id,
                    Status = ExportStatus.Success,
                }
            },

              new ExportCompleteTestData()
            {
                Name = "Export_Complete_Message_for_export_file_statuses",
                ExportCompleteEvent = new ExportCompleteEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("ExportComplete_WFI_Dispatched").WorkflowInstance.Id,
                    ExportTaskId = Helper.GetWorkflowByName("Complete_Request_Workflow_Dispatched").WorkflowRevision.Workflow.Tasks[0].Id,
                    Status = ExportStatus.Success,
                    FileStatuses = new Dictionary<string, FileExportStatus>()
                    {
                       {"bc9ca8ae-2bf7-4546-a537-8f813f357d9d/dcm/1.2.840.113619.2.243.6074146108103184.41976.4343.2084088/1.3.46.670589.11.0.0.11.4.2.0.12098.5.4500.2015011413252331688/1.3.46.670589.11.0.0.11.4.2.0.12098.5.4500.2015011413255023689.dcm", FileExportStatus.Success},
                       {"bc9ca8ae-2bf7-4546-a537-8f813f357d9d/dcm/1.2.840.113619.2.243.6074146108103184.41976.4343.2084088/1.3.46.670589.11.0.0.11.4.2.0.12098.5.4500.2015011413252331688/1.3.46.670589.11.0.0.11.4.2.0.12098.5.4500.2015011413255023690.dcm", FileExportStatus.Success},
                       {"bc9ca8ae-2bf7-4546-a537-8f813f357d9d/dcm/1.2.840.113619.2.243.6074146108103184.41976.4343.2084088/1.3.46.670589.11.0.0.11.4.2.0.12098.5.4500.2015011413252331688/1.3.46.670589.11.0.0.11.4.2.0.12098.5.4500.2015011413255025691.dcm", FileExportStatus.Success}
                    }
                }
            },
             new ExportCompleteTestData()
            {
                Name = "Export_Complete_Message_for_export_multi_dest_2_Succeeded",
                ExportCompleteEvent = new ExportCompleteEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_multi_dest_2").WorkflowInstance.Id,
                    ExportTaskId = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_2").WorkflowRevision.Workflow.Tasks[1].Id,
                    Status = ExportStatus.Success,
                }
            },
            new ExportCompleteTestData()
            {
                Name = "Export_Complete_Message_for_export_multi_dest_2_Failed",
                ExportCompleteEvent = new ExportCompleteEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_multi_dest_2").WorkflowInstance.Id,
                    ExportTaskId = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_2").WorkflowRevision.Workflow.Tasks[1].Id,
                    Status = ExportStatus.Failure,
                }
            },
            new ExportCompleteTestData()
            {
                Name = "Export_Complete_Message_for_export_multi_dest_2_PartialFailed",
                ExportCompleteEvent = new ExportCompleteEvent()
                {
                    WorkflowInstanceId = Helper.GetWorkflowInstanceByName("Workflow_Instance_for_export_multi_dest_2").WorkflowInstance.Id,
                    ExportTaskId = Helper.GetWorkflowByName("Workflow_Revision_for_export_multi_dest_2").WorkflowRevision.Workflow.Tasks[1].Id,
                    Status = ExportStatus.PartialFailure,
                }
            },
         };
    }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
