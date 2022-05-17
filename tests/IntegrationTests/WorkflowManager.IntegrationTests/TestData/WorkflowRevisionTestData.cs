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
                    Revision = 1,
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
                Name = "Multi_Task_Workflow",
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
        };
    }
}

