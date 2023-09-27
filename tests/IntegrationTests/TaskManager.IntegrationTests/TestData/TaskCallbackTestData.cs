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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
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
            },
            new TaskCallbackTestData()
            {
                Name = "Task_Callback_Execution_Stats",
                TaskCallbackEvent = new TaskCallbackEvent()
                {
                    CorrelationId = Helper.GetTaskDispatchByName("Task_Dispatch_Execution_Stats").TaskDispatchEvent.CorrelationId,
                    ExecutionId = Helper.GetTaskDispatchByName("Task_Dispatch_Execution_Stats").TaskDispatchEvent.ExecutionId,
                    Identity = "Identity_1",
                    TaskId = Helper.GetTaskDispatchByName("Task_Dispatch_Execution_Stats").TaskDispatchEvent.TaskId,
                    WorkflowInstanceId = Helper.GetTaskDispatchByName("Task_Dispatch_Execution_Stats").TaskDispatchEvent.WorkflowInstanceId,
                    Metadata = new Dictionary<string, object>()
                    {
                        {"IdentityKey", "md-argo-workflow-2-b4w9w" },
                        {"workflowInstanceId", "b6288697-6fdd-4cd8-8e54-ac079cd487be" },
                        {"duration", "30000" },
                        {"startedAt", "01/01/2023 00:00:30 +00:00" },
                        {"finishedAt", "01/01/2023 00:01:00 +00:00" },
                        {"resourceDuration.cpu", "12" },
                        {"resourceDuration.memory", "12" },
                        {"nodes.md-argo-workflow-2-b4w9w", "{\"children\":[\"md-argo-workflow-2-b4w9w-1789511733\"],\"displayName\":\"md-argo-workflow-2-b4w9w\",\"finishedAt\":\"2023-01-01T10:10:37+00:00\",\"id\":\"md-argo-workflow-2-b4w9w\",\"name\":\"md-argo-workflow-2-b4w9w\",\"outboundNodes\":[\"md-argo-workflow-2-b4w9w-2715803737\"],\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":3,\"memory\":3},\"startedAt\":\"2023-01-01T10:10:27+00:00\",\"templateName\":\"md-workflow-entrypoint\",\"templateScope\":\"local/md-argo-workflow-2-b4w9w\",\"type\":\"Steps\"}" },
                        {"nodes.md-argo-workflow-2-b4w9w-1784860063", "{\"boundaryID\":\"md-argo-workflow-2-b4w9w\",\"children\":[\"md-argo-workflow-2-b4w9w-1818284555\"],\"displayName\":\"md-workflow-entrypoint\",\"finishedAt\":\"2023-01-01T10:10:37+00:00\",\"id\":\"md-argo-workflow-2-b4w9w-1784860063\",\"name\":\"md-argo-workflow-2-b4w9w[0].md-workflow-entrypoint\",\"outboundNodes\":[\"md-argo-workflow-2-b4w9w-2715803737\"],\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":3,\"memory\":3},\"startedAt\":\"2023-01-01T10:10:27+00:00\",\"templateName\":\"demo-pipeline\",\"templateScope\":\"local/md-argo-workflow-2-b4w9w\",\"type\":\"Steps\"}" },
                        {"nodes.md-argo-workflow-2-b4w9w-1789511733", "{\"boundaryID\":\"md-argo-workflow-2-b4w9w\",\"children\":[\"md-argo-workflow-2-b4w9w-1784860063\"],\"displayName\":\"[0]\",\"finishedAt\":\"2023-01-01T10:10:37+00:00\",\"id\":\"md-argo-workflow-2-b4w9w-1789511733\",\"name\":\"md-argo-workflow-2-b4w9w[0]\",\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":3,\"memory\":3},\"startedAt\":\"2023-01-01T10:10:27+00:00\",\"templateScope\":\"local/md-argo-workflow-2-b4w9w\",\"type\":\"StepGroup\"}" },
                        {"nodes.md-argo-workflow-2-b4w9w-1818284555", "{\"boundaryID\":\"md-argo-workflow-2-b4w9w-1784860063\",\"children\":[\"md-argo-workflow-2-b4w9w-2715803737\"],\"displayName\":\"[0]\",\"finishedAt\":\"2023-01-01T10:10:37+00:00\",\"id\":\"md-argo-workflow-2-b4w9w-1818284555\",\"name\":\"md-argo-workflow-2-b4w9w[0].md-workflow-entrypoint[0]\",\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":3,\"memory\":3},\"startedAt\":\"2023-01-01T10:10:27+00:00\",\"templateScope\":\"local/md-argo-workflow-2-b4w9w\",\"type\":\"StepGroup\"}" },
                        {"nodes.md-argo-workflow-2-b4w9w-2675961615", "{\"boundaryID\":\"md-argo-workflow-2-b4w9w-3165936904\",\"children\":[\"md-argo-workflow-2-b4w9w-886919407\"],\"displayName\":\"[1]\",\"finishedAt\":\"2023-01-01T10:10:57+00:00\",\"id\":\"md-argo-workflow-2-b4w9w-2675961615\",\"name\":\"md-argo-workflow-2-b4w9w.onExit[1]\",\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":3,\"memory\":3},\"startedAt\":\"2023-01-01T10:10:47+00:00\",\"templateScope\":\"local/md-argo-workflow-2-b4w9w\",\"type\":\"StepGroup\"}" },
                        {"nodes.md-argo-workflow-2-b4w9w-2715803737", "{\"boundaryID\":\"md-argo-workflow-2-b4w9w-1784860063\",\"displayName\":\"calculate-mean\",\"finishedAt\":\"2023-01-01T00:00:55+00:00\",\"hostNodeName\":\"dgx\",\"id\":\"md-argo-workflow-2-b4w9w-2715803737\",\"inputs\":{\"artifacts\":[{\"name\":\"input-dicom\",\"path\":\"/tmp/dicom_input\",\"s3\":{\"accessKeySecret\":{\"key\":\"accessKey\",\"name\":\"input-dicom-638066958270913358\"},\"bucket\":\"monaideploy\",\"endpoint\":\"minio-hl.minio.svc.cluster.local:9000\",\"insecure\":true,\"key\":\"f9292573-e45f-4640-9c96-c077c1be40c0/dcm\",\"secretKeySecret\":{\"key\":\"secretKey\",\"name\":\"input-dicom-638066958270913358\"}}}]},\"name\":\"md-argo-workflow-2-b4w9w[0].md-workflow-entrypoint[0].calculate-mean\",\"outputs\":{\"artifacts\":[{\"archive\":{\"none\":{}},\"name\":\"report-pdf\",\"path\":\"/tmp/pixel_mean_output\",\"s3\":{\"accessKeySecret\":{\"key\":\"accessKey\",\"name\":\"liver-seg-638066958271065813\"},\"bucket\":\"monaideploy\",\"endpoint\":\"minio-hl.minio.svc.cluster.local:9000\",\"insecure\":true,\"key\":\"f9292573-e45f-4640-9c96-c077c1be40c0/workflows/b6288697-6fdd-4cd8-8e54-ac079cd487be/9bda6c90-5f31-4cb8-a214-030de163dc57/report-pdf\",\"secretKeySecret\":{\"key\":\"secretKey\",\"name\":\"liver-seg-638066958271065813\"}}}],\"exitCode\":\"0\"},\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":3,\"memory\":3},\"startedAt\":\"2023-01-01T00:00:35+00:00\",\"templateName\":\"mean-calc\",\"templateScope\":\"local/md-argo-workflow-2-b4w9w\",\"type\":\"Pod\"}" },
                        {"nodes.md-argo-workflow-2-b4w9w-2743219186", "{\"boundaryID\":\"md-argo-workflow-2-b4w9w-3165936904\",\"children\":[\"md-argo-workflow-2-b4w9w-3678631457\"],\"displayName\":\"[0]\",\"finishedAt\":\"2023-01-01T10:10:47+00:00\",\"id\":\"md-argo-workflow-2-b4w9w-2743219186\",\"name\":\"md-argo-workflow-2-b4w9w.onExit[0]\",\"phase\":\"Succeeded\",\"progress\":\"2/2\",\"resourcesDuration\":{\"cpu\":9,\"memory\":9},\"startedAt\":\"2023-01-01T10:10:37+00:00\",\"templateScope\":\"local/md-argo-workflow-2-b4w9w\",\"type\":\"StepGroup\"}" },
                        {"nodes.md-argo-workflow-2-b4w9w-3165936904", "{\"children\":[\"md-argo-workflow-2-b4w9w-2743219186\"],\"displayName\":\"md-argo-workflow-2-b4w9w.onExit\",\"finishedAt\":\"2023-01-01T10:10:57+00:00\",\"id\":\"md-argo-workflow-2-b4w9w-3165936904\",\"name\":\"md-argo-workflow-2-b4w9w.onExit\",\"outboundNodes\":[\"md-argo-workflow-2-b4w9w-886919407\"],\"phase\":\"Succeeded\",\"progress\":\"2/2\",\"resourcesDuration\":{\"cpu\":9,\"memory\":9},\"startedAt\":\"2023-01-01T10:10:37+00:00\",\"templateName\":\"exit-message-template\",\"templateScope\":\"local/md-argo-workflow-2-b4w9w\",\"type\":\"Steps\"}" },
                        {"nodes.md-argo-workflow-2-b4w9w-3678631457", "{\"boundaryID\":\"md-argo-workflow-2-b4w9w-3165936904\",\"children\":[\"md-argo-workflow-2-b4w9w-2675961615\"],\"displayName\":\"generate-message\",\"finishedAt\":\"2023-01-01T10:10:43+00:00\",\"hostNodeName\":\"dgx\",\"id\":\"md-argo-workflow-2-b4w9w-3678631457\",\"inputs\":{\"parameters\":[{\"name\":\"event\",\"value\":\"{\\\"workflow_instance_id\\\":\\\"b6288697-6fdd-4cd8-8e54-ac079cd487be\\\",\\\"task_id\\\":\\\"liver-seg\\\",\\\"execution_id\\\":\\\"9bda6c90-5f31-4cb8-a214-030de163dc57\\\",\\\"correlation_id\\\":\\\"1309a017-5e3e-41c9-8d09-a5ae1da44a50\\\",\\\"identity\\\":\\\"md-argo-workflow-2-b4w9w\\\",\\\"metadata\\\":{},\\\"outputs\\\":[]}\"},{\"name\":\"message\",\"value\":\"\\\"{\\\\\\\"ContentType\\\\\\\":\\\\\\\"application/json\\\\\\\",\\\\\\\"CorrelationID\\\\\\\":\\\\\\\"1309a017-5e3e-41c9-8d09-a5ae1da44a50\\\\\\\",\\\\\\\"MessageID\\\\\\\":\\\\\\\"57342e2d-bd11-4e38-b7f6-d542d1489582\\\\\\\",\\\\\\\"Type\\\\\\\":\\\\\\\"TaskCallbackEvent\\\\\\\",\\\\\\\"AppID\\\\\\\":\\\\\\\"Argo\\\\\\\",\\\\\\\"Exchange\\\\\\\":\\\\\\\"monaideploy\\\\\\\",\\\\\\\"RoutingKey\\\\\\\":\\\\\\\"md.tasks.callback\\\\\\\",\\\\\\\"DeliveryMode\\\\\\\":2,\\\\\\\"Body\\\\\\\":\\\\\\\"eyJ3b3JrZmxvd19pbnN0YW5jZV9pZCI6ImI2Mjg4Njk3LTZmZGQtNGNkOC04ZTU0LWFjMDc5Y2Q0ODdiZSIsInRhc2tfaWQiOiJsaXZlci1zZWciLCJleGVjdXRpb25faWQiOiI5YmRhNmM5MC01ZjMxLTRjYjgtYTIxNC0wMzBkZTE2M2RjNTciLCJjb3JyZWxhdGlvbl9pZCI6IjEzMDlhMDE3LTVlM2UtNDFjOS04ZDA5LWE1YWUxZGE0NGE1MCIsImlkZW50aXR5IjoibWQtYXJnby13b3JrZmxvdy0yLWI0dzl3IiwibWV0YWRhdGEiOnt9LCJvdXRwdXRzIjpbXX0=\\\\\\\"}\\\"\"}]},\"name\":\"md-argo-workflow-2-b4w9w.onExit[0].generate-message\",\"outputs\":{\"artifacts\":[{\"archive\":{\"none\":{}},\"name\":\"output\",\"path\":\"/tmp\",\"s3\":{\"accessKeySecret\":{\"key\":\"accessKey\",\"name\":\"liver-seg-638066958271065813\"},\"bucket\":\"monaideploy\",\"endpoint\":\"minio-hl.minio.svc.cluster.local:9000\",\"insecure\":true,\"key\":\"f9292573-e45f-4640-9c96-c077c1be40c0/workflows/b6288697-6fdd-4cd8-8e54-ac079cd487be/9bda6c90-5f31-4cb8-a214-030de163dc57/messaging\",\"secretKeySecret\":{\"key\":\"secretKey\",\"name\":\"liver-seg-638066958271065813\"}}}],\"exitCode\":\"0\"},\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":6,\"memory\":6},\"startedAt\":\"2023-01-01T10:10:37+00:00\",\"templateName\":\"generate-message\",\"templateScope\":\"local/md-argo-workflow-2-b4w9w\",\"type\":\"Pod\"}" },
                        {"nodes.md-argo-workflow-2-b4w9w-886919407", "{\"boundaryID\":\"md-argo-workflow-2-b4w9w-3165936904\",\"displayName\":\"send-message\",\"finishedAt\":\"2023-01-01T10:10:50+00:00\",\"hostNodeName\":\"dgx\",\"id\":\"md-argo-workflow-2-b4w9w-886919407\",\"inputs\":{\"artifacts\":[{\"archive\":{\"none\":{}},\"name\":\"message\",\"path\":\"/tmp/57342e2d-bd11-4e38-b7f6-d542d1489582.json\",\"s3\":{\"accessKeySecret\":{\"key\":\"accessKey\",\"name\":\"liver-seg-638066958271065813\"},\"bucket\":\"monaideploy\",\"endpoint\":\"minio-hl.minio.svc.cluster.local:9000\",\"insecure\":true,\"key\":\"f9292573-e45f-4640-9c96-c077c1be40c0/workflows/b6288697-6fdd-4cd8-8e54-ac079cd487be/9bda6c90-5f31-4cb8-a214-030de163dc57/messaging/57342e2d-bd11-4e38-b7f6-d542d1489582.json\",\"secretKeySecret\":{\"key\":\"secretKey\",\"name\":\"liver-seg-638066958271065813\"}}}]},\"name\":\"md-argo-workflow-2-b4w9w.onExit[1].send-message\",\"outputs\":{\"exitCode\":\"0\"},\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":3,\"memory\":3},\"startedAt\":\"2023-01-01T10:10:47+00:00\",\"templateName\":\"send-message\",\"templateScope\":\"local/md-argo-workflow-2-b4w9w\",\"type\":\"Pod\"}" }
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
        };
    }
}
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
