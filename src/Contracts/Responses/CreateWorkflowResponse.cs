// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Newtonsoft.Json;


namespace Monai.Deploy.WorkflowManager.Contracts.Responses
{
    public class CreateWorkflowResponse
    {
        public CreateWorkflowResponse(string workflowId)
        {
            WorkflowId = workflowId;
        }

        [JsonProperty("workflow_id")]
        public string WorkflowId { get; set; }
    }
}
