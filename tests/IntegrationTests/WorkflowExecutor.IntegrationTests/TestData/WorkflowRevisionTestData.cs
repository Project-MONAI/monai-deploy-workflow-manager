// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.TestData
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
                                Description = "Basic Workflow 1 Task 1"
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Basic_AE"
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
                                Description = "Basic Workflow 2 Task 1"
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Basic_AE"
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
                                Description = "Basic Workflow 3 Task 1"
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Basic_AE_3"
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
                                Description = "Same AeTitle 1 Task 1"
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
                                Description = "Same AeTitle 2 Task 1"
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
                Name = "Basic_Workflow_Multiple_Revisions_1",
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
                                Description = "Basic_Workflow_Multiple_Revisions_1"
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
                                Description = "Basic_Workflow_Multiple_Revisions_2"
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
                                Description = "Multi task workflow Task 1"
                            },
                            new TaskObject
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = "task_2",
                                Description = "Multi task workflow Task 2"
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
                                Description = "Task Update Task 1"
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
                                Description = "Multiple request task 1"
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Multi_Created"
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
                                Description = "Multiple request task 1"
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
                            },
                            new TaskObject
                            {
                                Id = "eefdd563-c589-433a-969f-2cddfe93af24",
                                Type = "Multi_task",
                                Description = "Multiple request task 3",
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
                                Args = new Dictionary<string, string> { { "test", "test" } }
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Static_AE",
                            DataOrigins = new string[]{"test"},
                            ExportDestinations = new string[]{"test"}
                        }
                    }
                }
            },
            new WorkflowRevisionTestData()
            {
                Name = "Basic_Workflow_multiple_revisions_1",
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
                                Description = "Basic Workflow 1 Task 1"
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Basic_AE"
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
                                Description = "Basic Workflow 1 Task 1"
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Basic_AE"
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
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Conditions = "{{ context.dicom.series.any('0010','0040') }} == 'lordge'",
                                        Name = "cake"
                                    }
                                }
                            },
                            new TaskObject
                            {
                                Id = "cake",
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1",
                                Args = new Dictionary<string, string> { { "test", "test" } }

                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Basic_AE",
                            DataOrigins = new string[]{"test"},
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
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = "'true'=='true'"
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
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
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = "'false'=='true'"
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
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
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = "'true'=='true'"
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                        Conditions = "'true'=='true'"
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "d2e5219d-9ccb-4584-b078-5216ee4b9b8b",
                                        Conditions = "'true'=='true'"
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                            },
                            new TaskObject
                            {
                                Id = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                            },
                            new TaskObject
                            {
                                Id = "d2e5219d-9ccb-4584-b078-5216ee4b9b8b",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
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
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = "'false'=='true'"
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                        Conditions = "'false'=='true'"
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "d2e5219d-9ccb-4584-b078-5216ee4b9b8b",
                                        Conditions = "'false'=='true'"
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                            },
                            new TaskObject
                            {
                                Id = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                Type = "Multi_task",
                            }
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Basic_AE",
                            DataOrigins = new string[]{"test"},
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
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = "'true'=='true'"
                                    },
                                    new TaskDestination()
                                    {
                                        Name = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                        Conditions = "'false'=='true'"
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
                            },
                            new TaskObject
                            {
                                Id = "e12849f3-247d-47eb-95c8-5aa16f551f62",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
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
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination()
                                    {
                                        Name = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                        Conditions = "'invalid'>'false'"
                                    },
                                }
                            },
                            new TaskObject
                            {
                                Id = "b9964b10-acb4-4050-a610-374fdbe2100d",
                                Type = "Multi_task",
                                Description = "Multiple request task 1",
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
                                Description = "Argo plugin Task 1"
                            },
                            new TaskObject
                            {
                                Id = "taskdest2",
                                Type = "argo",
                                Description = "Argo plugin Task 2"
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
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination { Name = "taskdest1" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "taskdest1",
                                Type = "Basic_task",
                                Description = "Basic Workflow 1 Task 1"
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
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination { Name = "taskdest1", Conditions = "'true'=='true'" },
                                    new TaskDestination { Name = "taskdest2", Conditions = "'false'=='true'" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "taskdest1",
                                Type = "argo",
                                Description = "Argo plugin Task 1"
                            },
                            new TaskObject
                            {
                                Id = "taskdest2",
                                Type = "argo",
                                Description = "Argo plugin Task 2"
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
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination { Name = "taskdest1", Conditions = "'false'=='true'" },
                                    new TaskDestination { Name = "taskdest2", Conditions = "'false'=='true'" }
                                }
                            },
                            new TaskObject
                            {
                                Id = "taskdest1",
                                Type = "argo",
                                Description = "Argo plugin Task 1"
                            },
                            new TaskObject
                            {
                                Id = "taskdest2",
                                Type = "argo",
                                Description = "Argo plugin Task 2"
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
                                Description = "Argo plugin Task 1"
                            },
                            new TaskObject
                            {
                                Id = "router2",
                                Type = "router",
                                Description = "Router Workflow router task 2",
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
                                Description = "Argo plugin Task 2"
                            },
                            new TaskObject
                            {
                                Id = "taskdest3",
                                Type = "argo",
                                Description = "Argo plugin Task 3"
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
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination { Name = "taskdest2"},
                                }
                            },
                            new TaskObject
                            {
                                Id = "taskdest1",
                                Type = "argo",
                                Description = "Argo plugin Task 1"
                            },
                        },
                        InformaticsGateway = new InformaticsGateway()
                        {
                            AeTitle = "Router_1"
                        }
                    }
                }
            },
        };
    }
}
