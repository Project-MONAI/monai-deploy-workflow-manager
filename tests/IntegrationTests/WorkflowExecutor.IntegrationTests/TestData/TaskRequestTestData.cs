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

using Monai.Deploy.WorkflowManager.Common.Models;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData
{
    public class TaskRequestTestData
    {
        public string? Name { get; set; }

        public TasksRequest? TaskRequest { get; set; }
    }

    public static class TaskRequestsTestData
    {
        public static List<TaskRequestTestData> TestData = new List<TaskRequestTestData>()
        {
            new TaskRequestTestData
            {
                Name = "Valid_Task_Details_1",
                TaskRequest = new TasksRequest()
                {
                    WorkflowInstanceId = "44a63094-9e36-4ba4-9fea-8e9b76aa875b",
                    ExecutionId = "8ff3ea90-0113-4071-9b92-5068956daeff",
                    TaskId = "7b8ea05b-8abe-4848-928d-d55f5eef1bc3",
                }
            },
            new TaskRequestTestData
            {
                Name = "Valid_Task_Details_2",
                TaskRequest = new TasksRequest()
                {
                    WorkflowInstanceId = "44a63094-9e36-4ba4-9fea-8e9b76aa875b",
                    ExecutionId = "a1cd5b89-85e8-4d32-b9aa-bdbc0f4bbba5",
                    TaskId = "953c0236-5292-4186-80ee-ef7d4073220b",
                }
            },
            new TaskRequestTestData
            {
                Name = "Invalid_WorkflowID_Task_Details_1",
                TaskRequest = new TasksRequest()
                {
                    WorkflowInstanceId = "NotAGUID",
                    ExecutionId = "a1cd5b89-85e8-4d32-b9aa-bdbc0f4bbba5",
                    TaskId = "953c0236-5292-4186-80ee-ef7d4073220b",
                }
            },
            new TaskRequestTestData
            {
                Name = "Invalid_ExecutionID_Task_Details_2",
                TaskRequest = new TasksRequest()
                {
                    WorkflowInstanceId = "44a63094-9e36-4ba4-9fea-8e9b76aa875b",
                    ExecutionId = "NotAGUID",
                    TaskId = "953c0236-5292-4186-80ee-ef7d4073220b",
                }
            },
            new TaskRequestTestData
            {
                Name = "Invalid_TaskID_Task_Details_3",
                TaskRequest = new TasksRequest()
                {
                    WorkflowInstanceId = "44a63094-9e36-4ba4-9fea-8e9b76aa875b",
                    ExecutionId = "a1cd5b89-85e8-4d32-b9aa-bdbc0f4bbba5",
                    TaskId = "NotAGUID",
                }
            },
            new TaskRequestTestData
            {
                Name = "Non_Existent_WorkflowID_Task_Details_1",
                TaskRequest = new TasksRequest()
                {
                    WorkflowInstanceId = "0c533e6d-8c86-422b-8564-00f68aff6e20",
                    ExecutionId = "a1cd5b89-85e8-4d32-b9aa-bdbc0f4bbba5",
                    TaskId = "953c0236-5292-4186-80ee-ef7d4073220b",
                }
            },
            new TaskRequestTestData
            {
                Name = "Non_Existent_ExecutionID_Task_Details_2",
                TaskRequest = new TasksRequest()
                {
                    WorkflowInstanceId = "44a63094-9e36-4ba4-9fea-8e9b76aa875b",
                    ExecutionId = "60544cb8-d475-4271-8ead-8ef698c0da99",
                    TaskId = "953c0236-5292-4186-80ee-ef7d4073220b",
                }
            },
            new TaskRequestTestData
            {
                Name = "Non_Existent_TaskID_Task_Details_3",
                TaskRequest = new TasksRequest()
                {
                    WorkflowInstanceId = "44a63094-9e36-4ba4-9fea-8e9b76aa875b",
                    ExecutionId = "a1cd5b89-85e8-4d32-b9aa-bdbc0f4bbba5",
                    TaskId = "c16640cd-af88-495c-8c7e-fcbfbd740179",
                }
            }
        };
    }
}
