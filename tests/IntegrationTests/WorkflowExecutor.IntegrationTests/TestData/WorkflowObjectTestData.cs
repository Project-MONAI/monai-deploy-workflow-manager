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
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Artifact = Monai.Deploy.WorkflowManager.Common.Contracts.Models.Artifact;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData
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
                Name = "Basic_Workflow_1",
                Workflow = new Workflow()
                {
                    Name = "Basic update",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "basic_id_with-legal-chars",
                            Type = "router",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "test", "test" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Basic_Workflow_2",
                Workflow = new Workflow()
                {
                    Name = "Basic update",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "basic_id_with-legal-chars",
                            Type = "router",
                            Description = "Basic Workflow update Task update",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {}
                            }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Missing_Name",
                Workflow = new Workflow()
                {
                    Name = "",
                    Description = "Basic workflow 1",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "router",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Basic_AE",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Desc_Length",
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
                            Type = "router",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Basic_AE",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Desc_Length",
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
                            Type = "router",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Basic_AE",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_AETitle_Length",
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
                            Type = "router",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Over 15 characters",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_DataOrg",
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
                            Type = "router",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
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
                Name = "Invalid_Workflow_ExportDest",
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
                            Type = "router",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Basic_AE",
                        ExportDestinations = new string[]{}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_TaskDesc_Length",
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
                            Type = "router",
                            Description = "Over 2000 chars dolor sit amet, consectetur adipiscing elit. Praesent eget consequat ante. Vivamus convallis porttitor vehicula. Proin turpis ante, fringilla nec sem sit amet, commodo tristique magna. Cras quam est, vehicula vitae tellus et, faucibus hendrerit lacus. Curabitur at ligula urna. Nam pretium felis et orci consectetur suscipit. In finibus arcu erat, quis congue libero finibus quis. Praesent mollis viverra arcu, vel finibus ligula auctor et. Phasellus ac lacus id tellus tempor porta. Sed quis nibh malesuada, consectetur sem sed, aliquam sapien. Mauris vehicula massa elit, vel varius massa fringilla non. Proin hendrerit dui nibh, ultrices egestas lacus ultrices ut. Vestibulum vitae nunc eget nunc hendrerit blandit. Nunc ut erat nisl. Sed condimentum gravida augue. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Curabitur vel mi congue, vehicula arcu quis, placerat ligula. In dapibus quam eget aliquam euismod. Praesent eget augue lacus. Aenean malesuada rhoncus fringilla. Sed non lectus nulla. Morbi maximus mi nec vulputate rhoncus. In sit amet ultrices massa. Duis a nunc congue, blandit purus non, dignissim nisl. In risus neque, fringilla sit amet laoreet vitae, aliquam et risus. In nec sem nulla. Aenean rutrum urna odio, et ornare diam pharetra nec. Cras suscipit diam vitae condimentum rutrum. Proin lacinia augue leo, et consectetur eros accumsan eget. Donec nec lacinia dui. Aenean imperdiet finibus volutpat. Proin scelerisque ante ligula, at faucibus odio bibendum vitae. Duis luctus arcu nibh, ac elementum libero ullamcorper id. Curabitur fermentum neque dui, ut suscipit ipsum pellentesque eu. Curabitur dapibus, magna egestas semper volutpat, ligula urna eleifend nunc, eget tristique lacus sapien in nisi. Etiam id bibendum purus. Proin dictum, orci in egestas faucibus, ipsum felis ullamcorper lectus, ac bibendum arcu arcu eu tellus. Aliquam a posuere nibh. Duis euismod magna a faucibus tempor. Vestibulum et lacus molestie.",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Basic_AE",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_TaskType",
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
                                Output = new OutputArtifact[] {}
                            },
                            Description = "Basic Workflow 1 Task 1",
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Basic_AE",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_TaskArgs",
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
                            Type = "router",
                            Description = "Basic Workflow 1 Task 1",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Basic_AE",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_TaskID_Length",
                Workflow = new Workflow()
                {
                    Name = "Inv_TaskID",
                    Description = "Inv_TaskID",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "Over 50 chars Lorem ipsum dolor sit amet consectetur adipiscing elit ligula",
                            Type = "router",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            Description = "Basic Workflow 1 Task 1",
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Basic_AE",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_TaskID_Content",
                Workflow = new Workflow()
                {
                    Name = "Inv_TaskID",
                    Description = "Inv_TaskID",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "Invalid chars ./'#;][",
                            Type = "router",
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            Description = "Basic Workflow 1 Task 1",
                            Args = new Dictionary<string, string> { { "test", "test" } }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Basic_AE",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Unreferenced_Task",
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
                            Args = new Dictionary<string, string> { { "test", "test" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                },
                            },
                        },
                        new TaskObject
                        {
                            Id = "artifact_task_2",
                            Type = "Artifact_task",
                            Description = "Artifact Workflow 1 Task 2",
                            Args = new Dictionary<string, string> { { "test", "test" } },
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
                        AeTitle = "Artifact_AE",
                        ExportDestinations = new string[]{"test"},
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Loopback_Task",
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
                            Args = new Dictionary<string, string> { { "test", "test" } },
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
                            Args = new Dictionary<string, string> { { "test", "test" } },
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
                            TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination{ Name = "artifact_task_1" }
                                }
                        },
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Artifact_AE",
                        ExportDestinations = new string[]{"test"},
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_0_Tasks",
                Workflow = new Workflow()
                {
                    Name = "Artifact 1",
                    Description = "Artifact 1",
                    Version = "1",
                    Tasks = new TaskObject[] {},
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Artifact_AE",
                        ExportDestinations = new string[]{"test"},
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Version_Null",
                Workflow = new Workflow()
                {
                    Name = "Artifact 1",
                    Description = "Artifact 1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "artifact_task_1",
                            Type = "Artifact_task",
                            Description = "Artifact Workflow 1 Task 1",
                            Args = new Dictionary<string, string> { { "test", "test" } },
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
                            Args = new Dictionary<string, string> { { "test", "test" } },
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
                        AeTitle = "Artifact_AE",
                        ExportDestinations = new string[]{"test"},
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Version_Blank",
                Workflow = new Workflow()
                {
                    Name = "Artifact 1",
                    Description = "Artifact 1",
                    Version = "",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "artifact_task_1",
                            Type = "Artifact_task",
                            Description = "Artifact Workflow 1 Task 1",
                            Args = new Dictionary<string, string> { { "test", "test" } },
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
                            Args = new Dictionary<string, string> { { "test", "test" } },
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
                        AeTitle = "Artifact_AE",
                        ExportDestinations = new string[]{"test"},
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Dup_Output",
                Workflow = new Workflow()
                {
                    Name = "Dup Output",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "basic_id_with-legal-chars",
                            Type = "router",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "test", "test" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[]
                                {
                                new OutputArtifact
                                    {
                                        Name = "non_unique_name",
                                        Value = "{{ context.executions.artifact_task_1.output_dir }}",
                                        Mandatory = true
                                    },
                                new OutputArtifact
                                    {
                                        Name = "non_unique_name",
                                        Value = "{{ context.executions.artifact_task_1.output_dir }}",
                                        Mandatory = true
                                    },
                                },
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Body_Object",
                Workflow = new Workflow()
                {
                    Name = "Artifact 1",
                    Description = "Artifact 1",
                    Version = ""
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Empty_Workflow_Body",
                Workflow = new Workflow()
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Missing_QueueName",
                Workflow = new Workflow()
                {
                    Name = "Basic update",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "Argo_Task",
                            Type = "argo",
                            Description = "Argo task missing QueueName",
                            Args = new Dictionary<string, string> {
                                { "workflow_name", "Workflow Name" },
                                { "reviewed_task_id", "Task ID" }
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Missing_WorkflowName",
                Workflow = new Workflow()
                {
                    Name = "Basic update",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "Argo_Task",
                            Type = "argo",
                            Description = "Argo task missing QueueName",
                            Args = new Dictionary<string, string> {
                                { "queue_name", "Queue Name" },
                                { "reviewed_task_id", "Task ID" }
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Missing_ReviewedTaskId",
                Workflow = new Workflow()
                {
                    Name = "Basic update",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "clinical_review",
                            Type = "aide_clinical_review",
                            Description = "Clinical Review task missing ReviewedTaskId",
                            Args = new Dictionary<string, string> {
                                { "mode", "qa" },
                                { "application_name", "Name" },
                                { "application_version", "Version" },
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Missing_All_Argo_Args",
                Workflow = new Workflow()
                {
                    Name = "Basic update",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "Argo_Task",
                            Type = "argo",
                            Description = "Argo task missing QueueName",
                            Args = new Dictionary<string, string> {},
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Missing_2_Argo_Args_1",
                Workflow = new Workflow()
                {
                    Name = "Basic update",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "Argo_Task",
                            Type = "argo",
                            Description = "Argo task missing QueueName",
                            Args = new Dictionary<string, string> {
                                { "workflow_name", "Workflow Name" },
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Missing_2_Argo_Args_2",
                Workflow = new Workflow()
                {
                    Name = "Basic update",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "Argo_Task",
                            Type = "argo",
                            Description = "Argo task missing QueueName",
                            Args = new Dictionary<string, string> {
                                { "queue_name", "Queue Name" },
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Missing_2_Argo_Args_3",
                Workflow = new Workflow()
                {
                    Name = "Basic update",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "Argo_Task",
                            Type = "argo",
                            Description = "Argo task missing QueueName",
                            Args = new Dictionary<string, string> {
                                { "reviewed_task_id", "Task ID" }
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Incorrect_Clinical_Review_Artifact",
                Workflow = new Workflow()
                {
                    Name = "Basic update",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "Argo_Task",
                            Type = "argo",
                            Description = "Argo task missing QueueName",
                            Args = new Dictionary<string, string> {
                                { "workflow_name", "Workflow Name" },
                                { "queue_name", "Queue Name" },
                                { "reviewed_task_id", "Task ID" }
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {
                                new TaskDestination
                                {
                                    Name = "Clinical_Review_Task"
                                }
                            }
                        },
                        new TaskObject
                        {
                            Id = "Clinical_Review_Task",
                            Type = "aide_clinical_review",
                            Description = "Clinical Review incorrect Input",
                            Args = new Dictionary<string, string> {},
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] { new Artifact { Name = "test", Value = "{{ context.executions.mean-pixel-calc.artifacts.report }}" } },
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Workflow_Dup_Workflow_Name",
                Workflow = new Workflow()
                {
                    Name = "Basic workflow",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "basic_id_with-legal-chars",
                            Type = "router",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "test", "test" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Clinical_Review_Task_Id",
                Workflow = new Workflow()
                {
                    Name = "Basic workflow",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "router",
                            Type = "router",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string>{ },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name="clinical-review"
                                }
                            }
                        },
                        new TaskObject
                        {
                            Id = "clinical-review",
                            Type = "aide_clinical_review",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string>
                            {
                                { "workflow_name", "test" },
                                { "reviewed_task_id", "router" },
                                { "application_name", "test" },
                                { "application_version", "1.1" },
                                { "mode", "QA" },
                                { "notifications", "false" }
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                },
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] { }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Clinical_Review_Multiple_Argo_Inputs",
                Workflow = new Workflow()
                {
                    Name = "Basic workflow",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "argo-task-1",
                            Type = "argo",
                            Description = "Argo task",
                            Args = new Dictionary<string, string> {
                                { "workflow_template_name", "Workflow Name" },
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                },
                                Output = new OutputArtifact[]
                                {
                                    new OutputArtifact { Name = "Argo1" }
                                }
                            },
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "clinical-review"
                                },
                                new TaskDestination
                                {
                                    Name = "argo-task-2"
                                }
                            }
                        },
                        new TaskObject
                        {
                            Id = "argo-task-2",
                            Type = "argo",
                            Description = "Argo task",
                            Args = new Dictionary<string, string> {
                                { "workflow_template_name", "Workflow Name" },
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                },
                                Output = new OutputArtifact[]
                                {
                                    new OutputArtifact { Name = "Argo2" }
                                }
                            },
                            TaskDestinations = new TaskDestination[] {
                                new TaskDestination
                                {
                                    Name = "clinical-review"
                                }
                            }
                        },
                        new TaskObject
                        {
                            Id = "clinical-review",
                            Type = "aide_clinical_review",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string>
                            {
                                { "workflow_name", "test" },
                                { "reviewed_task_id", "argo-task-1" },
                                { "application_name", "test" },
                                { "application_version", "1.1" },
                                { "mode", "QA" },
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                    new Artifact { Name = "Argo1", Value = "{{ context.executions.argo-task-1.artifacts.Argo1 }}" },
                                    new Artifact { Name = "Argo2", Value = "{{ context.executions.argo-task-2.artifacts.Argo2 }}" },
                                },
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Dup_Task_Id",
                Workflow = new Workflow()
                {
                    Name = "Basic workflow",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "liver-seg",
                            Type = "argo",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "workflow_template_name", "argo-workflow-2" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact
                                    {
                                        Name = "Dicom", Value = "{{ context.input.dicom }}"
                                    }
                                },
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        },
                        new TaskObject
                        {
                            Id = "liver-seg",
                            Type = "argo",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "workflow_template_name", "argo-workflow-2" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact
                                    {
                                        Name = "Dicom", Value = "{{ context.input.dicom }}"
                                    }
                                },
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Workflow_Coverging_Task_Dest",
                Workflow = new Workflow()
                {
                    Name = "Basic workflow",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "router_task",
                            Type = "router",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "workflow_template_name", "argo-workflow-2" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact
                                    {
                                        Name = "Dicom", Value = "{{ context.input.dicom }}"
                                    }
                                },
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[]
                            {
                                 new TaskDestination{ Name = "task1" },
                                 new TaskDestination{ Name = "task2" }
                            }
                        },
                        new TaskObject
                        {
                            Id = "task1",
                            Type = "argo",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "workflow_template_name", "argo-workflow-2" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact
                                    {
                                        Name = "Dicom", Value = "{{ context.input.dicom }}"
                                    }
                                },
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[]
                            {
                                 new TaskDestination{ Name = "task3" }
                            }
                        },
                        new TaskObject
                        {
                            Id = "task2",
                            Type = "argo",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "workflow_template_name", "argo-workflow-2" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact
                                    {
                                        Name = "Dicom", Value = "{{ context.input.dicom }}"
                                    }
                                },
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[]
                            {
                                 new TaskDestination{ Name = "task3" }
                            }
                        },
                        new TaskObject
                        {
                            Id = "task3",
                            Type = "argo",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "workflow_template_name", "argo-workflow-2" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact
                                    {
                                        Name = "Dicom", Value = "{{ context.input.dicom }}"
                                    }
                                },
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] { }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Clinical_Review_Missing_Notifications",
                Workflow = new Workflow()
                {
                    Name = "Basic workflow",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "argo-task",
                            Type = "argo",
                            Description = "Argo task",
                            Args = new Dictionary<string, string> {
                                { "workflow_template_name", "Workflow Name" },
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                },
                                Output = new OutputArtifact[]
                                {
                                    new OutputArtifact { Name = "Argo2" }
                                }
                            },
                            TaskDestinations = new TaskDestination[] {
                                new TaskDestination
                                {
                                    Name = "clinical-review"
                                }
                            }
                        },
                        new TaskObject
                        {
                            Id = "clinical-review",
                            Type = "aide_clinical_review",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string>
                            {
                                { "workflow_name", "test" },
                                { "reviewed_task_id", "argo-task" },
                                { "application_name", "test" },
                                { "application_version", "1.1" },
                                { "mode", "QA" },
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                },
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] { }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Clinical_Review_Invalid_Notifications",
                Workflow = new Workflow()
                {
                    Name = "Basic workflow",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "argo-task",
                            Type = "argo",
                            Description = "Argo task",
                            Args = new Dictionary<string, string> {
                                { "workflow_template_name", "Workflow Name" },
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                },
                                Output = new OutputArtifact[]
                                {
                                    new OutputArtifact { Name = "Argo2" }
                                }
                            },
                            TaskDestinations = new TaskDestination[] {
                                new TaskDestination
                                {
                                    Name = "clinical-review"
                                }
                            }
                        },
                        new TaskObject
                        {
                            Id = "clinical-review",
                            Type = "aide_clinical_review",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string>
                            {
                                { "workflow_name", "test" },
                                { "reviewed_task_id", "argo-task" },
                                { "application_name", "test" },
                                { "application_version", "1.1" },
                                { "mode", "QA" },
                                { "notifications", "dog" }
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                },
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] { }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Valid_Workflow_With_Clinical_Review",
                Workflow = new Workflow()
                {
                    Name = "Basic workflow",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "argo-task",
                            Type = "argo",
                            Description = "Argo task",
                            Args = new Dictionary<string, string> {
                                { "workflow_template_name", "Workflow Name" },
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                },
                                Output = new OutputArtifact[]
                                {
                                    new OutputArtifact { Name = "Argo2" }
                                }
                            },
                            TaskDestinations = new TaskDestination[] {
                                new TaskDestination
                                {
                                    Name = "clinical-review"
                                }
                            }
                        },
                        new TaskObject
                        {
                            Id = "clinical-review",
                            Type = "aide_clinical_review",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string>
                            {
                                { "workflow_name", "test" },
                                { "reviewed_task_id", "argo-task" },
                                { "application_name", "test" },
                                { "application_version", "1.1" },
                                { "mode", "QA" },
                                { "notifications", "true" }
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact { Name = "Dicom", Value = "{{ context.input.dicom }}" },
                                },
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] { }
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                },
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_Data_Origin",
                Workflow = new Workflow()
                {
                    Name = "Basic workflow",
                    Description = "Basic workflow update",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "basic_id_with-legal-chars",
                            Type = "router",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "test", "test" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"},
                        DataOrigins = new string[] { "invalid_source" }
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Valid_remote_task",
                Workflow = new Workflow()
                {
                    Name = "Valid_remote_task",
                    Description = "Valid remote task",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "Valid_remote_task_step1_router",
                            Type = "router",
                            Description = "Valid remote Workflow Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "test", "test" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination()
                                {
                                    Name = "Valid_remote_task_step2_remote_app"
                                }
                            }
                        },
                        new TaskObject
                        {
                            Id = "Valid_remote_task_step2_remote_app",
                            Type = "remote_app_execution",
                            Description = "Valid remote Workflow Basic remote app execution task",
                            Args = new Dictionary<string, string> { { "test", "test" } },
                            ExportDestinations = new ExportDestination[] { new ExportDestination { Name = "test" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact()
                                    {
                                        Name = "input.dcm",
                                        Mandatory = true,
                                        Value = "input.dcm"
                                    }
                                },
                                Output = new OutputArtifact[]
                                {
                                    new OutputArtifact()
                                    {
                                        Type = ArtifactType.AU,
                                        Mandatory = true,
                                        Name = "output.pdf",
                                    }
                                }
                            },
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination()
                                {
                                    Name = "valid_clinical_review",
                                }
                            }
                        },
                        new TaskObject
                        {
                            Id = "valid_clinical_review",
                            Type = "aide_clinical_review",
                            Description = "Valid remote Workflow Clinical Review",
                            Args = new Dictionary<string, string> {
                                { "mode", "qa" },
                                { "application_name", "Name" },
                                { "application_version", "Version" },
                                { "reviewed_task_id", "Valid_remote_task_step2_remote_app"},
                                { "notifications", "false" }
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[]
                                {
                                    new Artifact()
                                    {
                                        Name = "input.dcm",
                                        Mandatory = true,
                                        Value = "{{ context.executions.Valid_remote_task_step2_remote_app.artifacts.report }}"
                                    }
                                },
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "Invalid_remote_task_without_outputs_type_set",
                Workflow = new Workflow()
                {
                    Name = "Invalid_remote_task_without_outputs_type_set",
                    Description = "Invalid remote task without outputs type set",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "remote_task_without_outputs_type_set_step1_router",
                            Type = "router",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "test", "test" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        },
                        new TaskObject
                        {
                            Id = "invalid_remote_task_step2_remote_app",
                            Type = "remote_app_execution",
                            Description = "Basic remote app execution task",
                            Args = new Dictionary<string, string> { { "test", "test" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[]
                                {
                                    new OutputArtifact()
                                }
                            },
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
            new WorkflowObjectTestData()
            {
                Name = "invalid_remote_task_without_outputs",
                Workflow = new Workflow()
                {
                    Name = "invalid_remote_task_without_outputs",
                    Description = "invalid remote task without outputs",
                    Version = "1",
                    Tasks = new TaskObject[]
                    {
                        new TaskObject
                        {
                            Id = "step1_router",
                            Type = "router",
                            Description = "Basic Workflow update Task update",
                            Args = new Dictionary<string, string> { { "test", "test" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        },
                        new TaskObject
                        {
                            Id = "invalid_remote_task_step2_remote_app",
                            Type = "remote_app_execution",
                            Description = "Basic remote app execution task",
                            Args = new Dictionary<string, string> { { "test", "test" } },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination()
                                {
                                    Name = "clinical_review",
                                }
                            }
                        },
                        new TaskObject
                        {
                            Id = "clinical_review",
                            Type = "aide_clinical_review",
                            Description = "Clinical Review task missing ReviewedTaskId",
                            Args = new Dictionary<string, string> {
                                { "mode", "qa" },
                                { "application_name", "Name" },
                                { "application_version", "Version" },
                                { "reviewed_task_id", "Task ID" }
                            },
                            Artifacts = new ArtifactMap()
                            {
                                Input = new Artifact[] {},
                                Output = new OutputArtifact[] {}
                            },
                            TaskDestinations = new TaskDestination[] {}
                        }
                    },
                    InformaticsGateway = new InformaticsGateway()
                    {
                        AeTitle = "Update",
                        ExportDestinations = new string[]{"test"}
                    }
                }
            },
        };
    }
}
