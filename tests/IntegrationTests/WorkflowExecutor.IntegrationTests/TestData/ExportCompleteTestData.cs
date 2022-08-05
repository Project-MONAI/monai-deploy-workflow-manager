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

namespace Monai.Deploy.WorkflowManager.WorkflowExecutor.IntegrationTests.TestData
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
