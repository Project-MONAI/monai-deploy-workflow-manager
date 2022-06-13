﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Common;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Services;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.WorkflowExecuter.Tests.Services
{
    public class WorkflowExecuterServiceTests
    {
        private IWorkflowExecuterService WorkflowExecuterService { get; set; }

        private readonly Mock<IWorkflowRepository> _workflowRepository;
        private readonly Mock<ILogger<WorkflowExecuterService>> _logger;
        private readonly Mock<IWorkflowInstanceRepository> _workflowInstanceRepository;
        private readonly Mock<IMessageBrokerPublisherService> _messageBrokerPublisherService;
        private readonly IOptions<WorkflowManagerOptions> _configuration;
        private readonly IOptions<StorageServiceConfiguration> _storageConfiguration;

        public WorkflowExecuterServiceTests()
        {
            _workflowRepository = new Mock<IWorkflowRepository>();
            _logger = new Mock<ILogger<WorkflowExecuterService>>();
            _workflowInstanceRepository = new Mock<IWorkflowInstanceRepository>();
            _messageBrokerPublisherService = new Mock<IMessageBrokerPublisherService>();
            _configuration = Options.Create(new WorkflowManagerOptions()
            {
                Messaging = new MessageBrokerConfiguration
                {
                    Topics = new MessageBrokerConfigurationKeys
                    {
                        TaskDispatchRequest = "md.task.dispatch"
                    }
                }
            });
            _storageConfiguration = Options.Create(new StorageServiceConfiguration());

            WorkflowExecuterService = new WorkflowExecuterService(
                _logger.Object,
                _configuration,
                _storageConfiguration,
                _workflowRepository.Object,
                _workflowInstanceRepository.Object,
                _messageBrokerPublisherService.Object,
                new ConditionalParameterParser(_logger.Object));
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

            _workflowRepository.Setup(w => w.GetWorkflowsByAeTitleAsync(workflowRequest.CalledAeTitle)).ReturnsAsync(workflows);

            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest);

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

            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<WorkflowInstance>());
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(2));

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

            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowsIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(workflowsInstance);
            _workflowInstanceRepository.Setup(w => w.UpdateTaskStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TaskExecutionStatus>())).ReturnsAsync(true);

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest);

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

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(2));

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

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(1));

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

            var response = await WorkflowExecuterService.ProcessTaskUpdate(updateEvent);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(1));

            response.Should().BeTrue();
        }

        [Fact]
        public async Task HandleTaskDestinations_ShouldReturnOnlyRequiredExecutions_ReturnsCoffeeDoughnutsButNotSalad()
        {
            #region TaskDestinations/Condtionals
            var tdCoffee = new TaskDestination
            {
                Name = "coffee",
            };

            var tdDoughnuts = new TaskDestination
            {
                Name = "doughnuts",
                Conditions = "{{ context.executions.task['doughnuts'].'A' }} == 'B'"
            };

            var tdSalad = new TaskDestination
            {
                Name = "salad",
                Conditions = "{{ context.executions.task['doughnuts'].'A' }} == 'A'"
            };

            var currentTaskDestinations = new TaskDestination[]
            {
                tdCoffee, tdDoughnuts, tdSalad
            };
            #endregion

            #region TaskExecutiuons/MetaData
            var metadata = new Dictionary<string, object>() { { "A", "B" } };

            var tePizza = new TaskExecution
            {
                TaskId = "pizza",
                Status = TaskExecutionStatus.Created
            };

            var teDoughnuts = new TaskExecution
            {
                TaskId = "doughnuts",
                Status = TaskExecutionStatus.Created,
                Metadata = metadata
            };

            var teCoffee = new TaskExecution
            {
                TaskId = "coffee",
                Status = TaskExecutionStatus.Created,
            };

            var teSalad = new TaskExecution
            {
                TaskId = "salad",
                Status = TaskExecutionStatus.Created,
            };
            #endregion

            var workflowInstance = new WorkflowInstance
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                PayloadId = Guid.NewGuid().ToString(),
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                {
                    tePizza, teCoffee, teDoughnuts
                }
            };

            var workflow = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
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
                                tdCoffee, tdDoughnuts
                            }
                        },
                        new TaskObject
                        {
                            Id = "doughnuts",
                            Type = "type",
                            Description = "taskdesc",
                            InputParameters = new Dictionary<string, object>
                            {
                                { "param1", @"{{ context.executions.task['doughnuts'].'A' }}" },
                                { "param2", @"hello" }
                            }
                        },
                        new TaskObject
                        {
                            Id = "coffee",
                            Type = "type",
                            Description = "taskdesc"
                        }
                    }
                }
            };

            var result = WorkflowExecuterService.HandleTaskDestinations(workflowInstance,
                                                                          workflow,
                                                                          currentTaskDestinations,
                                                                          metadata);

            Assert.Contains(result, r => r.TaskId == teCoffee.TaskId);
            Assert.Contains(result, r => r.TaskId == teDoughnuts.TaskId);
            Assert.DoesNotContain(result, r => r.TaskId == teSalad.TaskId);

            var param1Result = result.First(r => r.TaskId == "doughnuts").InputParameters["param1"].Equals("'B'");
            var param2Result = result.First(r => r.TaskId == "doughnuts").InputParameters["param2"].Equals("hello");

            Assert.True(param1Result);
            Assert.True(param2Result);
        }

    }
}
