// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.TestData
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
                        AeTitle = "Basic_AE",
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
                    Name = "Over 15 characters",
                    Description = "Over 200 chars dolor sit amet, consectetur adipiscing elit. Donec bibendum dapibus elit, quis tempus metus. Sed aliquam metus tempus pretium pharetra. Etiam a est id nunc tempor consectetur. Proin turpis.",
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
        };
    }
}
