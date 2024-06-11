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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.API;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Services;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Message = Monai.Deploy.Messaging.Messages.Message;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Common;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Database;
using Monai.Deploy.WorkflowManager.Common.Storage.Services;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Extensions;
using Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Parser;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkloadManager.WorkflowExecuter.Extensions;
using Monai.Deploy.WorkflowManager.Common.Database.Repositories;
using Artifact = Monai.Deploy.WorkflowManager.Common.Contracts.Models.Artifact;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Tests.Services
{
    public class WorkflowExecuterServiceTests
    {
        private IWorkflowExecuterService WorkflowExecuterService { get; set; }

        private readonly Mock<IWorkflowRepository> _workflowRepository;
        private readonly Mock<IArtifactMapper> _artifactMapper;
        private readonly Mock<ILogger<WorkflowExecuterService>> _logger;
        private readonly Mock<IWorkflowInstanceRepository> _workflowInstanceRepository;
        private readonly Mock<IWorkflowInstanceService> _workflowInstanceService;
        private readonly Mock<IArtifactsRepository> _artifactReceivedRepository;
        private readonly Mock<IMessageBrokerPublisherService> _messageBrokerPublisherService;
        private readonly Mock<IStorageService> _storageService;
        private readonly Mock<IPayloadService> _payloadService;
        private readonly Mock<IWorkflowService> _workflowService;
        private readonly IOptions<WorkflowManagerOptions> _configuration;
        private readonly IOptions<StorageServiceConfiguration> _storageConfiguration;
        private readonly Mock<ITaskExecutionStatsRepository> _taskExecutionStatsRepository;
        private readonly Mock<IDicomService> _dicom = new Mock<IDicomService>();
        private readonly int _timeoutForTypeTask = 999;
        private readonly int _timeoutForDefault = 966;

        public WorkflowExecuterServiceTests()
        {
            _workflowRepository = new Mock<IWorkflowRepository>();
            _artifactReceivedRepository = new Mock<IArtifactsRepository>();
            _artifactMapper = new Mock<IArtifactMapper>();
            _logger = new Mock<ILogger<WorkflowExecuterService>>();
            _workflowInstanceRepository = new Mock<IWorkflowInstanceRepository>();
            _workflowInstanceService = new Mock<IWorkflowInstanceService>();
            _messageBrokerPublisherService = new Mock<IMessageBrokerPublisherService>();
            _taskExecutionStatsRepository = new Mock<ITaskExecutionStatsRepository>();
            _storageService = new Mock<IStorageService>();
            _payloadService = new Mock<IPayloadService>();
            _workflowService = new Mock<IWorkflowService>();

            _configuration = Options.Create(new WorkflowManagerOptions()
            {
                TaskTimeoutMinutes = _timeoutForDefault,
                PerTaskTypeTimeoutMinutes = new Dictionary<string, double> { { "taskType", _timeoutForTypeTask } },
                Messaging = new MessageBrokerConfiguration
                {
                    Topics = new MessageBrokerConfigurationKeys { TaskDispatchRequest = "md.task.dispatch", ExportRequestPrefix = "md.export.request", ExportHL7 = "md.export.hl7" },
                    DicomAgents = new DicomAgentConfiguration { DicomWebAgentName = "monaidicomweb" }
                },
                MigExternalAppPlugins = new List<string> { { "examplePlugin" } }.ToArray()
            });

            _storageConfiguration = Options.Create(new StorageServiceConfiguration() { Settings = new Dictionary<string, string> { { "bucket", "testbucket" }, { "endpoint", "localhost" }, { "securedConnection", "False" } } });

            var logger = new Mock<ILogger<ConditionalParameterParser>>();

            var conditionalParser = new ConditionalParameterParser(logger.Object,
                                                                   _dicom.Object,
                                                                   _workflowInstanceService.Object,
                                                                   _payloadService.Object,
                                                                   _workflowService.Object);

            WorkflowExecuterService = new WorkflowExecuterService(_logger.Object,
                                                                  _configuration,
                                                                  _storageConfiguration,
                                                                  _workflowRepository.Object,
                                                                  _workflowInstanceRepository.Object,
                                                                  _messageBrokerPublisherService.Object,
                                                                  _workflowInstanceService.Object,
                                                                  conditionalParser,
                                                                  _taskExecutionStatsRepository.Object,
                                                                  _artifactMapper.Object,
                                                                  _storageService.Object,
                                                                  _payloadService.Object,
                                                                  _artifactReceivedRepository.Object);
        }

        [Fact]
        public void WorkflowExecuterService_Throw_If_No_Config()
        {
            var dicom = new Mock<IDicomService>();
            var logger = new Mock<ILogger<ConditionalParameterParser>>();

            var conditionalParser = new ConditionalParameterParser(logger.Object,
                                                       dicom.Object,
                                                       _workflowInstanceService.Object,
                                                       _payloadService.Object,
                                                       _workflowService.Object);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => new WorkflowExecuterService(_logger.Object,
                                                                  null,
                                                                  _storageConfiguration,
                                                                  _workflowRepository.Object,
                                                                  _workflowInstanceRepository.Object,
                                                                  _messageBrokerPublisherService.Object,
                                                                  _workflowInstanceService.Object,
                                                                  conditionalParser,
                                                                  _taskExecutionStatsRepository.Object,
                                                                  _artifactMapper.Object,
                                                                  _storageService.Object,
                                                                  _payloadService.Object,
                                                                  _artifactReceivedRepository.Object));

        }

        [Fact]
        public void WorkflowExecuterService_Throw_If_No_Storage_Config()
        {
            var dicom = new Mock<IDicomService>();
            var logger = new Mock<ILogger<ConditionalParameterParser>>();

            var conditionalParser = new ConditionalParameterParser(logger.Object,
                                                       dicom.Object,
                                                       _workflowInstanceService.Object,
                                                       _payloadService.Object,
                                                       _workflowService.Object);

            Assert.Throws<ArgumentNullException>(() => new WorkflowExecuterService(_logger.Object,
                                                                  _configuration,
                                                                  null,
                                                                  _workflowRepository.Object,
                                                                  _workflowInstanceRepository.Object,
                                                                  _messageBrokerPublisherService.Object,
                                                                  _workflowInstanceService.Object,
                                                                  conditionalParser,
                                                                  _taskExecutionStatsRepository.Object,
                                                                  _artifactMapper.Object,
                                                                  _storageService.Object,
                                                                  _payloadService.Object,
                                                                  _artifactReceivedRepository.Object));
        }

        [Fact]
        public async Task ProcessArtifactReceived_WhenMessageIsNull_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => WorkflowExecuterService.ProcessArtifactReceivedAsync(null));
        }

        [Fact]
        public async Task ProcessArtifactReceived_WhenWorkflowInstanceIdIsNull_ReturnsFalse()
        {
            var message = new ArtifactsReceivedEvent { };
            var result = await WorkflowExecuterService.ProcessArtifactReceivedAsync(message);
            Assert.False(result);
        }

        [Fact]
        public async Task ProcessArtifactReceived_WhenTaskIdIsNull_ReturnsFalse()
        {
            var message = new ArtifactsReceivedEvent { WorkflowInstanceId = "123" };
            var result = await WorkflowExecuterService.ProcessArtifactReceivedAsync(message);
            Assert.False(result);
        }

        [Fact]
        public async Task ProcessArtifactReceived_WhenWorkflowInstanceRepositoryReturnsNull_ReturnsFalse()
        {
            var message = new ArtifactsReceivedEvent { WorkflowInstanceId = "123", TaskId = "456" };
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(message.WorkflowInstanceId))!
                .ReturnsAsync((WorkflowInstance)null!);
            var result = await WorkflowExecuterService.ProcessArtifactReceivedAsync(message);
            Assert.False(result);
        }

        [Fact]
        public async Task ProcessArtifactReceived_WhenWorkflowRepositoryReturnsNull_ReturnsFalse()
        {
            var message = new ArtifactsReceivedEvent { WorkflowInstanceId = "123", TaskId = "456" };
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(message.WorkflowInstanceId))!
                .ReturnsAsync(new WorkflowInstance { WorkflowId = "789" });
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync("789"))!
                .ReturnsAsync((WorkflowRevision)null!);
            var result = await WorkflowExecuterService.ProcessArtifactReceivedAsync(message);
            Assert.False(result);
        }

        // ProcessArtifactReceived workflowTemplate.Workflow?.Tasks.FirstOrDefault returns null
        [Fact]
        public async Task ProcessArtifactReceived_WhenWorkflowTemplateReturnsNull_ReturnsFalse()
        {
            var message = new ArtifactsReceivedEvent { WorkflowInstanceId = "123", TaskId = "456" };
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(message.WorkflowInstanceId))!
                .ReturnsAsync(new WorkflowInstance { WorkflowId = "789" });
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync("789"))!
                .ReturnsAsync(new WorkflowRevision
                {
                    Workflow = new Workflow
                    {
                        Tasks = [new TaskObject() { Id = "not456" }]
                    }
                });
            var result = await WorkflowExecuterService.ProcessArtifactReceivedAsync(message);
            Assert.False(result);
        }

        [Fact]
        public async Task ProcessArtifactReceived_WhenStillHasMissingArtifacts_ReturnsTrue()
        {
            var message = new ArtifactsReceivedEvent
            {
                WorkflowInstanceId = "123", TaskId = "456",
                Artifacts = new List<Messaging.Common.Artifact>() { new Messaging.Common.Artifact() { Type = ArtifactType.CT } }
            };
            var workflowInstance = new WorkflowInstance
            {
                WorkflowId = "789", Tasks = new List<TaskExecution>()
                { new TaskExecution() { TaskId = "456" } }
            };
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(message.WorkflowInstanceId))!
                .ReturnsAsync(workflowInstance);
            var templateArtifacts = new OutputArtifact[] { new OutputArtifact() { Type = ArtifactType.CT }, new OutputArtifact() { Type = ArtifactType.DG } };
            var taskTemplate = new TaskObject() { Id = "456", Artifacts = new ArtifactMap { Output = templateArtifacts } };
            var workflowTemplate = new WorkflowRevision { Workflow = new Workflow { Tasks = new[] { taskTemplate } } };
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync("789"))!
                .ReturnsAsync(workflowTemplate);
            _artifactReceivedRepository.Setup(r => r.GetAllAsync(workflowInstance.WorkflowId, taskTemplate.Id))
                .ReturnsAsync((List<ArtifactReceivedItems>?)null);

            var result = await WorkflowExecuterService.ProcessArtifactReceivedAsync(message);
            Assert.True(result);
        }

        [Fact]
        public async Task ProcessArtifactReceived_WhenAllArtifactsReceivedArtifactsButTaskExecNotFound_ReturnsFalse()
        {
            //incoming artifacts
            var message = new ArtifactsReceivedEvent
            {
                WorkflowInstanceId = "123", TaskId = "456",
                Artifacts = new List<Messaging.Common.Artifact>() { new Messaging.Common.Artifact() { Type = ArtifactType.CT } }
            };
            var workflowInstance = new WorkflowInstance
            {
                WorkflowId = "789", Tasks = new List<TaskExecution>()
                { new TaskExecution() { TaskId = "not456" } }
            };
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(message.WorkflowInstanceId))!
                .ReturnsAsync(workflowInstance);
            //expected artifacts
            var templateArtifacts = new OutputArtifact[]
            {
                new OutputArtifact() { Type = ArtifactType.CT },
            };
            var taskTemplate = new TaskObject() { Id = "456", Artifacts = new ArtifactMap { Output = templateArtifacts } };
            var workflowTemplate = new WorkflowRevision { Workflow = new Workflow { Tasks = new[] { taskTemplate } } };
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync("789"))!
                .ReturnsAsync(workflowTemplate);

            //previously received artifacts
            _artifactReceivedRepository.Setup(r => r.GetAllAsync(workflowInstance.WorkflowId, taskTemplate.Id))
                .ReturnsAsync((List<ArtifactReceivedItems>?)null);

            var result = await WorkflowExecuterService.ProcessArtifactReceivedAsync(message);
            Assert.False(result);
        }

        [Fact]
        public async Task ProcessPayload_WhenWorkflowInstanceAndTaskIdHaveAValue_ReturnsFalse()
        {
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                WorkflowInstanceId = "123",
                TaskId = "345"
            };

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            Assert.False(result);
        }

        [Fact]
        public async Task ProcessPayload_ValidAeTitleWorkflowRequest_ReturnesTrue()
        {
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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
            _workflowRepository.Setup(w => w.GetWorkflowsForWorkflowRequestAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(workflows);
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
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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
                                    Output = new OutputArtifact[]
                                    {
                                        new OutputArtifact
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
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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

            _payloadService.Setup(p => p.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Payload() { });
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
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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

            Message? messageSent = null;
            _messageBrokerPublisherService.Setup(m => m.Publish(It.IsAny<string>(), It.IsAny<Message>()))
                .Callback((string topic, Message m) => messageSent = m);

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish($"{_configuration.Value.Messaging.Topics.ExportRequestPrefix}.{_configuration.Value.Messaging.DicomAgents.ScuAgentName}", It.IsAny<Message>()), Times.Exactly(1));

            Assert.True(result);
            Assert.NotNull(messageSent);
#pragma warning disable CS8604 // Possible null reference argument.
            var body = Encoding.UTF8.GetString(messageSent?.Body);
            var exportMessageBody = JsonConvert.DeserializeObject<ExportRequestEvent>(body);
            Assert.Empty(exportMessageBody!.PluginAssemblies);

            var exportEventMessage = messageSent.ConvertTo<ExportRequestEvent>();
            Assert.NotNull(exportEventMessage.Target);
            Assert.Equal(DataService.DIMSE, exportEventMessage.Target.DataService);

#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Fact]
        public async Task ProcessPayload_WithInvalidExportTask_DoesNotDispatchExport()
        {
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var workflowsInstance = new List<WorkflowInstance>
            {
                new WorkflowInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId2,
                    WorkflowName = workflows[1].Workflow.Name,
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
#pragma warning restore CS8602 // Dereference of a possibly null reference.

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
                WorkflowName = workflow.Workflow.Name,
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
                WorkflowName = workflow.Workflow.Name,
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
                WorkflowName = workflow.Workflow.Name,
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
                WorkflowName = workflow.Workflow.Name,
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
                WorkflowName = workflow.Workflow.Name,
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
                WorkflowName = workflow.Workflow.Name,
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
                WorkflowName = workflow.Workflow.Name,
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
                WorkflowName = workflow.Workflow.Name,
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
                WorkflowName = workflow.Workflow.Name,
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
            _storageService.Setup(w => w.VerifyObjectsExistAsync(workflowInstance.BucketId, artifactDict.Select(a => a.Value).ToList(), default)).ReturnsAsync(new Dictionary<string, bool>() { { "New_Value", true } });
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
                                Output = new OutputArtifact[]
                                {
                                    new OutputArtifact
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
                WorkflowName = workflow.Workflow.Name,
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
            var list = artifactDict.Select(a => a.Value).ToList();
            _storageService.Setup(w => w.VerifyObjectsExistAsync(workflowInstance.BucketId, list, default)).ReturnsAsync(new Dictionary<string, bool>() { { "New_Value", true } });
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
                WorkflowName = workflow.Workflow.Name,
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
        public async Task ProcessTaskUpdate_Timout_Sends_Message_To_TaskTimeoutRoutingKey()
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
                Status = TaskExecutionStatus.Failed,
                Reason = FailureReason.TimedOut,
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
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                WorkflowName = workflow.Workflow.Name,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Failed
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);
            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.AideClinicalReviewCancelation, It.IsAny<Message>()), Times.Exactly(1));
        }

        [Fact]
        public async Task ProcessExportComplete_ValidExportCompleteEventMultipleTaskDestinationsDispatched_ReturnsTrue()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();
            var correlationId = Guid.NewGuid().ToString();

            var exportEvent = new ExportCompleteEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                ExportTaskId = "pizza1",
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
                            Id = "pizza1",
                            Type = ValidationConstants.ExportTaskType,
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
                            Description = "taskdesc",
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                WorkflowName = workflow.Workflow.Name,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza1",
                            Status = TaskExecutionStatus.Created,
                            TaskType=ValidationConstants.ExportTaskType
                        }
                        //,
                        //new TaskExecution
                        //{
                        //    TaskId = "coffee",
                        //    Status = TaskExecutionStatus.Created
                        //}
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

        [Fact]
        public void AttachPatientMetaData_AtachesDataToTaskExec_TaskExecShouldHavePatientData()
        {
            var taskExec = new TaskExecution
            {
                TaskId = Guid.NewGuid().ToString(),
            };

            var patientDetails = new PatientDetails()
            {
                PatientAge = "39",
                PatientDob = DateTime.Now,
                PatientHospitalId = Guid.NewGuid().ToString(),
                PatientId = Guid.NewGuid().ToString(),
                PatientName = "Daphne the alpaca",
                PatientSex = "Unknown",
            };

            taskExec.AttachPatientMetaData(patientDetails, null);

            taskExec.TaskPluginArguments.Should().NotBeNull();
            taskExec.TaskPluginArguments[PatientKeys.PatientId].Should().BeSameAs(patientDetails.PatientId);
            taskExec.TaskPluginArguments[PatientKeys.PatientAge].Should().BeSameAs(patientDetails.PatientAge);
            taskExec.TaskPluginArguments[PatientKeys.PatientSex].Should().BeSameAs(patientDetails.PatientSex);
            taskExec.TaskPluginArguments[PatientKeys.PatientDob].Should().NotBeNull();
            taskExec.TaskPluginArguments[PatientKeys.PatientHospitalId].Should().BeSameAs(patientDetails.PatientHospitalId);
            taskExec.TaskPluginArguments[PatientKeys.PatientName].Should().BeSameAs(patientDetails.PatientName);
        }

        [Fact]
        public async Task TaskExecShouldHaveCorrectTimeout()
        {
            var workflowId = Guid.NewGuid().ToString();
            var payloadId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();

            var pizzaTask = new TaskObject
            {
                Id = "pizza",
                Type = "taskType",
                Description = "taskdesc",
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
            };
            var bucket = "bucket";

            var newPizza = await WorkflowExecuterService.CreateTaskExecutionAsync(pizzaTask, workflowInstance, bucket, payloadId);
            Assert.Equal(_timeoutForTypeTask, newPizza.TimeoutInterval);

        }

        [Fact]
        public async Task TaskExecShouldPickUpConfiguredDefaultTimeout()
        {
            var workflowId = Guid.NewGuid().ToString();
            var payloadId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();

            var pizzaTask = new TaskObject
            {
                Id = "pizza",
                Type = "someothertype",
                Description = "taskdesc",
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
            };
            var bucket = "bucket";

            var newPizza = await WorkflowExecuterService.CreateTaskExecutionAsync(pizzaTask, workflowInstance, bucket, payloadId);
            Assert.Equal(_timeoutForDefault, newPizza.TimeoutInterval);

        }

        [Fact]
        public async Task ProcessPayload_WithExternalAppTask_Dispatches()
        {
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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
                                Type = "remote_app_execution",
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
            Message? messageSent = null;
            _messageBrokerPublisherService.Setup(m => m.Publish(It.IsAny<string>(), It.IsAny<Message>()))
                .Callback((string topic, Message m) => messageSent = m);

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish($"{_configuration.Value.Messaging.Topics.ExternalAppRequest}", It.IsAny<Message>()), Times.Exactly(1));

            Assert.True(result);
            Assert.NotNull(messageSent);
#pragma warning disable CS8604 // Possible null reference argument.
            var body = Encoding.UTF8.GetString(messageSent?.Body);
            var exportMessageBody = JsonConvert.DeserializeObject<ExportRequestEvent>(body);
            Assert.NotEmpty(exportMessageBody!.PluginAssemblies);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Fact]
        public async Task ProcessPayload_WithExportComplete_Resumes()
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
                            Type = ValidationConstants.ExportTaskType,
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
                            Description = "taskdesc",
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                WorkflowName = workflow.Workflow.Name,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Created,
                            TaskType = ValidationConstants.ExportTaskType
                        }
                        //,
                        //new TaskExecution
                        //{
                        //    TaskId = "coffee",
                        //    Status = TaskExecutionStatus.Created
                        //}
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
        public async Task ProcessPayload_WithExternalAppComplete_Pauses()
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
                            Type = ValidationConstants.ExportTaskType,
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
                            Description = "taskdesc",
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                WorkflowName = workflow.Workflow.Name,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
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

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(0));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ArtifactReceived_With_Happy_Path_Continues()
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
                            Type = ValidationConstants.ExportTaskType,
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
                            Description = "taskdesc",
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                WorkflowName = workflow.Workflow.Name,
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

            _artifactReceivedRepository.Setup(w => w.GetAllAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<ArtifactReceivedItems>());

            var mess = new ArtifactsReceivedEvent { WorkflowInstanceId = workflowInstance.Id, TaskId = "coffee" };


            var response = await WorkflowExecuterService.ProcessArtifactReceivedAsync(mess);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(0));
            _taskExecutionStatsRepository.Verify(w => w.UpdateExecutionStatsAsync(It.IsAny<TaskExecution>(), workflowId, TaskExecutionStatus.Succeeded));
            _workflowInstanceRepository.Verify(w => w.UpdateTaskStatusAsync(workflowInstanceId, "coffee", TaskExecutionStatus.Succeeded));

            response.Should().BeTrue();

            _workflowRepository.Verify(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>()), Times.Never());
            _workflowRepository.Verify(w => w.GetWorkflowsForWorkflowRequestAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessTaskUpdate_Timout_Sends_Sets_WorkflowInstanceStatus()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();

            var metadata = new Dictionary<string, object>();
            metadata.Add("a", "b");

            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "pizza",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Failed,
                Reason = FailureReason.TimedOut,
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
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                WorkflowName = workflow.Workflow.Name,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Created
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);
            _workflowInstanceRepository.Verify(r => r.UpdateWorkflowInstanceStatusAsync(It.IsAny<string>(), Status.Failed), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProcessTaskUpdate_Timout_Sends_Sets_Task_Status()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();

            var metadata = new Dictionary<string, object>();
            metadata.Add("a", "b");

            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "pizza",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Failed,
                Reason = FailureReason.TimedOut,
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
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                WorkflowName = workflow.Workflow.Name,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Accepted
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);
            _workflowInstanceRepository.Verify(r => r.UpdateTaskStatusAsync(workflowInstance.Id, "pizza", TaskExecutionStatus.Failed), Times.Once);
        }

        [Fact]
        public async Task ArtifactReceveid_Valid_ReturnesTrue()
        {
            var taskId = Guid.NewGuid().ToString();
            var workflowId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();
            var artifactEvent = new ArtifactsReceivedEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                WorkflowInstanceId = workflowInstanceId,
                TaskId = taskId
            };

            var workflows = new List<WorkflowRevision>
            {
                new WorkflowRevision
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId,
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
                                Id = taskId,
                                Type = "type",
                                Description = "outgoing",
                                TaskDestinations = new TaskDestination[] { new TaskDestination { Name = "task2" } }
                            },
                            new TaskObject {
                                Id = "task2",
                                Type = "type",
                                Description = "returning",
                            }
                        }
                    }
                }
            };
            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                BucketId = "BucketId",
                PayloadId = "PayloadId",
                WorkflowId = workflowId,
                Tasks = new List<TaskExecution>
                {
                    new TaskExecution{
                        TaskId = taskId,
                    }
                }
            };
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflows[0].WorkflowId)).ReturnsAsync(workflows[0]);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstanceId)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(It.IsAny<string>(), It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _artifactReceivedRepository.Setup(w => w.GetAllAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<ArtifactReceivedItems>());
            var result = await WorkflowExecuterService.ProcessArtifactReceivedAsync(artifactEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Once());

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessPayload_WithExportTask_NoExportsFails()
        {
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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

            _messageBrokerPublisherService.Setup(m => m.Publish(It.IsAny<string>(), It.IsAny<Message>()));

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish($"{_configuration.Value.Messaging.Topics.ExportRequestPrefix}.{_configuration.Value.Messaging.DicomAgents.ScuAgentName}", It.IsAny<Message>()), Times.Exactly(0));

            Assert.True(result);

#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Fact]
        public async Task ProcessPayload_WithHl7ExportTask_DispatchesExportHl7()
        {
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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
                                Type = "export_hl7",
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

            Message? messageSent = null;
            _messageBrokerPublisherService.Setup(m => m.Publish(It.IsAny<string>(), It.IsAny<Message>()))
                .Callback((string topic, Message m) => messageSent = m);

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.ExportHL7, It.IsAny<Message>()), Times.Exactly(1));

            Assert.True(result);
            Assert.NotNull(messageSent);
#pragma warning disable CS8604 // Possible null reference argument.
            var body = Encoding.UTF8.GetString(messageSent?.Body);
            var exportMessageBody = JsonConvert.DeserializeObject<ExportRequestEvent>(body);
            Assert.Empty(exportMessageBody!.PluginAssemblies);

            var exportEventMessage = messageSent.ConvertTo<ExportRequestEvent>();
            Assert.NotNull(exportEventMessage.Target);
            Assert.Equal(DataService.HL7, exportEventMessage.Target.DataService);

#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Fact]
        public async Task ProcessPayload_WithHl7ExportTask_NoExportsFails()
        {
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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
                                Type = "export_hl7",
                                Description = "taskdesc",
                                Artifacts = new ArtifactMap
                                {
                                    Input = new Artifact[] { new Artifact { Name = "dicomexport", Value = "{{ context.input }}" } }
                                },
                                TaskDestinations = Array.Empty<TaskDestination>(),
                                ExportDestinations = new ExportDestination[]
                                {
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

            _messageBrokerPublisherService.Setup(m => m.Publish(It.IsAny<string>(), It.IsAny<Message>()));

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.ExportHL7, It.IsAny<Message>()), Times.Exactly(0));

            Assert.True(result);

#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Fact]
        public async Task ProcessPayload_WithInvalidHl7ExportTask_DoesNotDispatchExportHl7()
        {
            // because this test has no  _artifactMapper.Setup(a => a.TryConvertArtifactVariablesToPath
            // it returns no matching artifacts. hence the failure !
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
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
                                Type = "export_hl7",
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

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.ExportHL7, It.IsAny<Message>()), Times.Exactly(0));
            Assert.True(result);
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEventWithExportHl7TaskDestination_ReturnsTrue()
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
                            Type = "export_hl7",
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
                WorkflowName = workflow.Workflow.Name,
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

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.ExportHL7, It.IsAny<Message>()), Times.Exactly(1));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEventWithExportHl7TaskDestination_NoExportDestinations_DoesNotDispatchExport()
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
                            Type = "export_hl7",
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
                WorkflowName = workflow.Workflow.Name,
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

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.ExportHL7, It.IsAny<Message>()), Times.Exactly(0));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessTaskUpdate_ValidTaskUpdateEventWith_Same_Status_returns_true()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();
            var taskId = Guid.NewGuid().ToString();

            var updateEvent = new TaskUpdateEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = "pizza",
                ExecutionId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Succeeded,
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
                        }
                    }
                }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId,
                WorkflowName = workflow.Workflow.Name,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "pizza",
                            Status = TaskExecutionStatus.Succeeded
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

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.ExportHL7, It.IsAny<Message>()), Times.Exactly(0));

            _logger.Verify(logger => logger.IsEnabled(LogLevel.Trace), Times.Once);

            response.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessPayload_With_Multiple_Taskdestinations_One_Has_Inputs()
        {
            var workflowInstanceId = Guid.NewGuid().ToString();
            var workflowId1 = Guid.NewGuid().ToString();
            var workflowId2 = Guid.NewGuid().ToString();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Workflows = new List<string>
                {
                    workflowId1.ToString()
                }
            };

            var workflows = new List<WorkflowRevision>
            {
                new ()
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
                            ExportDestinations = ["PROD_PACS"]
                        },
                        Tasks =
                        [
                            new TaskObject {
                                Id = "router",
                                Type = "router",
                                Description = "router",
                                Artifacts = new ArtifactMap
                                {
                                    Input = [new Artifact { Name = "dicomexport", Value = "{{ context.input }}" }],
                                    Output =
                                [
                                    new ()
                                    {
                                        Name = "Artifact1",
                                        Value = "Artifact1Value",
                                        Mandatory = true,
                                        Type = ArtifactType.DOC
                                    },
                                    new ()
                                    {
                                        Name = "Artifact2",
                                        Value = "Artifact2Value",
                                        Mandatory = true,
                                        Type = ArtifactType.CT
                                    }
                                ]
                                },
                                TaskDestinations = new TaskDestination[]
                                {
                                    new() {
                                        Name = "export1"
                                    },
                                    new ()
                                    {
                                        Name = "export2"
                                    }
                                }
                            },
                            new() {
                                Id ="export1",
                                Type = "export",
                                Artifacts = new ArtifactMap
                                {
                                    Input = [new () { Name = "Artifact1", Value = "{{ context.executions.router.artifacts.output.Artifact1 }}", Mandatory = true  }]
                                },
                                ExportDestinations = [new (){Name = "PROD_PACS"}]
                            },
                            new() {
                                Id ="export2",
                                Type = "export",
                                Artifacts = new ArtifactMap
                                {
                                    Input = [new () { Name = "Artifact2", Value = "{{ context.executions.router.artifacts.output.Artifact2 }}", Mandatory = true }]
                                },
                                ExportDestinations = [new (){Name = "PROD_PACS"}]
                            },
                        ]
                    }
                }
            };
            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = workflowId1,
                WorkflowName = workflows.First()!.Workflow!.Name,
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks =
                [
                    new()
                    {
                        TaskId = "router",
                        Status = TaskExecutionStatus.Created
                    }
                ]
            };

            var artifactDict = new List<Messaging.Common.Storage>
                {
                    new ()
                    {
                        Name = "artifactname",
                        RelativeRootPath = "path/to/artifact"
                    }
                };

            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);

            _workflowRepository.Setup(w => w.GetByWorkflowsIdsAsync(new List<string> { workflowId1.ToString() })).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowId1.ToString())).ReturnsAsync(workflows[0]);
            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.UpdateTasksAsync(It.IsAny<string>(), It.IsAny<List<TaskExecution>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            var dcmInfo = new Dictionary<string, string>() { { "dicomexport", "/dcm" } };
            _artifactMapper.Setup(a => a.TryConvertArtifactVariablesToPath(It.IsAny<Artifact[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), out dcmInfo)).Returns(true);

            _messageBrokerPublisherService.Setup(m => m.Publish(It.IsAny<string>(), It.IsAny<Message>()));

            var pathList = artifactDict.Select(a => a.RelativeRootPath).ToList();

            _storageService.Setup(w => w.VerifyObjectsExistAsync(
                workflowInstance.BucketId, It.Is<IReadOnlyList<string>>(l => l.Any(a => pathList.Any(p => p == a))), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, bool>() { { pathList.First(), true } });

            _storageService.Setup(w => w.ListObjectsAsync(It.IsAny<string>(), It.IsAny<string>(), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<VirtualFileInfo>()
                {
                                new VirtualFileInfo("testfile.dcm", "/dcm/testfile.dcm", "test", ulong.MaxValue)
                });

            var mess = new ArtifactsReceivedEvent
            {
                WorkflowInstanceId = workflowInstance.Id,
                TaskId = "router",
                Artifacts = [new Messaging.Common.Artifact { Type = ArtifactType.DOC, Path = "path/to/artifact" }],
                CorrelationId = Guid.NewGuid().ToString()
            };


            var response = await WorkflowExecuterService.ProcessArtifactReceivedAsync(mess);

            Assert.True(response);
            //_workflowInstanceRepository.Verify(w => w.UpdateTaskStatusAsync(workflowInstanceId, "router", TaskExecutionStatus.Succeeded));
            _workflowInstanceRepository.Verify(w => w.UpdateTaskStatusAsync(workflowInstanceId, "export1", TaskExecutionStatus.Dispatched));



#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Fact]
        public async Task ProcessPayload_With_Failing_Workflow_Conditional_Should_Not_Procced()
        {
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            var workflows = new List<WorkflowRevision>
            {
                new() {
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
                        Tasks =
                        [
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        ],
                        Predicate = ["{{ context.dicom.series.any('0010','0040') }} == 'lordge'"]
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetWorkflowsByAeTitleAsync(It.IsAny<List<string>>())).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetWorkflowsForWorkflowRequestAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflows[0].WorkflowId)).ReturnsAsync(workflows[0]);
            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Never());

            Assert.False(result);
        }

        [Fact]
        public async Task ProcessPayload_With_Passing_Workflow_Conditional_Should_Procced()
        {
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            var workflows = new List<WorkflowRevision>
            {
                new() {
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
                        Tasks =
                        [
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        ],
                        Predicate = ["{{ context.dicom.series.any('0010','0040') }} == 'lordge'"]
                    }
                }
            };

            _dicom.Setup(w => w.GetAnyValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(() => "lordge");

            _workflowRepository.Setup(w => w.GetWorkflowsByAeTitleAsync(It.IsAny<List<string>>())).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetWorkflowsForWorkflowRequestAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflows[0].WorkflowId)).ReturnsAsync(workflows[0]);
            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Once());

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessPayload_With_Empty_Workflow_Conditional_Should_Procced()
        {
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            var workflows = new List<WorkflowRevision>
            {
                new() {
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
                        Tasks =
                        [
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        ],
                        Predicate = []
                    }
                }
            };

            _dicom.Setup(w => w.GetAnyValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(() => "lordge");

            _workflowRepository.Setup(w => w.GetWorkflowsByAeTitleAsync(It.IsAny<List<string>>())).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetWorkflowsForWorkflowRequestAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflows[0].WorkflowId)).ReturnsAsync(workflows[0]);
            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, new Payload() { Id = Guid.NewGuid().ToString() });

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Once());

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessPayload_Payload_Should_Include_triggered_workflow_names()
        {
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                DataTrigger = new DataOrigin { Source = "aetitle", Destination = "aetitle" },
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            var workflows = new List<WorkflowRevision>
            {
                new() {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Revision = 1,
                    Workflow = new Workflow
                    {
                        Name = "Workflowname",
                    }
                }
            };

            _dicom.Setup(w => w.GetAnyValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(() => "lordge");

            _workflowRepository.Setup(w => w.GetWorkflowsByAeTitleAsync(It.IsAny<List<string>>())).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetWorkflowsForWorkflowRequestAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(workflows);
            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflows[0].WorkflowId)).ReturnsAsync(workflows[0]);
            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);
            var payload = new Payload() { Id = Guid.NewGuid().ToString() };
            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest, payload);


            Assert.Contains(workflows[0].Workflow!.Name, payload.TriggeredWorkflowNames);
            Assert.True(result);
        }
    }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
}
