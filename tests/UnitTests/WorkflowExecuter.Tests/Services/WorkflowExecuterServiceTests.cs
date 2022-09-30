/*
 * Copyright 2021-2022 MONAI Consortium
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Storage.API;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Common.Extensions;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Common;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Services;
using Moq;
using Xunit;
using Monai.Deploy.WorkflowManager.Storage.Services;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Parser;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using System.Threading;

namespace Monai.Deploy.WorkflowManager.WorkflowExecuter.Tests.Services
{
    public class WorkflowExecuterServiceTests
    {
        private IWorkflowExecuterService WorkflowExecuterService { get; set; }

        private readonly Mock<IWorkflowRepository> _workflowRepository;
        private readonly Mock<IArtifactMapper> _artifactMapper;
        private readonly Mock<ILogger<WorkflowExecuterService>> _logger;
        private readonly Mock<IWorkflowInstanceRepository> _workflowInstanceRepository;
        private readonly Mock<IWorkflowInstanceService> _workflowInstanceService;
        private readonly Mock<IMessageBrokerPublisherService> _messageBrokerPublisherService;
        private readonly Mock<IStorageService> _storageService;
        private readonly Mock<IPayloadService> _payloadService;
        private readonly Mock<IWorkflowService> _workflowService;
        private readonly IOptions<WorkflowManagerOptions> _configuration;
        private readonly IOptions<StorageServiceConfiguration> _storageConfiguration;

        public WorkflowExecuterServiceTests()
        {
            _workflowRepository = new Mock<IWorkflowRepository>();
            _artifactMapper = new Mock<IArtifactMapper>();
            _logger = new Mock<ILogger<WorkflowExecuterService>>();
            _workflowInstanceRepository = new Mock<IWorkflowInstanceRepository>();
            _workflowInstanceService = new Mock<IWorkflowInstanceService>();
            _messageBrokerPublisherService = new Mock<IMessageBrokerPublisherService>();
            _storageService = new Mock<IStorageService>();
            _payloadService = new Mock<IPayloadService>();
            _workflowService = new Mock<IWorkflowService>();

            _configuration = Options.Create(new WorkflowManagerOptions() { Messaging = new MessageBrokerConfiguration { Topics = new MessageBrokerConfigurationKeys { TaskDispatchRequest = "md.task.dispatch", ExportRequestPrefix = "md.export.request" }, DicomAgents = new DicomAgentConfiguration { DicomWebAgentName = "monaidicomweb" } } });
            _storageConfiguration = Options.Create(new StorageServiceConfiguration() { Settings = new Dictionary<string, string> { { "bucket", "testbucket" }, { "endpoint", "localhost" }, { "securedConnection", "False" } } });

            var dicom = new Mock<IDicomService>();
            var logger = new Mock<ILogger<ConditionalParameterParser>>();

            var conditionalParser = new ConditionalParameterParser(logger.Object,
                                                                   dicom.Object,
                                                                   _workflowInstanceService.Object,
                                                                   _payloadService.Object,
                                                                   _workflowService.Object);

            WorkflowExecuterService = new WorkflowExecuterService(_logger.Object,
                                                                  _configuration,
                                                                  _storageConfiguration,
                                                                  _workflowRepository.Object,
                                                                  _workflowInstanceRepository.Object,
                                                                  _messageBrokerPublisherService.Object,
                                                                  conditionalParser,
                                                                  _artifactMapper.Object,
                                                                  _storageService.Object,
                                                                  _payloadService.Object);
        }

        [Fact]
        public async Task ProcessPayload_ValidAeTitleWorkflowRequest_ReturnesTrue()
        {
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                CalledAeTitle = "aetitle",
                CallingAeTitle = "aetitle",
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            var workflows = new List<WorkflowRevision>
            {
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname",
                        Description = "Workflowdesc",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetWorkflowsByAeTitleAsync(It.IsAny<List<string>>())).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflows[0].WorkflowId)).ReturnsAsync(workflows[0]);
            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Once());

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessPayload_ValidWorkflowIdRequest_ReturnesTrue()
        {
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                CalledAeTitle = "aetitle",
                CallingAeTitle = "aetitle",
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Workflows = new List<string>
                {
                    workflowId1.ToString(),
                    workflowId2.ToString()
                }
            };

            var workflows = new List<WorkflowRevision>
            {
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId1,
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname1",
                        Description = "Workflowdesc1",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                },
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId2,
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname2",
                        Description = "Workflowdesc2",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetByWorkflowsIdsAsync(new List<string> { workflowId1.ToString(), workflowId2.ToString() })).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowId1)).ReturnsAsync(workflows[0]);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowId2)).ReturnsAsync(workflows[1]);
            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _artifactMapper.Setup(a => a.ConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Dictionary<string, string>());

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(2));

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessPayload_ValidWorkflowIdRequestWithArtifacts_ReturnesTrue()
        {
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                CalledAeTitle = "aetitle",
                CallingAeTitle = "aetitle",
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Workflows = new List<string>
                {
                    workflowId1.ToString(),
                    workflowId2.ToString()
                }
            };

            var workflows = new List<WorkflowRevision>
            {
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId1,
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname1",
                        Description = "Workflowdesc1",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                },
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId2,
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname2",
                        Description = "Workflowdesc2",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc",
                                Artifacts = new ArtifactMap
                                {
                                    Output = new Artifact[]
                                    {
                                        new Artifact
                                        {
                                            Name = "output.pdf"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetByWorkflowsIdsAsync(new List<string> { workflowId1.ToString(), workflowId2.ToString() })).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowId1)).ReturnsAsync(workflows[0]);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowId2)).ReturnsAsync(workflows[1]);
            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _artifactMapper.Setup(a => a.ConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Dictionary<string, string>());

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(2));

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessPayload_RouterTaskWithMultipleDestinations_DispatchesMultiple()
        {
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                CalledAeTitle = "aetitle",
                CallingAeTitle = "aetitle",
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Workflows = new List<string>
                {
                    workflowId1.ToString()
                }
            };

            var workflows = new List<WorkflowRevision>
            {
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId1,
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname1",
                        Description = "Workflowdesc1",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "router",
                                Description = "taskdesc",
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination
                                    {
                                        Name = "task1"
                                    },
                                    new TaskDestination
                                    {
                                        Name = "task2"
                                    }
                                }
                            },
                            new TaskObject {
                                Id = "task1",
                                Type = "basictask",
                                Description = "taskdesc"
                            },
                            new TaskObject {
                                Id = "task2",
                                Type = "basictask",
                                Description = "taskdesc"
                            }
                        }
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetByWorkflowsIdsAsync(new List<string> { workflowId1.ToString() })).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowId1.ToString())).ReturnsAsync(workflows[0]);
            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(It.IsAny<string>(), It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _artifactMapper.Setup(a => a.ConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Dictionary<string, string>());

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(2));

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessPayload_WithExportTask_DispatchesExport()
        {
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                CalledAeTitle = "aetitle",
                CallingAeTitle = "aetitle",
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Workflows = new List<string>
                {
                    workflowId1.ToString()
                }
            };

            var workflows = new List<WorkflowRevision>
            {
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId1,
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname1",
                        Description = "Workflowdesc1",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle",
                            ExportDestinations = new string[] { "PROD_PACS" }
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "export",
                                Description = "taskdesc",
                                Artifacts = new ArtifactMap
                                {
                                    Input = new Artifact[] { new Artifact { Name = "dicomexport", Value = "{{ context.input }}" } }
                                },
                                TaskDestinations = Array.Empty<TaskDestination>(),
                                ExportDestinations = new ExportDestination[]
                                {
                                    new ExportDestination
                                    {
                                        Name = "PROD_PACS"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetByWorkflowsIdsAsync(new List<string> { workflowId1.ToString() })).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowId1.ToString())).ReturnsAsync(workflows[0]);
            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(It.IsAny<string>(), It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            var dcmInfo = new Dictionary<string, string>() { { "dicomexport", "/dcm" } };
            _artifactMapper.Setup(a => a.TryConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), out dcmInfo)).Returns(true);
            _storageService.Setup(w => w.ListObjectsAsync(workflowRequest.Bucket, "/dcm", true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<VirtualFileInfo>()
                {
                    new VirtualFileInfo("testfile.dcm", "/dcm/testfile.dcm", "test", ulong.MaxValue)
                });

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish($"{_configuration.Value.Messaging.Topics.ExportRequestPrefix}.{_configuration.Value.Messaging.DicomAgents.ScuAgentName}", It.IsAny<Message>()), Times.Exactly(1));

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessPayload_WithInvalidExportTask_DoesNotDispatchExport()
        {
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                CalledAeTitle = "aetitle",
                CallingAeTitle = "aetitle",
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Workflows = new List<string>
                {
                    workflowId1.ToString()
                }
            };

            var workflows = new List<WorkflowRevision>
            {
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId1,
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname1",
                        Description = "Workflowdesc1",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle",
                            ExportDestinations = new string[] { "PROD_PACS" }
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "export",
                                Description = "taskdesc",
                                Artifacts = new ArtifactMap
                                {
                                    Input = new Artifact[] { new Artifact { Name = "dicomexport", Value = "{{ context.input }}" } }
                                },
                                TaskDestinations = Array.Empty<TaskDestination>(),
                                ExportDestinations = new ExportDestination[]
                                {
                                    new ExportDestination
                                    {
                                        Name = "PROD_PINS"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetByWorkflowsIdsAsync(new List<string> { workflowId1.ToString() })).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowId1.ToString())).ReturnsAsync(workflows[0]);
            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(It.IsAny<string>(), It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _artifactMapper.Setup(a => a.ConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Dictionary<string, string>() { { "dicomexport", "/dcm" } });

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish($"{_configuration.Value.Messaging.Topics.ExportRequestPrefix}.{_configuration.Value.Messaging.DicomAgents.DicomWebAgentName}", It.IsAny<Message>()), Times.Exactly(0));
            Assert.True(result);
        }

        [Fact]
        public async Task ProcessPayload_FileNotFound_FailsWorkflow()
        {
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                CalledAeTitle = "aetitle",
                CallingAeTitle = "aetitle",
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Workflows = new List<string>
                {
                    workflowId1.ToString(),
                    workflowId2.ToString()
                }
            };

            var workflows = new List<WorkflowRevision>
            {
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId1,
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname1",
                        Description = "Workflowdesc1",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                },
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId2,
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname2",
                        Description = "Workflowdesc2",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetByWorkflowsIdsAsync(new List<string> { workflowId1.ToString(), workflowId2.ToString() })).ReturnsAsync(workflows);

            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _artifactMapper.Setup(a => a.ConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new FileNotFoundException());

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(0));

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessPayload_WorkflowAlreadyStarted_TaskNotDispatched()
        {
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var taskId = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                CalledAeTitle = "aetitle",
                CallingAeTitle = "aetitle",
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                PayloadId = Guid.NewGuid(),
                Workflows = new List<string>
                {
                    workflowId1.ToString(),
                    workflowId2.ToString()
                }
            };

            var workflows = new List<WorkflowRevision>
            {
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId1,
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname1",
                        Description = "Workflowdesc1",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                },
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId2,
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname2",
                        Description = "Workflowdesc2",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = taskId,
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                }
            };

            var workflowsInstance = new List<WorkflowInstance>
            {
                new WorkflowInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId2,
                    PayloadId = workflowRequest.PayloadId.ToString(),
                    Status = Status.Created,
                    BucketId = $"{workflowRequest.Bucket}/{workflowId2}",
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = taskId,
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetByWorkflowsIdsAsync(new List<string> { workflowId1.ToString(), workflowId2.ToString() })).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowId1)).ReturnsAsync(workflows[0]);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowId2)).ReturnsAsync(workflows[1]);
            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(workflowsInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _artifactMapper.Setup(a => a.ConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Dictionary<string, string>());

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(1));

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEvent_ReturnsTrue()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();

            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "taskid",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Succeeded,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = new Dictionary<string, object>(),
                CorrelationId = Guid.NewGuid().ToString()
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflow = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname2",
                    Description = "Workflowdesc2",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle"
                    },
                    Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = "taskid",
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "taskid",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowInstance.WorkflowId)).ReturnsAsync(workflow);
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });
            _artifactMapper.Setup(a => a.ConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Dictionary<string, string>());

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _workflowInstanceRepository.Verify(w => w.UpdateWorkflowInstanceStatusAsync(workflowInstanceId, Status.Succeeded), Times.Exactly(1));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEventWithTaskDestinations_ReturnsTrue()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();
            var taskId = Guid.NewGuid().ToString();

            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "pizza",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Succeeded,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = new Dictionary<string, object>(),
                CorrelationId = Guid.NewGuid().ToString()
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflow = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname2",
                    Description = "Workflowdesc2",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle"
                    },
                    Tasks = new TaskObject[]
                    {
                        new TaskObject {
                            Id = "pizza",
                            Type = "type",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "coffee"
                                },
                                new TaskDestination
                                {
                                    Name = "doughnuts"
                                }
                            }
                        },
                        new TaskObject {
                            Id = "coffee",
                            Type = "type",
                            Description = "taskdesc"
                        },
                        new TaskObject {
                            Id = "doughnuts",
                            Type = "type",
                            Description = "taskdesc"
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(workflowInstance.Id, It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowInstance.WorkflowId)).ReturnsAsync(workflow);
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });
            _artifactMapper.Setup(a => a.ConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Dictionary<string, string>());

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(2));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEventWithRouterTaskDestinations_ReturnsTrue()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();
            var taskId = Guid.NewGuid().ToString();

            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "pizza",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Succeeded,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = new Dictionary<string, object>(),
                CorrelationId = Guid.NewGuid().ToString()
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflow = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname2",
                    Description = "Workflowdesc2",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle"
                    },
                    Tasks = new TaskObject[]
                    {
                        new TaskObject {
                            Id = "pizza",
                            Type = "type",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "coffee"
                                },
                                new TaskDestination
                                {
                                    Name = "doughnuts"
                                }
                            }
                        },
                        new TaskObject {
                            Id = "coffee",
                            Type = "router",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "fig"
                                }
                            }
                        },
                        new TaskObject {
                            Id = "doughnuts",
                            Type = "router",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "log"
                                }
                            }
                        },
                        new TaskObject {
                            Id = "fig",
                            Type = "type",
                            Description = "taskdesc"
                        },
                        new TaskObject {
                            Id = "log",
                            Type = "type",
                            Description = "taskdesc"
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(workflowInstance.Id, It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowInstance.WorkflowId)).ReturnsAsync(workflow);
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });
            _artifactMapper.Setup(a => a.ConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Dictionary<string, string>());

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(2));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEventWithExportTaskDestination_ReturnsTrue()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();
            var taskId = Guid.NewGuid().ToString();

            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "pizza",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Succeeded,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = new Dictionary<string, object>(),
                CorrelationId = Guid.NewGuid().ToString()
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflow = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname2",
                    Description = "Workflowdesc2",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle",
                        ExportDestinations = new string[] { "PROD_PACS" }
                    },
                    Tasks = new TaskObject[]
                    {
                        new TaskObject {
                            Id = "pizza",
                            Type = "type",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "exporttaskid"
                                },
                            }
                        },
                        new TaskObject {
                            Id = "exporttaskid",
                            Type = "export",
                            Description = "taskdesc",
                            Artifacts = new ArtifactMap
                            {
                                Input = new Artifact[] { new Artifact { Name = "dicomexport", Value = "{{ context.input }}" } }
                            },
                            TaskDestinations = Array.Empty<TaskDestination>(),
                            ExportDestinations = new ExportDestination[]
                            {
                                new ExportDestination
                                {
                                    Name = "PROD_PACS"
                                }
                            }
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(workflowInstance.Id, It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowInstance.WorkflowId)).ReturnsAsync(workflow);
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });
            var expectedDcmValue = new Dictionary<string, string> { { "dicomexport", "/dcm" } };
            _artifactMapper.Setup(a => a.TryConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), out expectedDcmValue)).Returns(true);
            _storageService.Setup(w => w.ListObjectsAsync(It.IsAny<string>(), "/dcm", true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<VirtualFileInfo>()
                {
                    new VirtualFileInfo("testfile.dcm", "/dcm/testfile.dcm", "test", ulong.MaxValue)
                });

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish($"{_configuration.Value.Messaging.Topics.ExportRequestPrefix}.{_configuration.Value.Messaging.DicomAgents.ScuAgentName}", It.IsAny<Message>()), Times.Exactly(1));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEventWithExportTaskDestination_NoExportDestinations_DoesNotDispatchExport()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();
            var taskId = Guid.NewGuid().ToString();

            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "pizza",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Succeeded,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = new Dictionary<string, object>(),
                CorrelationId = Guid.NewGuid().ToString()
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflow = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname2",
                    Description = "Workflowdesc2",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle"
                    },
                    Tasks = new TaskObject[]
                    {
                        new TaskObject {
                            Id = "pizza",
                            Type = "type",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "exporttaskid"
                                },
                            }
                        },
                        new TaskObject {
                            Id = "exporttaskid",
                            Type = "export",
                            Description = "taskdesc",
                            Artifacts = new ArtifactMap
                            {
                                Input = new Artifact[] { new Artifact { Name = "dicomexport", Value = "{{ context.input }}" } }
                            },
                            TaskDestinations = Array.Empty<TaskDestination>()
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(workflowInstance.Id, It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowInstance.WorkflowId)).ReturnsAsync(workflow);
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });
            _artifactMapper.Setup(a => a.ConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Dictionary<string, string> { { "dicomexport", "/dcm" } });

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish($"{_configuration.Value.Messaging.Topics.ExportRequestPrefix}.{_configuration.Value.Messaging.DicomAgents.DicomWebAgentName}", It.IsAny<Message>()), Times.Exactly(0));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEventNonSuccessStatus_ReturnsTrue()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();

            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "pizza",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Accepted,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = new Dictionary<string, object>(),
                CorrelationId = Guid.NewGuid().ToString()
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflow = new WorkflowRevision
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname2",
                    Description = "Workflowdesc2",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle"
                    },
                    Tasks = new TaskObject[]
                    {
                        new TaskObject {
                            Id = "pizza",
                            Type = "type",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "coffee"
                                },
                                new TaskDestination
                                {
                                    Name = "doughnuts"
                                }
                            }
                        },
                        new TaskObject {
                            Id = "coffee",
                            Type = "type",
                            Description = "taskdesc"
                        },
                        new TaskObject {
                            Id = "doughnuts",
                            Type = "type",
                            Description = "taskdesc"
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowInstance.WorkflowId)).ReturnsAsync(workflow);
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(0));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidAcceptedTaskUpdateEventDoesNotDispatchTasks_ReturnsTrue()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();

            var metadata = new Dictionary<string, object>();
            metadata.Add("a", "b");
            metadata.Add("c", "d");
            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "pizza",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Accepted,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = metadata,
                CorrelationId = Guid.NewGuid().ToString()
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflow = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname2",
                    Description = "Workflowdesc2",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle"
                    },
                    Tasks = new TaskObject[]
                    {
                        new TaskObject {
                            Id = "pizza",
                            Type = "type",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "coffee"
                                },
                                new TaskDestination
                                {
                                    Name = "doughnuts"
                                }
                            }
                        },
                        new TaskObject {
                            Id = "coffee",
                            Type = "type",
                            Description = "taskdesc"
                        },
                        new TaskObject {
                            Id = "doughnuts",
                            Type = "type",
                            Description = "taskdesc"
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Dispatched
                        },
                        new TaskExecution
                        {
                            TaskId = "coffee",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(workflowInstance.Id, It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowInstance.WorkflowId)).ReturnsAsync(workflow);
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(0));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEventOneTaskDestinationDispatched_ReturnsTrue()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();

            var metadata = new Dictionary<string, object>();
            metadata.Add("a", "b");
            metadata.Add("c", "d");
            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "pizza",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Succeeded,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = metadata,
                CorrelationId = Guid.NewGuid().ToString()
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflow = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname2",
                    Description = "Workflowdesc2",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle"
                    },
                    Tasks = new TaskObject[]
                    {
                        new TaskObject {
                            Id = "pizza",
                            Type = "type",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "coffee"
                                },
                                new TaskDestination
                                {
                                    Name = "doughnuts"
                                }
                            }
                        },
                        new TaskObject {
                            Id = "coffee",
                            Type = "type",
                            Description = "taskdesc"
                        },
                        new TaskObject {
                            Id = "doughnuts",
                            Type = "type",
                            Description = "taskdesc"
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Dispatched
                        },
                        new TaskExecution
                        {
                            TaskId = "coffee",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(workflowInstance.Id, It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowInstance.WorkflowId)).ReturnsAsync(workflow);
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(1));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEventWithOutputArtifacts_ReturnsTrue()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();
            const string taskId = "pizza";

            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = taskId,
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Succeeded,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = new Dictionary<string, object>(),
                CorrelationId = Guid.NewGuid().ToString(),
                Outputs = new List<Messaging.Common.Storage>
                {
                    new Messaging.Common.Storage
                    {
                        Name = "artifactname",
                        RelativeRootPath = "path/to/artifact"
                    }
                }
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflow = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname2",
                    Description = "Workflowdesc2",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle"
                    },
                    Tasks = new TaskObject[]
                    {
                        new TaskObject {
                            Id = taskId,
                            Type = "type",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "coffee"
                                },
                                new TaskDestination
                                {
                                    Name = "doughnuts"
                                }
                            }
                        },
                        new TaskObject {
                            Id = "coffee",
                            Type = "type",
                            Description = "taskdesc"
                        },
                        new TaskObject {
                            Id = "doughnuts",
                            Type = "type",
                            Description = "taskdesc"
                        }
                    }
                }
            };

            var pizzaTask = new TaskExecution
            {
                TaskId = "pizza",
                Status = TaskExecutionStatus.Dispatched,
                OutputArtifacts = new Dictionary<string, string>
                {
                    { "k", "v" }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                {
                    pizzaTask,
                    new TaskExecution
                    {
                        TaskId = "coffee",
                        Status = TaskExecutionStatus.Dispatched
                    }
                }
            };

            var artifactDict = updateEvent.Outputs.ToArtifactDictionary();

            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(workflowInstance.Id, It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.UpdateTaskOutputArtifactsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(true);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowInstance.WorkflowId)).ReturnsAsync(workflow);
            _storageService.Setup(w => w.VerifyObjectsExistAsync(workflowInstance.BucketId, artifactDict)).ReturnsAsync(new Dictionary<string, string> { { "New_Key", "New_Value" } });
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(1));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEventWithOutputArtifactsMissing_ReturnsFalse()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();

            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "pizza",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Succeeded,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = new Dictionary<string, object>(),
                CorrelationId = Guid.NewGuid().ToString(),
                Outputs = new List<Messaging.Common.Storage>
                {
                    new Messaging.Common.Storage
                    {
                        Name = "artifactname",
                        RelativeRootPath = "path/to/artifact"
                    }
                }
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflow = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname2",
                    Description = "Workflowdesc2",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle"
                    },
                    Tasks = new TaskObject[]
                    {
                        new TaskObject {
                            Id = "pizza",
                            Type = "type",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "coffee"
                                },
                                new TaskDestination
                                {
                                    Name = "doughnuts"
                                }
                            },
                            Artifacts = new ArtifactMap
                            {
                                Output = new Artifact[]
                                {
                                    new Artifact
                                    {
                                        Name = "Artifact Name",
                                        Value = "Artifact Value",
                                        Mandatory = true
                                    }
                                }
                            }
                        },
                        new TaskObject {
                            Id = "coffee",
                            Type = "type",
                            Description = "taskdesc"
                        },
                        new TaskObject {
                            Id = "doughnuts",
                            Type = "type",
                            Description = "taskdesc"
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Dispatched
                        },
                        new TaskExecution
                        {
                            TaskId = "coffee",
                            Status = TaskExecutionStatus.Dispatched
                        }
                    }
            };

            var artifactDict = updateEvent.Outputs.ToArtifactDictionary();

            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(workflowInstance.Id, It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.UpdateTaskOutputArtifactsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(true);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowInstance.WorkflowId)).ReturnsAsync(workflow);
            _storageService.Setup(w => w.VerifyObjectsExistAsync(workflowInstance.BucketId, artifactDict)).ReturnsAsync(new Dictionary<string, string> { { "New_Key", "New_Value" } });
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(0));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEventWorkflowDoesNotExist_ReturnsTrue()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();

            var metadata = new Dictionary<string, object>();
            metadata.Add("a", "b");
            metadata.Add("c", "d");

            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "pizza",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Succeeded,
                Reason = FailureReason.None,
                Message = "This is a message",
                Metadata = metadata,
                CorrelationId = Guid.NewGuid().ToString()
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflow = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname2",
                    Description = "Workflowdesc2",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle"
                    },
                    Tasks = new TaskObject[]
                    {
                        new TaskObject {
                            Id = "pizza",
                            Type = "type",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "coffee"
                                },
                                new TaskDestination
                                {
                                    Name = "doughnuts"
                                }
                            }
                        },
                        new TaskObject {
                            Id = "doughnuts",
                            Type = "type",
                            Description = "taskdesc"
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Dispatched
                        },
                        new TaskExecution
                        {
                            TaskId = "coffee",
                            Status = TaskExecutionStatus.Created
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(workflowInstance.Id, It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowInstance.WorkflowId)).ReturnsAsync(workflow);
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(1));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessExportComplete_ValidExportCompleteEventMultipleTaskDestinationsDispatched_ReturnsTrue()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();
            var correlationId = Guid.NewGuid().ToString();

            var exportEvent = new ExportCompleteEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                ExportTaskId = "pizza",
                Status = ExportStatus.Success,
                Message = "This is a message"
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflow = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname2",
                    Description = "Workflowdesc2",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle"
                    },
                    Tasks = new TaskObject[]
                    {
                        new TaskObject {
                            Id = "pizza",
                            Type = "type",
                            Description = "taskdesc",
                            TaskDestinations = new TaskDestination[]
                            {
                                new TaskDestination
                                {
                                    Name = "coffee"
                                },
                                new TaskDestination
                                {
                                    Name = "doughnuts"
                                }
                            }
                        },
                        new TaskObject {
                            Id = "coffee",
                            Type = "type",
                            Description = "taskdesc"
                        },
                        new TaskObject {
                            Id = "doughnuts",
                            Type = "type",
                            Description = "taskdesc"
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Created
                        },
                        new TaskExecution
                        {
                            TaskId = "coffee",
                            Status = TaskExecutionStatus.Created
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(workflowInstance.Id, It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowInstance.WorkflowId)).ReturnsAsync(workflow);
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });

            var response = await WorkflowExecuterService.ProcessExportComplete(exportEvent, correlationId);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(2));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessExportComplete_ValidExportCompleteEventFailedDoesNotDispatchTasks_ReturnsTrue()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();
            var correlationId = Guid.NewGuid().ToString();

            var exportEvent = new ExportCompleteEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                ExportTaskId = "pizza",
                Status = ExportStatus.Failure,
                Message = "This is a message"
            };

            var workflowId = Guid.NewGuid().ToString();

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Created
                        },
                        new TaskExecution
                        {
                            TaskId = "coffee",
                            Status = TaskExecutionStatus.Created
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(workflowInstance.Id, It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload { PatientDetails = new PatientDetails { } });

            var response = await WorkflowExecuterService.ProcessExportComplete(exportEvent, correlationId);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(0));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task CreateTaskExecutionAsync_ValidWorkflowInstanceAndTask_ShouldCreateTaskExecution()
        {
            const int expectedTaskTimeoutLength = 10;
            const string bucket = "bucket";
            var expectedTaskTimeoutLengthDT = DateTime.UtcNow.AddMinutes(expectedTaskTimeoutLength);
            var workflowId = Guid.NewGuid().ToString();
            var payloadId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();

            var pizzaTask = new TaskObject
            {
                Id = "pizza",
                Type = "type",
                Description = "taskdesc",
                TimeoutMinutes = expectedTaskTimeoutLength,
                TaskDestinations = new TaskDestination[]
                {
                    new TaskDestination
                    {
                        Name = "coffee"
                    },
                    new TaskDestination
                    {
                        Name = "doughnuts"
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
            };

            var newDict = new Dictionary<string, string>();
            _artifactMapper.Setup(a => a.TryConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), out newDict)).Returns(true);

            var newPizza = await WorkflowExecuterService.CreateTaskExecutionAsync(pizzaTask, workflowInstance, bucket, payloadId);

            newPizza.Should().NotBeNull();
            Assert.Equal(pizzaTask.Id, newPizza.TaskId);
            Assert.Equal(pizzaTask.Type, newPizza.TaskType);
            Assert.Equal(workflowInstance.Id, newPizza.WorkflowInstanceId);
            Assert.Equal(TaskExecutionStatus.Created, newPizza.Status);
            Assert.Equal(FailureReason.None, newPizza.Reason);
            Assert.Equal(expectedTaskTimeoutLength, newPizza.TimeoutInterval);
            newPizza.Timeout.Should().BeCloseTo(expectedTaskTimeoutLengthDT, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task CreateTaskExecutionAsync_InvalidArtifactInWorkflowInstanceAndTask_ShouldCreateTaskExecutionWithFailedStatus()
        {
            const int expectedTaskTimeoutLength = 10;
            const string bucket = "bucket";
            var expectedTaskTimeoutLengthDT = DateTime.UtcNow.AddMinutes(expectedTaskTimeoutLength);
            var workflowId = Guid.NewGuid().ToString();
            var payloadId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();

            var pizzaTask = new TaskObject
            {
                Id = "pizza",
                Type = "type",
                Description = "taskdesc",
                TimeoutMinutes = expectedTaskTimeoutLength,
                Artifacts = new ArtifactMap
                {
                    Input = new Artifact[]
                    {
                        new Artifact
                        {
                            Name = "arti1",
                            Value = "arti1Value"
                        }
                    }
                },
                TaskDestinations = new TaskDestination[]
                {
                    new TaskDestination
                    {
                        Name = "coffee"
                    },
                    new TaskDestination
                    {
                        Name = "doughnuts"
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
            };

            var expectedDict = new Dictionary<string, string>();
            _artifactMapper.Setup(a => a.TryConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), out expectedDict)).Returns(false);

            var newPizza = await WorkflowExecuterService.CreateTaskExecutionAsync(pizzaTask, workflowInstance, bucket, payloadId);

            newPizza.Should().NotBeNull();
            Assert.Equal(pizzaTask.Id, newPizza.TaskId);
            Assert.Equal(pizzaTask.Type, newPizza.TaskType);
            Assert.Equal(workflowInstance.Id, newPizza.WorkflowInstanceId);
            Assert.Equal(TaskExecutionStatus.Failed, newPizza.Status);
            Assert.Equal(FailureReason.ExternalServiceError, newPizza.Reason);
            Assert.Equal(expectedTaskTimeoutLength, newPizza.TimeoutInterval);
            newPizza.Timeout.Should().BeCloseTo(expectedTaskTimeoutLengthDT, TimeSpan.FromSeconds(2));
            Assert.Equal(expectedDict, newPizza.InputArtifacts);
        }

        [Fact]
        public async Task CreateTaskExecutionAsync_ValidArtifactInWorkflowInstanceAndTask_ShouldCreateTaskExecutionWithCreatedStatus()
        {
            const int expectedTaskTimeoutLength = 10;
            const string bucket = "bucket";
            var expectedTaskTimeoutLengthDT = DateTime.UtcNow.AddMinutes(expectedTaskTimeoutLength);
            var workflowId = Guid.NewGuid().ToString();
            var payloadId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();

            var pizzaTask = new TaskObject
            {
                Id = "pizza",
                Type = "type",
                Description = "taskdesc",
                TimeoutMinutes = expectedTaskTimeoutLength,
                Artifacts = new ArtifactMap
                {
                    Input = new Artifact[]
                    {
                        new Artifact
                        {
                            Name = "arti1",
                            Value = "arti1Value"
                        }
                    }
                },
                TaskDestinations = new TaskDestination[]
                {
                    new TaskDestination
                    {
                        Name = "coffee"
                    },
                    new TaskDestination
                    {
                        Name = "doughnuts"
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
            };

            var expectedDict = new Dictionary<string, string>()
            {
                { "k", "v" }
            };
            _artifactMapper.Setup(a => a.TryConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), out expectedDict)).Returns(true);

            var newPizza = await WorkflowExecuterService.CreateTaskExecutionAsync(pizzaTask, workflowInstance, bucket, payloadId);

            newPizza.Should().NotBeNull();
            Assert.Equal(pizzaTask.Id, newPizza.TaskId);
            Assert.Equal(pizzaTask.Type, newPizza.TaskType);
            Assert.Equal(workflowInstance.Id, newPizza.WorkflowInstanceId);
            Assert.Equal(TaskExecutionStatus.Created, newPizza.Status);
            Assert.Equal(FailureReason.None, newPizza.Reason);
            Assert.Equal(expectedTaskTimeoutLength, newPizza.TimeoutInterval);
            newPizza.Timeout.Should().BeCloseTo(expectedTaskTimeoutLengthDT, TimeSpan.FromSeconds(2));
            Assert.Equal(expectedDict, newPizza.InputArtifacts);
        }
    }
}
