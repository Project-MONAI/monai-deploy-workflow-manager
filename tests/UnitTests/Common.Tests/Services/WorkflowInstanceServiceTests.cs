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

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Exceptions;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManger.Common.Tests.Services
{
    public class WorkflowInstanceServiceTests
    {
        private IWorkflowInstanceService WorkflowInstanceService { get; set; }

        private readonly Mock<IWorkflowInstanceRepository> _workflowInstanceRepository;
        private readonly Mock<ILogger<WorkflowInstanceService>> _logger;

        public WorkflowInstanceServiceTests()
        {
            _workflowInstanceRepository = new Mock<IWorkflowInstanceRepository>();
            _logger = new Mock<ILogger<WorkflowInstanceService>>();

            WorkflowInstanceService = new WorkflowInstanceService(_workflowInstanceRepository.Object, _logger.Object);
        }

        [Fact]
        public async Task UpdateExportCompleteMetadataAsync_NullWorkflowInstanceId_ThrowsException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            await Assert.ThrowsAsync<ArgumentNullException>(() => WorkflowInstanceService.UpdateExportCompleteMetadataAsync(null, "45435436", new Dictionary<string, FileExportStatus>()));
        }

        [Fact]
        public async Task UpdateExportCompleteMetadataAsync_NullExecutionId_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => WorkflowInstanceService.UpdateExportCompleteMetadataAsync("45435436", null, new Dictionary<string, FileExportStatus>()));
        }

        [Fact]
        public async Task UpdateExportCompleteMetadataAsync_Update_Passes()
        {
            var fileExports = new Dictionary<string, FileExportStatus>()
            {
                { "export.dcm", FileExportStatus.Success },
                { "export2.dcm", FileExportStatus.ConfigurationError },
            };
            await WorkflowInstanceService.UpdateExportCompleteMetadataAsync("45435436", "4544223434", fileExports);
        }

        [Fact]
        public async Task AcknowledgeTaskError_NullWorkflowInstanceId_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => WorkflowInstanceService.AcknowledgeTaskError(null, "45435436"));
        }

        [Fact]
        public async Task AcknowledgeTaskError_NullExecutionId_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => WorkflowInstanceService.AcknowledgeTaskError("45435436", null));
        }

        [Fact]
        public async Task GetAllFailedAsync_ReturnsExpected()
        {
            var workflowInstances = new List<WorkflowInstance> {
                new WorkflowInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    Status = Status.Failed,
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            Status = TaskExecutionStatus.Failed
                        }
                    }
                }
            };

            _workflowInstanceRepository.Setup(w => w.GetAllFailedAsync()).ReturnsAsync(workflowInstances);

            var result = await WorkflowInstanceService.GetAllFailedAsync();

            result.Should().BeEquivalentTo(workflowInstances);
        }

        [Fact]
        public async Task AcknowledgeTaskError_WorkflowDoesNotExist_ThrowsNotFoundException()
        {
            var workflowInstance = new WorkflowInstance
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                Status = Status.Failed,
                Tasks = new List<TaskExecution>
                {
                    new TaskExecution
                    {
                        ExecutionId = Guid.NewGuid().ToString(),
                        Status = TaskExecutionStatus.Failed
                    }
                }
            };

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(value: null);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

            await Assert.ThrowsAsync<MonaiNotFoundException>(() => WorkflowInstanceService.AcknowledgeTaskError(workflowInstance.Id, workflowInstance.Tasks.First().ExecutionId));
        }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        [Fact]
        public async Task AcknowledgeTaskError_WorkflowNotFailed_ThrowsBadRequestException()
        {
            var workflowInstance = new WorkflowInstance
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                Status = Status.Failed,
                Tasks = new List<TaskExecution>
                {
                    new TaskExecution
                    {
                        ExecutionId = Guid.NewGuid().ToString(),
                        Status = TaskExecutionStatus.Succeeded
                    }
                }
            };

            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);

            await Assert.ThrowsAsync<MonaiBadRequestException>(() => WorkflowInstanceService.AcknowledgeTaskError(workflowInstance.Id, workflowInstance.Tasks.First().ExecutionId));
        }

        [Fact]
        public async Task AcknowledgeTaskError_WorkflowExists_ReturnsUpdatedWorkflow()
        {
            var workflowInstance = new WorkflowInstance
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                Status = Status.Failed,
                Tasks = new List<TaskExecution>
                {
                    new TaskExecution
                    {
                        ExecutionId = Guid.NewGuid().ToString(),
                        Status = TaskExecutionStatus.Failed
                    }
                }
            };

            var updatedTaskInstance = new WorkflowInstance
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                Status = Status.Failed,
                Tasks = new List<TaskExecution>
                {
                    new TaskExecution
                    {
                        ExecutionId = Guid.NewGuid().ToString(),
                        Status = TaskExecutionStatus.Failed,
                        AcknowledgedTaskErrors = DateTime.UtcNow
                    }
                }
            };

            var updatedWorkflowTaskInstance = new WorkflowInstance
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                Status = Status.Failed,
                Tasks = new List<TaskExecution>
                {
                    new TaskExecution
                    {
                        ExecutionId = Guid.NewGuid().ToString(),
                        Status = TaskExecutionStatus.Failed,
                        AcknowledgedTaskErrors = DateTime.UtcNow
                    }
                },
                AcknowledgedWorkflowErrors = DateTime.UtcNow
            };

            _workflowInstanceRepository.Setup(w => w.GetByWorkflowInstanceIdAsync(workflowInstance.Id)).ReturnsAsync(workflowInstance);
            _workflowInstanceRepository.Setup(w => w.AcknowledgeTaskError(workflowInstance.Id, workflowInstance.Tasks.First().ExecutionId)).ReturnsAsync(updatedTaskInstance);
            _workflowInstanceRepository.Setup(w => w.AcknowledgeWorkflowInstanceErrors(workflowInstance.Id)).ReturnsAsync(updatedWorkflowTaskInstance);

            var result = await WorkflowInstanceService.AcknowledgeTaskError(workflowInstance.Id, workflowInstance.Tasks.First().ExecutionId);

            result.Should().BeEquivalentTo(updatedWorkflowTaskInstance);
        }
    }
}
