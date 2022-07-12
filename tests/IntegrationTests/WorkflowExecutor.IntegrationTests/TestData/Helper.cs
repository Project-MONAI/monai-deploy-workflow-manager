// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.WorkflowExecutor.IntegrationTests.TestData;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.TestData
{
    public static class Helper
    {
        public static WorkflowRevisionTestData GetWorkflowByName(string name)
        {
            var workflowRevisionTestData = WorkflowRevisionsTestData.TestData.FirstOrDefault(c => c.Name.Contains(name));

            if (workflowRevisionTestData != null)
            {
                return workflowRevisionTestData;
            }

            throw new Exception($"workflow {name} does not exist. Please check and try again!");
        }

        public static WorkflowRequestTestData GetWorkflowRequestByName(string name)
        {
            var workflowRequestTestData = WorkflowRequestsTestData.TestData.FirstOrDefault(c => c.Name.Contains(name));

            if (workflowRequestTestData != null)
            {
                return workflowRequestTestData;
            }

            throw new Exception($"workflow {name} does not exist. Please check and try again!");
        }

        public static WorkflowInstanceTestData GetWorkflowInstanceByName(string name)
        {
            var workflowInstanceTestData = WorkflowInstancesTestData.TestData.FirstOrDefault(c => c.Name.Contains(name));

            if (workflowInstanceTestData != null)
            {
                return workflowInstanceTestData;
            }

            throw new Exception($"Workflow Instance {name} does not exist. Please check and try again!");
        }

        public static PayloadTestData GetPayloadByName(string name)
        {
            var payloadTestData = PayloadsTestData.TestData.FirstOrDefault(c => c.Name.Contains(name));

            if (payloadTestData != null)
            {
                return payloadTestData;
            }

            throw new Exception($"Payload {name} does not exist. Please check and try again!");
        }
    }
}
