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
    public class TaskDispatchTestData
    {
        public string? Name { get; set; }

        public TaskDispatchEvent? TaskDispatchEvent { get; set; }
    }

    public static class TaskDispatchesTestData
    {
        public static List<TaskDispatchTestData> TestData = new List<TaskDispatchTestData>()
        {
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Basic",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input1",
                            Endpoint = "//test_1",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test1",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm_1"
                        },
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "//dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "workflow_name", "Workflow_1" },
                        { "reviewed_task_details", "Reviewed_Task" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review_Full_Patient_Details",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "/dcm"
                        }
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test1",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "/dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "workflow_name", "Workflow_1" },
                        { "reviewed_task_details", "Reviewed_Task" },
                        { "patient_id", "100001" },
                        { "patient_name", "John Doe" },
                        { "patient_sex", "Male" },
                        { "patient_dob", "01/01/1990" },
                        { "queue_name", "aide.clinical_review.request" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review_Partial_Patient_Details",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test1",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "//dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "workflow_name", "Workflow_1" },
                        { "reviewed_task_details", "Reviewed_Task" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review_No_Patient_Details",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test1",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "//dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "workflow_name", "Workflow_1" },
                        { "reviewed_task_details", "Reviewed_Task" },
                        { "queue_name", "aide.clinical_review.request" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review_Multi_File",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input1",
                            Endpoint = "//test_1",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test1",
                                AccessToken = "test1",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm_1"
                        },
                        new Messaging.Common.Storage
                        {
                            Name = "input2",
                            Endpoint = "//test_2",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test2",
                                AccessToken = "test2",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm_2"
                        }
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "//dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "workflow_name", "Workflow_1" },
                        { "reviewed_task_details", "Reviewed_Task" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Invalid_Input_Missing",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "//dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "workflow_name", "Workflow_1" },
                        { "reviewed_task_details", "Reviewed_Task" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Invalid_ExecutionId_Missing",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "//dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "workflow_name", "Workflow_1" },
                        { "reviewed_task_details", "Reviewed_Task" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Invalid_PayloadId_Missing",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "//dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "workflow_name", "Workflow_1" },
                        { "reviewed_task_details", "Reviewed_Task" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Invalid_TaskId_Missing",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "//dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "workflow_name", "Workflow_1" },
                        { "reviewed_task_details", "Reviewed_Task" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Invalid_TaskPluginType_NotSupported",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "not_supported",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "//dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "workflow_name", "Workflow_1" },
                        { "reviewed_task_details", "Reviewed_Task" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review_WorkflowName_Missing",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test1",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "//dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "reviewed_task_details", "Reviewed_Task" },
                        { "queue_name", "aide.clinical_review.request" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review_QueueName_Missing",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test1",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "//dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "workflow_name", "Workflow_1" },
                        { "reviewed_task_details", "Reviewed_Task" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Accepted",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "aide_clinical_review",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "input",
                            Endpoint = "//test",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "//dcm"
                        }
                    },
                    IntermediateStorage = new Messaging.Common.Storage
                    {
                        Name = "input",
                        Endpoint = "//test",
                        Credentials = new Messaging.Common.Credentials()
                        {
                            AccessKey = "test1",
                            AccessToken = "test",
                        },
                        Bucket = "bucket1",
                        RelativeRootPath = "//dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "workflow_name", "Workflow_1" },
                        { "reviewed_task_details", "Reviewed_Task" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                    }
                }
            },
        };
    }
}
