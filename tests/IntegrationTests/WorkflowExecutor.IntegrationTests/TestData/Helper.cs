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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData
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

        public static ExecutionStatTestData GetExecutionStatsByName(string name)
        {
            var executionStatsTestData = ExecutionStatsTestData.TestData.Find(c => c.Name == name);

            if (executionStatsTestData != null)
            {
                return executionStatsTestData;
            }

            throw new Exception($"Execution stat {name} does not exist. Please check and try again!");
        }
    }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
