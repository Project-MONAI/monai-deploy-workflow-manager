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

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests
{
    public class TaskCallbackTestData
    {
        public string? Name { get; set; }

        public TaskCallbackEvent? TaskCallbackEvent { get; set; }
    }

    public static class TaskCallbacksTestData
    {
        public static List<TaskCallbackTestData> TestData = new List<TaskCallbackTestData>()
        {
            new TaskCallbackTestData()
            {
                Name = "Task_Callback_Basic",
                TaskCallbackEvent = new TaskCallbackEvent()
                {
                    CorrelationId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic").TaskDispatchEvent.CorrelationId,
                    ExecutionId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic").TaskDispatchEvent.ExecutionId,
                    Identity = "Identity_1",
                    TaskId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic").TaskDispatchEvent.TaskId,
                    WorkflowInstanceId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic").TaskDispatchEvent.WorkflowInstanceId,
                    Metadata = new Dictionary<string, object>()
                    {
                        { "acceptance", true },
                        { "user_id", "example_user_id" }
                    },
                    Outputs = new List<Messaging.Common.Storage> {
                        new Messaging.Common.Storage()
                        {
                            Name = "output",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    }
                }
            },
            new TaskCallbackTestData()
            {
                Name = "Task_Callback_Succeeded",
                TaskCallbackEvent = new TaskCallbackEvent()
                {
                    CorrelationId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic_Clinical_Review").TaskDispatchEvent.CorrelationId,
                    ExecutionId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic_Clinical_Review").TaskDispatchEvent.ExecutionId,
                    Identity = "Identity_1",
                    TaskId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic_Clinical_Review").TaskDispatchEvent.TaskId,
                    WorkflowInstanceId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic_Clinical_Review").TaskDispatchEvent.WorkflowInstanceId,
                    Metadata = new Dictionary<string, object>()
                    {
                        { "acceptance", true },
                        { "user_id", "example_user_id" }
                    },
                    Outputs = new List<Messaging.Common.Storage> {
                        new Messaging.Common.Storage()
                        {
                            Name = "output",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    }
                }
            },
            new TaskCallbackTestData()
            {
                Name = "Task_Callback_Partial_Fail",
                TaskCallbackEvent = new TaskCallbackEvent()
                {
                    CorrelationId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic_Clinical_Review").TaskDispatchEvent.CorrelationId,
                    ExecutionId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic_Clinical_Review").TaskDispatchEvent.ExecutionId,
                    Identity = "Identity_1",
                    TaskId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic_Clinical_Review").TaskDispatchEvent.TaskId,
                    WorkflowInstanceId = Helper.GetTaskDispatchByName("Task_Dispatch_Basic_Clinical_Review").TaskDispatchEvent.WorkflowInstanceId,
                    Metadata = new Dictionary<string, object>()
                    {
                        { "acceptance", false },
                        { "reason", "not correct" },
                        { "user_id", "example_user_id" }
                    },
                    Outputs = new List<Messaging.Common.Storage> {
                        new Messaging.Common.Storage()
                        {
                            Name = "output",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    }
                }
            }
        };
    }
}
