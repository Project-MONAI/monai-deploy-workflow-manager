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

using Monai.Deploy.Messaging.Common;
using Monai.Deploy.WorkflowManager.Common.Contracts.Constants;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Artifact = Monai.Deploy.WorkflowManager.Common.Contracts.Models.Artifact;
// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable RedundantEmptyObjectCreationArgumentList

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData
{
    public class WorkflowRevisionTestData
    {
        public string? Name { get; set; }

        public WorkflowRevision? WorkflowRevision { get; set; }
    }

    public static class WorkflowRevisionsTestData
    {
        public static List<WorkflowRevisionTestData> TestData = new List<WorkflowRevisionTestData>()
        {
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_1",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow 1",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "MONAI"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_2",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow 2",
                        Description = "Basic workflow 2",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_task",
                                Description = "Basic Workflow 2 Task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "MONAI"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_3",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow 3",
                        Description = "Basic workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_task",
                                Description = "Basic Workflow 3 Task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "MONAI"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Same_AeTitle_1",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Same AeTitle 1",
                        Description = "Same AeTitle 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_task",
                                Description = "Same AeTitle 1 Task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Same_Ae"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Same_AeTitle_2",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Same AeTitle 2",
                        Description = "Same AeTitle 2",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_task",
                                Description = "Same AeTitle 2 Task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Same_Ae"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_Multiple_Revisions_1", //not to be confused with 'Basic_Workflow_multiple_revisions_1' (lower case)
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = "85c48175-f4db-4d3c-bf3a-14f736edaccd",
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow multiple revisions 1",
                        Description = "Basic workflow multiple revisions 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_Workflow_Multiple_Revisions_1",
                                Description = "Basic_Workflow_Multiple_Revisions_1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "MONAI"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_Multiple_Revisions_2",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = "85c48175-f4db-4d3c-bf3a-14f736edaccd",
                    Revision = 2,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow multiple revisions 2",
                        Description = "Basic workflow multiple revisions 2",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_Workflow_Multiple_Revisions_2",
                                Description = "Basic_Workflow_Multiple_Revisions_2",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "MONAI_2"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Independent_Task_Workflow",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi task workflow",
                        Description = "Multi task workflow",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "task_1",
                                Description = "Multi task workflow Task 1",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "task_2",
                                Description = "Multi task workflow Task 2",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Workflow"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Task_Status_Update_Workflow",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Task Update Status",
                        Description = "Task Update Status",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Task_Update",
                                Description = "Task Update Task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Task_Update"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Request_Workflow_Created",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow 1",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "2dbd1af7-b699-4467-8e99-05a0c22422b4",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Req"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Request_Workflow_Dispatched",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow 1",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "7d7c8b83-6628-413c-9912-a89314e5e2d5",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Dispatch"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Complete_Request_Workflow_Dispatched",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow 1",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "7d7c8b83-6628-413c-9912-a89314e5e2d5",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Dispatch"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Clinical_Review_1",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Mulit Task workflow 1",
                        Description = "Multi Task workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "00d275ce-81d8-4d54-a923-d34cf1955cc4",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "510ba0cf-8632-4112-994d-36617318a74f"
                                    }
                                }
                            },
                            new TaskObject
                            {
                                Id = "510ba0cf-8632-4112-994d-36617318a74f",
                                Type = "aide_clinical_review",
                                Description = "Multiple request task 2",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination
                                    {
                                        Name = "510ba0cf-8632-4112-994d-36617318a74r"
                                    }
                                }
                            },
                            new TaskObject
                            {
                                Id = "510ba0cf-8632-4112-994d-36617318a74r",
                                Type = "task",
                                Description = "Multiple request task 2",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_1"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_1",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Mulit Task workflow 1",
                        Description = "Multi Task workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "00d275ce-81d8-4d54-a923-d34cf1955cc4",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "510ba0cf-8632-4112-994d-36617318a74f"
                                    }
                                }
                            },
                            new TaskObject
                            {
                                Id = "510ba0cf-8632-4112-994d-36617318a74f",
                                Type = "Multi_task",
                                Description = "Multiple request task 2",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_1"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_2",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 2",
                        Description = "Multi Task workflow 2",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "b971c259-c3d5-4ddf-93b2-56c83fb7b474",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "6da9b529-f7b5-40c2-a604-b7421599b364"
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "eefdd563-c589-433a-969f-2cddfe93af24"
                                    }
                                }
                            },
                            new TaskObject
                            {
                                Id = "6da9b529-f7b5-40c2-a604-b7421599b364",
                                Type = "Multi_task",
                                Description = "Multiple request task 2",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "eefdd563-c589-433a-969f-2cddfe93af24",
                                Type = "Multi_task",
                                Description = "Multiple request task 3",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_1"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_3",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Mulit Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "f4c96785-4cd4-42c9-9e43-4c50b654e397",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "6f26fb39-5991-4b6e-9885-67e32a575559"
                                    }
                                }
                            },
                            new TaskObject
                            {
                                Id = "6f26fb39-5991-4b6e-9885-67e32a575559",
                                Type = "Multi_task",
                                Description = "Multiple request task 2",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Invalid_Task_Destination",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d"
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "36dcdd1a-7e57-405e-a6c3-b0e99be1f3d2",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_1_static",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = "66678af8-e8ac-4b77-a431-9d1a289d6c3b",
                    WorkflowId = "c86a437d-d026-4bdf-b1df-c7a6372b89e3",
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Args = new Dictionary<string, string> { { "test", "test" } },
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Static_AE",
                            ExportDestinations = new string[]{"test"}
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_multiple_revisions_1", //not to be confused with 'Basic_Workflow_Multiple_Revisions_1' (upper case)
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = "570611d3-ad74-43a4-ae84-539164ee8f0c",
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow",
                        Description = "Basic workflow",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "MONAI_2"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_multiple_revisions_2",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = "570611d3-ad74-43a4-ae84-539164ee8f0c",
                    Revision = 2,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow",
                        Description = "Basic workflow",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "MONAI_2"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Workflow_Revision_for_bucket_minio",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = "66678af8-e8ac-4b77-a431-9d1a289d6c3b",
                    WorkflowId = "c86a437d-d026-4bdf-b1df-c7a6372b89e3",
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "pizza",
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Args = new Dictionary<string, string> { { "test", "test" } },
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Conditions = new string[] {
                                            "{{ context.dicom.series.any('0028','0100') }} == '-16'",
                                            "{{ context.dicom.series.any('0028','0101') }} == '-16.5'",
                                            "{{ context.dicom.series.any('0028','0101') }} < '-16.1'",
                                            "{{ context.dicom.series.any('0028','0102') }} CONTAINS '-16'",
                                            "{{ context.dicom.series.any('0028','0102') }} NOT_CONTAINS '-160'",
                                            "{{ context.dicom.series.any('0038','0101') }} == '-16.500'",
                                            "{{ context.dicom.series.any('0028','0102') }} NOT_CONTAINS ['-160', '100', '15']",
                                            "{{ context.dicom.series.any('0010','0040') }} == 'lordge'", },
                                        Name = "cake"
                                    }
                                }
                            },
                            new TaskObject
                            {
                                Id = "cake",
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Artifacts = new ArtifactMap(),
                                Args = new Dictionary<string, string> { { "test", "test" } }
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Basic_AE",
                            ExportDestinations = new string[]{"test"}
                        }
                    }
                }
            },

               new WorkflowRevisionTestData()
            {
                Name = "Workflow_Revision_For_Case_Sensitivity",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = "B0B4387A-3A84-456B-86AE-890E268C7BF1",
                    WorkflowId = "1D995113-AE61-481E-B3FD-BC27D47D82EE",
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "router",
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Args = new Dictionary<string, string> { { "test", "test" } },
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Conditions = new string[] { "{{ context.dicom.series.any('0008','103e') }} == 'Processed by Clara'" },
                                        Name = "argo"
                                    }
                                }
                            },
                            new TaskObject
                            {
                                Id = "argo",
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 2",
                                Artifacts = new ArtifactMap(),
                                Args = new Dictionary<string, string> { { "test", "test" } }
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Basic_AE",
                            ExportDestinations = new string[]{"test"}
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Destination_Single_Condition_True",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] {"'true'=='true'"}
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Destination_Single_Multi_Condition_True",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] {"'true'=='true'", "'true'=='true'" }
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Destination_Single_Multi_Condition_False",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] {"'true'=='true'", "'true'=='false'" }
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Destination_Single_Metadata_Condition_True",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Input", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] {"{{ context.input.patient_details.name }} == 'Steve Jobs'"}
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Destination_Single_Metadata_Null_Condition_True",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Input", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] { "{{ context.input.patient_details.name }} == null" }
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Destination_Single_Condition_False",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] { "'false'=='true'" }
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Destination_Single_Metadata_Condition_False",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Input", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] { "{{ context.input.patient_details.name }} == 'Incorrect Name'" }
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Multiple_Destination_Single_Condition_True",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] { "'true'=='true'" }
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                        Conditions = new string[] { "'true'=='true'" }
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "d2e5219d-9ccb-4584-b078-5216ee4b9b8b",
                                        Conditions = new string[] { "'true'=='true'" }
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "d2e5219d-9ccb-4584-b078-5216ee4b9b8b",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Multiple_Destination_Single_Condition_False",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] { "'false'=='true'" }
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Multiple_Destination_Single_Condition_True",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] { "'true'=='true'" }
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                        Conditions = new string[] { "'true'=='true'" }
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "d2e5219d-9ccb-4584-b078-5216ee4b9b8b",
                                        Conditions = new string[] {"'true'=='true'" }
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "d2e5219d-9ccb-4584-b078-5216ee4b9b8b",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Multiple_Destination_Single_Condition_False",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] { "'false'=='true'" }
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                        Conditions = new string[] { "'false'=='true'" }
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "d2e5219d-9ccb-4584-b078-5216ee4b9b8b",
                                        Conditions = new string[] { "'false'=='true'" }
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                Type = "Multi_task",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Basic_AE",
                            ExportDestinations = new string[]{"test"}
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Destination_Multiple_Condition_True_And_False",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] { "'true'=='true'" }
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                        Conditions = new string[] { "'false'=='true'" }
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Multi_Task_Workflow_Task_Destination_Invalid_Condition",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Multi Task workflow 3",
                        Description = "Multi Task workflow 3",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "36d29b9d-d496-4568-a305-f0775c0f2084",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = new string[] { "'invalid'>'false'" }
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Task_3"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Routing_Workflow_Multi_Destination",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Router Workflow Multi Destination",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "router",
                                Description = "Router Task",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination { Name = "taskdest1" },
                                    new TaskDestination { Name = "taskdest2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "taskdest1",
                                Type = "argo",
                                Description = "Argo plugin Task 1",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "taskdest2",
                                Type = "argo",
                                Description = "Argo plugin Task 2",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Router_1"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Routing_Workflow_Single_Destination",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Router Workflow Single Destination",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "router",
                                Description = "Router Workflow 1 Task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination { Name = "taskdest1" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "taskdest1",
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Router_1"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Routing_Workflow_Single_Destination_Conditional",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Router Workflow Conditional Statements",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "router",
                                Description = "Router Workflow 1 Task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination { Name = "taskdest1", Conditions = new string[] { "'true'=='true'" } },
                                    new TaskDestination { Name = "taskdest2", Conditions = new string[] { "'false'=='true'" } }
                                }
                            },
                            new TaskObject
                            {
                                Id = "taskdest1",
                                Type = "argo",
                                Description = "Argo plugin Task 1",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "taskdest2",
                                Type = "argo",
                                Description = "Argo plugin Task 2",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Router_1"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Routing_Workflow_No_Destination_Conditional",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Router Workflow Conditional Statements",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "router",
                                Description = "Router Workflow 1 Task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination { Name = "taskdest1", Conditions = new string[] { "'false'=='true'" } },
                                    new TaskDestination { Name = "taskdest2", Conditions = new string[] { "'false'=='true'" } }
                                }
                            },
                            new TaskObject
                            {
                                Id = "taskdest1",
                                Type = "argo",
                                Description = "Argo plugin Task 1",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "taskdest2",
                                Type = "argo",
                                Description = "Argo plugin Task 2",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Router_1"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Routing_Workflow_Multi_Router",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Router Workflow mulit router",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "router",
                                Description = "Router Workflow router task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination { Name = "taskdest1" },
                                    new TaskDestination { Name = "router2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "taskdest1",
                                Type = "argo",
                                Description = "Argo plugin Task 1",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "router2",
                                Type = "router",
                                Description = "Router Workflow router task 2",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination { Name = "taskdest2" },
                                    new TaskDestination { Name = "taskdest3" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "taskdest2",
                                Type = "argo",
                                Description = "Argo plugin Task 2",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "taskdest3",
                                Type = "argo",
                                Description = "Argo plugin Task 3",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Router_1"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Routing_Workflow_Invalid_Destination",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Router Workflow Conditional Statements",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "router",
                                Description = "Router Workflow 1 Task 1",
                                Artifacts = new ArtifactMap(),
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination { Name = "taskdest2"},
                                }
                            },
                            new TaskObject
                            {
                                Id = "taskdest1",
                                Type = "argo",
                                Description = "Argo plugin Task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Router_1"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "OneTask_Context.Dicom.Input_ArtifactMandatory=Null",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Input", Value = "{{ context.input.dicom }}" },
                                    },
                                }
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "OneTask_Context.Dicom.Input_ArtifactMandatory=True",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}", Mandatory = true },
                                    },
                                }
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "OneTask_Context.Dicom.Input_ArtifactMandatory=False",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}", Mandatory = false },
                                    },
                                }
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "TwoTask_Context.Dicom.Input_ArtifactMandatory=Null",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "artifact_task_2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "artifact_task_2",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 2",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "TwoTask_Context.Dicom.Input_ArtifactMandatory=True",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "artifact_task_2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "artifact_task_2",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 2",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "TwoTask_Context.Dicom.Input_ArtifactMandatory=False",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "artifact_task_2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "artifact_task_2",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 2",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Artifact_Workflow_Context.Input.Dicom_Non_Mandatory",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "artifact_task_2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "artifact_task_2",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 2",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}", Mandatory = false },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Workflow_Revision_for_export_single_dest_1",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "export_task_1" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "export_task_1",
                                Type = TaskTypeConstants.DicomExportTask,
                                Description = "Export Workflow 1 Task 2",
                                ExportDestinations = new ExportDestination[]
                                {
                                    new ExportDestination { Name = "PROD_PACS" }
                                },
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "output", Value = "{{ context.executions.artifact_task_1.artifacts.output }}" },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE",
                            ExportDestinations = new string[]
                            {
                                "PROD_PACS"
                            }
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Workflow_Revision_for_publish_an_invalid_task_update",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = "5A99B6B4-8ADF-45CA-A664-882C85399AEE",
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "export_task_1" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "export_task_1",
                                Type = TaskTypeConstants.DicomExportTask,
                                Description = "Export Workflow 1 Task 2",
                                ExportDestinations = new ExportDestination[]
                                {
                                    new ExportDestination { Name = "PROD_PACS" }
                                },
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "output", Value = "{{ context.executions.artifact_task_1.output_dir }}" },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE",
                            ExportDestinations = new string[]
                            {
                                "PROD_PACS"
                            }
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Workflow_Revision_for_export_folder",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "export_task_1" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "export_task_1",
                                Type = TaskTypeConstants.DicomExportTask,
                                Description = "Export Workflow 1 Task 2",
                                ExportDestinations = new ExportDestination[]
                                {
                                    new ExportDestination { Name = "PROD_PACS" }
                                },
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "output", Value = "{{ context.executions.artifact_task_1.output_dir }}" },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE",
                            ExportDestinations = new string[]
                            {
                                "PROD_PACS"
                            }
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Workflow_Revision_for_export_multi_dest_1",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "export_task_1" },
                                    new TaskDestination{ Name = "export_task_2" },
                                }
                            },
                            new TaskObject
                            {
                                Id = "export_task_1",
                                Type = TaskTypeConstants.DicomExportTask,
                                Description = "Export Workflow 1 Task 2",
                                ExportDestinations = new ExportDestination[]
                                {
                                    new ExportDestination { Name = "PROD_PACS" }
                                },
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "output", Value = "{{ context.executions.artifact_task_1.artifacts.output }}" },
                                    },
                                },
                            },
                            new TaskObject
                            {
                                Id = "export_task_2",
                                Type = TaskTypeConstants.DicomExportTask,
                                Description = "Export Workflow 1 Task 3",
                                ExportDestinations = new ExportDestination[]
                                {
                                    new ExportDestination { Name = "PROD_PACS2" }
                                },
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "output", Value = "{{ context.executions.artifact_task_1.artifacts.output }}" },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE",
                            ExportDestinations = new string[]
                            {
                                "PROD_PACS",
                                "PROD_PACS2"
                            }
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Workflow_Revision_for_export_multi_dest_2",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "export_task_1" },
                                }
                            },
                            new TaskObject
                            {
                                Id = "export_task_1",
                                Type = TaskTypeConstants.DicomExportTask,
                                Description = "Export Workflow 1 Task 2",
                                ExportDestinations = new ExportDestination[]
                                {
                                    new ExportDestination { Name = "PROD_PACS" }
                                },
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "output", Value = "{{ context.executions.artifact_task_1.artifacts.output }}", Mandatory = false },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "task_3" },
                                }
                            },
                            new TaskObject
                            {
                                Id = "task_3",
                                Type = "argo",
                                Description = "Argo plugin Task 1",
                                Artifacts = new ArtifactMap(),
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE",
                            ExportDestinations = new string[]
                            {
                                "PROD_PACS",
                            }
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=True",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "artifact_task_2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "artifact_task_2",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 2",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact
                                        {
                                            Name = "output",
                                            Value = "{{ context.executions.artifact_task_1.artifacts.output }}",
                                            Mandatory = true
                                        },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=False",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "artifact_task_2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "artifact_task_2",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 2",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact
                                        {
                                            Name = "output",
                                            Value = "{{ context.executions.artifact_task_1.artifacts.output }}",
                                            Mandatory = false
                                        },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Artifact.Artifact_Name_Mandatory=Null",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "artifact_task_2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "artifact_task_2",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 2",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact
                                        {
                                            Name = "output",
                                            Value = "{{ context.executions.artifact_task_1.artifacts.output }}"
                                        },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=True",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "artifact_task_2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "artifact_task_2",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 2",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact
                                        {
                                            Name = "output",
                                            Value = "{{ context.executions.artifact_task_1.output_dir }}",
                                            Mandatory = true
                                        },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=False",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "artifact_task_2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "artifact_task_2",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 2",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact
                                        {
                                            Name = "output",
                                            Value = "{{ context.executions.artifact_task_1.output_dir }}",
                                            Mandatory = false
                                        },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "TwoTask_Context.Executions.Task_id.Output_Dir_Mandatory=Null",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "artifact_task_2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "artifact_task_2",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 2",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact
                                        {
                                            Name = "output",
                                            Value = "{{ context.executions.artifact_task_1.output_dir }}"
                                        },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Mandatory_Output",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Artifact 1",
                        Description = "Artifact 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "artifact_task_1",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 1",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    },
                                    Output = new OutputArtifact[]
                                    {
                                        new OutputArtifact {
                                            Name = "output",
                                            Mandatory = true
                                        },
                                    }
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "artifact_task_2" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "artifact_task_2",
                                Type = "Artifact_task",
                                Description = "Artifact Workflow 1 Task 2",
                                Artifacts = new ArtifactMap()
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact
                                        {
                                            Name = "output",
                                            Value = "{{ context.input.dicom }}",
                                        },
                                    },
                                },
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Artifact_AE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_1_Deleted",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = "66678af8-e8ac-4b77-a431-9d1a289d6c3b",
                    WorkflowId = "c86a437d-d026-4bdf-b1df-c7a6372b89e3",
                    Revision = 1,
                    Deleted = new DateTime(2000, 01, 01, 0, 0, 0, kind: DateTimeKind.Utc),
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Args = new Dictionary<string, string> { { "test", "test" } },
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Static_AE",
                            ExportDestinations = new string[]{"test"}
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_Multiple_Revisions_1_Deleted",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = "85c48175-f4db-4d3c-bf3a-14f736edaccd",
                    Revision = 1,
                    Deleted = new DateTime(2000, 01, 01, 0, 0, 0, kind: DateTimeKind.Utc),
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow multiple revisions 1",
                        Description = "Basic workflow multiple revisions 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_Workflow_Multiple_Revisions_1",
                                Description = "Basic_Workflow_Multiple_Revisions_1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Revision"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_Multiple_Revisions_2_Deleted",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = "85c48175-f4db-4d3c-bf3a-14f736edaccd",
                    Revision = 2,
                    Deleted = new DateTime(2000, 01, 01, 0, 0, 0, kind: DateTimeKind.Utc),
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow multiple revisions 2",
                        Description = "Basic workflow multiple revisions 2",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_Workflow_Multiple_Revisions_2",
                                Description = "Basic_Workflow_Multiple_Revisions_2",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Revision"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_Multiple_Revisions_Different_AE_1",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = "85c48175-f4db-4d3c-bf3a-14f736edaccd",
                    Revision = 1,
                    Deleted = new DateTime(2000, 01, 01, 12, 00, 00, DateTimeKind.Utc),
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow multiple revisions 1",
                        Description = "Basic workflow multiple revisions 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_Workflow_Multiple_Revisions_1",
                                Description = "Basic_Workflow_Multiple_Revisions_1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Rev_Old"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_Multiple_Revisions_Different_AE_2",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = "85c48175-f4db-4d3c-bf3a-14f736edaccd",
                    Revision = 2,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow multiple revisions 2",
                        Description = "Basic workflow multiple revisions 2",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_Workflow_Multiple_Revisions_2",
                                Description = "Basic_Workflow_Multiple_Revisions_2",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Rev_New"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Workflow_Called_AET",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow 1",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "AIDE"
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Workflow_Called_AET_Calling_AET",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow 1",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "AIDE",
                            DataOrigins = new string[] { "PACS1" }
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Workflow_Called_AET_Multi_Calling_AET",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow 1",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "AIDE",
                            DataOrigins = new string[] { "PACS1", "PACS2" }
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Workflow_Revision_For_Artifact_ReceivedEvent_1",
                WorkflowRevision = new WorkflowRevision()
                {
                    Id = "293C95D6-91AE-4417-95CA-D54FF9E592D6",
                    WorkflowId = "C139946F-0FB9-452C-843A-A77F4BAACB8E",
                    Revision = 1,
                    Workflow = new Workflow()
                    {
                        Name = "Basic workflow 1",
                        Description = "Basic workflow 1",
                        Version = "1",
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "e545de90-c936-40ab-ad11-19ef07f4960a",
                                Type = "root_task",
                                Description = "Basic Workflow 1 Task 1 - root task",
                                Artifacts = new ArtifactMap(),
                            },
                            new TaskObject
                            {
                                Id = "e545de90-c936-40ab-ad11-19ef07f49607",
                                Type = "remote_task",
                                Description = "Basic Workflow 1 Task 2 - remote_task",
                                Artifacts = new ArtifactMap()
                                {
                                    Output = new OutputArtifact[]
                                    {
                                        new OutputArtifact()
                                        {
                                            Name = "artifact1",
                                            Type = ArtifactType.CT,
                                            Value = "artifactPath1",
                                            Mandatory = true,
                                        },
                                        new OutputArtifact()
                                        {
                                            Name = "artifact2",
                                            Type = ArtifactType.AR,
                                            Value = "artifactPath2",
                                            Mandatory = true,
                                        },
                                    }
                                }
                            },
                            new TaskObject
                            {
                                Id = "e545de90-c936-40ab-ad11-19ef07f4960b",
                                Type = "clinical_review",
                                Description = "Basic Workflow 1 Task 3 - clinical_review",
                                Artifacts = new ArtifactMap(),
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "AIDE",
                            DataOrigins = new string[] { "PACS1", "PACS2" }
                        }
                    }
                }
            },
        };
    }
}
