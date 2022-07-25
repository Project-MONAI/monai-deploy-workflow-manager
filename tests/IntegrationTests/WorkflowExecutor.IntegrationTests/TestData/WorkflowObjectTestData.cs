// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.WorkflowExecutor.IntegrationTests.TestData
{
    public class WorkflowObjectTestData
    {
        public string? Name { get; set; }

        public Workflow? Workflow { get; set; }
    }

    public static class WorkflowObjectsTestData
    {
        public static List<WorkflowObjectTestData> TestData = new List<WorkflowObjectTestData>()
        {
            new WorkflowObjectTestData()
            {
                Name = "Basic_Workflow_Update_1",
                Workflow = new Workflow()
                {
                    Name = "Basic update",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "Basic_task",
                            Description = "Basic Workflow update Task update",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new Artifact[] {}
                            },
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        DataOrigins = new string[]{"test"},
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Update_Name_Length",
                Workflow = new Workflow()
                {
                    Name = "Over 15 characters",
                    Description = "Basic workflow 1",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "Basic_task",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new Artifact[] {}
                            },
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
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Update_Desc_Length",
                Workflow = new Workflow()
                {
                    Name = "Basic workflow",
                    Description = "Over 200 chars dolor sit amet, consectetur adipiscing elit. Donec bibendum dapibus elit, quis tempus metus. Sed aliquam metus tempus pretium pharetra. Etiam a est id nunc tempor consectetur. Proin turpis.",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "Basic_task",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new Artifact[] {}
                            },
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
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Update_Desc_Length",
                Workflow = new Workflow()
                {
                    Name = "Inv_Desc",
                    Description = "Over 200 chars dolor sit amet, consectetur adipiscing elit. Donec bibendum dapibus elit, quis tempus metus. Sed aliquam metus tempus pretium pharetra. Etiam a est id nunc tempor consectetur. Proin turpis.",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "Basic_task",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new Artifact[] {}
                            },
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
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Update_AETitle_Length",
                Workflow = new Workflow()
                {
                    Name = "Inv_AE",
                    Description = "Inv_AE",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "Basic_task",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new Artifact[] {}
                            },
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Over 15 characters",
                        DataOrigins = new string[]{"test"},
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Update_DataOrg",
                Workflow = new Workflow()
                {
                    Name = "Inv_DataOrg",
                    Description = "Inv_DataOrg",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "Basic_task",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new Artifact[] {}
                            },
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Basic_AE",
                        DataOrigins = new string[]{},
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Update_ExportDest",
                Workflow = new Workflow()
                {
                    Name = "Inv_ExpDest",
                    Description = "Inv_ExpDest",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "Basic_task",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new Artifact[] {}
                            },
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Basic_AE",
                        DataOrigins = new string[]{"test"},
                        ExportDestinations = new string[]{}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Update_TaskDesc_Length",
                Workflow = new Workflow()
                {
                    Name = "Inv_TaskDesc",
                    Description = "Inv_TaskDesc",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "Basic_task",
                            Description = "Over 2000 chars dolor sit amet, consectetur adipiscing elit. Praesent eget consequat ante. Vivamus convallis porttitor vehicula. Proin turpis ante, fringilla nec sem sit amet, commodo tristique magna. Cras quam est, vehicula vitae tellus et, faucibus hendrerit lacus. Curabitur at ligula urna. Nam pretium felis et orci consectetur suscipit. In finibus arcu erat, quis congue libero finibus quis. Praesent mollis viverra arcu, vel finibus ligula auctor et. Phasellus ac lacus id tellus tempor porta. Sed quis nibh malesuada, consectetur sem sed, aliquam sapien. Mauris vehicula massa elit, vel varius massa fringilla non. Proin hendrerit dui nibh, ultrices egestas lacus ultrices ut. Vestibulum vitae nunc eget nunc hendrerit blandit. Nunc ut erat nisl. Sed condimentum gravida augue. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Curabitur vel mi congue, vehicula arcu quis, placerat ligula. In dapibus quam eget aliquam euismod. Praesent eget augue lacus. Aenean malesuada rhoncus fringilla. Sed non lectus nulla. Morbi maximus mi nec vulputate rhoncus. In sit amet ultrices massa. Duis a nunc congue, blandit purus non, dignissim nisl. In risus neque, fringilla sit amet laoreet vitae, aliquam et risus. In nec sem nulla. Aenean rutrum urna odio, et ornare diam pharetra nec. Cras suscipit diam vitae condimentum rutrum. Proin lacinia augue leo, et consectetur eros accumsan eget. Donec nec lacinia dui. Aenean imperdiet finibus volutpat. Proin scelerisque ante ligula, at faucibus odio bibendum vitae. Duis luctus arcu nibh, ac elementum libero ullamcorper id. Curabitur fermentum neque dui, ut suscipit ipsum pellentesque eu. Curabitur dapibus, magna egestas semper volutpat, ligula urna eleifend nunc, eget tristique lacus sapien in nisi. Etiam id bibendum purus. Proin dictum, orci in egestas faucibus, ipsum felis ullamcorper lectus, ac bibendum arcu arcu eu tellus. Aliquam a posuere nibh. Duis euismod magna a faucibus tempor. Vestibulum et lacus molestie.",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new Artifact[] {}
                            },
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
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Update_TaskType_Length",
                Workflow = new Workflow()
                {
                    Name = "Inv_TaskType",
                    Description = "Inv_TaskType",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "Over 2000 chars dolor sit amet, consectetur adipiscing elit. Praesent eget consequat ante. Vivamus convallis porttitor vehicula. Proin turpis ante, fringilla nec sem sit amet, commodo tristique magna. Cras quam est, vehicula vitae tellus et, faucibus hendrerit lacus. Curabitur at ligula urna. Nam pretium felis et orci consectetur suscipit. In finibus arcu erat, quis congue libero finibus quis. Praesent mollis viverra arcu, vel finibus ligula auctor et. Phasellus ac lacus id tellus tempor porta. Sed quis nibh malesuada, consectetur sem sed, aliquam sapien. Mauris vehicula massa elit, vel varius massa fringilla non. Proin hendrerit dui nibh, ultrices egestas lacus ultrices ut. Vestibulum vitae nunc eget nunc hendrerit blandit. Nunc ut erat nisl. Sed condimentum gravida augue. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Curabitur vel mi congue, vehicula arcu quis, placerat ligula. In dapibus quam eget aliquam euismod. Praesent eget augue lacus. Aenean malesuada rhoncus fringilla. Sed non lectus nulla. Morbi maximus mi nec vulputate rhoncus. In sit amet ultrices massa. Duis a nunc congue, blandit purus non, dignissim nisl. In risus neque, fringilla sit amet laoreet vitae, aliquam et risus. In nec sem nulla. Aenean rutrum urna odio, et ornare diam pharetra nec. Cras suscipit diam vitae condimentum rutrum. Proin lacinia augue leo, et consectetur eros accumsan eget. Donec nec lacinia dui. Aenean imperdiet finibus volutpat. Proin scelerisque ante ligula, at faucibus odio bibendum vitae. Duis luctus arcu nibh, ac elementum libero ullamcorper id. Curabitur fermentum neque dui, ut suscipit ipsum pellentesque eu. Curabitur dapibus, magna egestas semper volutpat, ligula urna eleifend nunc, eget tristique lacus sapien in nisi. Etiam id bibendum purus. Proin dictum, orci in egestas faucibus, ipsum felis ullamcorper lectus, ac bibendum arcu arcu eu tellus. Aliquam a posuere nibh. Duis euismod magna a faucibus tempor. Vestibulum et lacus molestie.",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new Artifact[] {}
                            },
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
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Update_TaskArgs",
                Workflow = new Workflow()
                {
                    Name = "Inv_TaskArgs",
                    Description = "Inv_TaskArgs",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "Basic_task",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new Artifact[] {}
                            },
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Basic_AE",
                        DataOrigins = new string[]{"test"},
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
        };
    }
}
