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
                Name = "Task_Dispatch_Basic_Clinical_Review",
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Basic_Argo",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "argo",
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
                        { "Namespace", "Namespace_1" },
                        { "ArgoApiToken", "123456789" },
                        { "AllowInsecureUrl", "false" },
                        { "BaseUrl", "https://test.com" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Reviewer_Role_Single_Role",
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "reviewer_roles", "head_scan_clinician" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Reviewer_Role_Mutiple_Roles",
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "reviewer_roles", "head_scan_clinician, brain_scan_clinician" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Reviewer_Role_Default_Role",
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "reviewer_roles", "" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review_Application_Name",
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "reviewer_roles", "" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review_Application_Version",
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "reviewer_roles", "" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review_QA_Mode",
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "reviewer_roles", "" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review_Reviewed_Execution_Id",
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "reviewer_roles", "" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Invalid",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "argo",
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "patient_name", "John Doe" },
                        { "patient_sex", "Male" },
                        { "patient_dob", "01/01/1990" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" }
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review_Notifications_True",
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "reviewer_roles", "" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
                        { "notifications", "true" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Clinical_Review_Notifications_False",
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
                        { "reviewed_task_id", "some_task" },
                        { "reviewed_execution_id", "some_execution" },
                        { "patient_id", "100001" },
                        { "queue_name", "aide.clinical_review.request" },
                        { "reviewer_roles", "" },
                        { "application_name", "app name" },
                        { "application_version", "v1" },
                        { "mode", "QA" },
                        { "notifications", "false" },
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Email_All",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "email",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "series1/07051db3-3c1d-4bf2-8764-ba45dc918e74.dcm",
                            Endpoint = "//test_1",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "dcm_1"
                        },
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
                        { "recipient_emails", "email@test.com" },
                        { "recipient_roles", "clinician" },
                        { "metadata_values", "Modality" },
                        { "workflow_name", "example_workflow_name" }
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Email_Emails",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "email",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "series1/07051db3-3c1d-4bf2-8764-ba45dc918e74.dcm",
                            Endpoint = "//test_1",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "dcm_1"
                        },
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
                        { "recipient_emails", "email@test.com" },
                        { "metadata_values", "Modality" },
                        { "workflow_name", "example_workflow_name" }
                    }
                }
            },
            new TaskDispatchTestData
            {
                Name = "Task_Dispatch_Email_Roles",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    PayloadId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    ExecutionId = Guid.NewGuid().ToString(),
                    WorkflowInstanceId = Guid.NewGuid().ToString(),
                    TaskId = Guid.NewGuid().ToString(),
                    Status = TaskExecutionStatus.Dispatched,
                    TaskPluginType = "email",
                    Inputs = new List<Messaging.Common.Storage>()
                    {
                        new Messaging.Common.Storage
                        {
                            Name = "series1/07051db3-3c1d-4bf2-8764-ba45dc918e74.dcm",
                            Endpoint = "//test_1",
                            Credentials = new Messaging.Common.Credentials()
                            {
                                AccessKey = "test",
                                AccessToken = "test",
                            },
                            Bucket = "bucket1",
                            RelativeRootPath = "dcm_1"
                        },
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
                        RelativeRootPath = "dcm"
                    },
                    TaskPluginArguments = new Dictionary<string, string>()
                    {
                        { "recipient_roles", "clinician" },
                        { "metadata_values", "Modality" },
                        { "workflow_name", "example_workflow_name" }
                    }
                }
            }
        };
    }
}
