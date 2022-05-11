// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.IntegrationTests.TestData
{
    public static class Helper
    {
        public static object WorkflowsTestData { get; private set; }

        public static string GetWorkflowIdByName(string name)
        {
            var workflowRevision = WorkflowRevisionsTestData.TestData.FirstOrDefault(c => c.Name.Contains(name));
            if (workflowRevision != null)
            {
                if (workflowRevision.WorkflowRevision != null)
                {
                    return workflowRevision.WorkflowRevision.WorkflowId;
                }
            }
            throw new Exception($"workflow {name} does not exist. Please check and try again!");
        }

        public static string GetPayloadIdByName(string name)
        {
            var workflowRequest = WorkflowRequestsTestData.TestData.FirstOrDefault(c => c.Name.Contains(name));
            if (workflowRequest != null)
            {
                if (workflowRequest.WorkflowRequestMessage != null)
                {
                    return workflowRequest.WorkflowRequestMessage.PayloadId.ToString();
                }
            }
            throw new Exception($"workflow {name} does not exist. Please check and try again!");
        }
    }
}
