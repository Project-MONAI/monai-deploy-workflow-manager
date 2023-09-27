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

using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData
{
    public class PayloadTestData
    {
        public string? Name { get; set; }

        public Payload? Payload { get; set; }
    }

    public static class PayloadsTestData
    {
        public static List<PayloadTestData> TestData = new List<PayloadTestData>()
        {
            new PayloadTestData()
            {
                Name = "Payload_Full_Patient",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = "c5c3636b-81dd-44a9-8c4b-71adec7d47b2",
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Steve Jobs",
                        PatientSex = "male"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_Partial_Patient",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = "86c0f117-4021-412e-b163-0dc621df672a",
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 3,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = null,
                        PatientId = "dae4a6d1-573d-4a3f-978f-ed056f628de6",
                        PatientName = "Jane Doe",
                        PatientSex = "female"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_Null_Patient",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_2",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = "30a8e0c6-e6c4-458f-aa4d-b224b493d3c0",
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 3,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = null,
                        PatientId = null,
                        PatientName = null,
                        PatientSex = null
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_basic_1",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = "09da8f2c-c0ae-4de6-9b66-28a2bed6c2f6",
                        PatientName = "Jan Jones",
                        PatientSex = "f"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_basic_2",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = "09da8f2c-c0ae-4de6-9b66-28a2bed6c2f6",
                        PatientName = "Tim Apple",
                        PatientSex = "m"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_basic_3",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = "09da8f2c-c0ae-4de6-9b66-28a2bed6c2f6",
                        PatientName = "Foo Bar",
                        PatientSex = "N/A"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_basic_4",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Jamie",
                        PatientSex = "male"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_basic_5",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Jack Johnson",
                        PatientSex = "non-binary"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_basic_6",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Linda Croft",
                        PatientSex = "f"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_basic_7",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Donald Jefferson",
                        PatientSex = "male"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_basic_8",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = Guid.NewGuid().ToString(),
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Mike Mcgee",
                        PatientSex = "male"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_Null",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = "c5c3636b-81dd-44a9-8c4b-71adec7d47b2",
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = null,
                        PatientId = null,
                        PatientName = null,
                        PatientSex = null
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_PayloadDeleted_InProgress",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = "c5c3635b-81dd-44a9-8c3b-71adec7d47c6",
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PayloadDeleted = PayloadDeleted.InProgress,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Mike Mcgee",
                        PatientSex = "male"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_PayloadDeleted_No",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = "d5c3633b-41de-44a9-8c3a-71adec3d47c1",
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PayloadDeleted = PayloadDeleted.No,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Mike Mcgee",
                        PatientSex = "male"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_PayloadStatus_1",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = "a5c3633b-31dd-44c9-8a1a-71adec3d47c1",
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PayloadDeleted = PayloadDeleted.No,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Mike Mcgee",
                        PatientSex = "male"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_PayloadStatus_2",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = "a5c3633b-31dd-44c9-8a1a-71adec3d47c2",
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PayloadDeleted = PayloadDeleted.No,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Mike Mcgee",
                        PatientSex = "male"
                    }
                }
            },
            new PayloadTestData()
            {
                Name = "Payload_PayloadStatus_3",
                Payload = new Payload()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Bucket = "bucket_1",
                    DataTrigger = new Messaging.Events.DataOrigin{ Source = "MIG", Destination = "Basic_AE" },
                    CorrelationId = Guid.NewGuid().ToString(),
                    PayloadId = "a5c3633b-31dd-44c9-8a1a-71adec3d47c3",
                    Workflows = new List<string> { Guid.NewGuid().ToString() },
                    FileCount = 50,
                    PayloadDeleted = PayloadDeleted.No,
                    PatientDetails = new PatientDetails()
                    {
                        PatientDob = new DateTime(1996, 02, 05, 0, 0, 0, kind: DateTimeKind.Utc),
                        PatientId = Guid.NewGuid().ToString(),
                        PatientName = "Mike Mcgee",
                        PatientSex = "male"
                    }
                }
            }
        };
    }
}
