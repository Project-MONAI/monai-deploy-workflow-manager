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
                        { "metadata_1", "test_1" },
                        { "metadata_2", "test_2" },
                        { "metadata_3", "test_3" },
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
