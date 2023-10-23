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
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Common;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Tests.Common
{
    public class ArtifactMapperTests
    {
        private IArtifactMapper ArtifactMapper { get; set; }

        private readonly Mock<IWorkflowInstanceRepository> _workflowInstanceRepository;
        private readonly Mock<IStorageService> _storageService;
        private readonly Mock<ILogger<ArtifactMapper>> _logger;

        public ArtifactMapperTests()
        {
            _workflowInstanceRepository = new Mock<IWorkflowInstanceRepository>();
            _storageService = new Mock<IStorageService>();
            _logger = new Mock<ILogger<ArtifactMapper>>();

            ArtifactMapper = new ArtifactMapper(_workflowInstanceRepository.Object, _storageService.Object, _logger.Object);
        }

        [Fact]
        public void ConvertArtifactVariablesToPath_MultipleArtifacts_ReturnsMappedPaths()
        {
            var artifacts = new Artifact[]
            {
                new Artifact
                {
                    Name = "taskartifact",
                    Value = "{{ context.executions.image_type_detector.artifacts.dicom }}",
                    Mandatory = true
                },
                new Artifact
                {
                    Name = "dicomimage",
                    Value = "{{ context.input.dicom }}",
                    Mandatory = true
                },
                new Artifact
                {
                    Name = "outputtaskdir",
                    Value = "{{ context.executions.coffee.output_dir }}",
                    Mandatory = true
                }
            };

            var payloadId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();
            var executionId = Guid.NewGuid().ToString();

            var expected = new Dictionary<string, string>
            {
                { "dicom", $"{payloadId}/workflows/{workflowInstanceId}/{executionId}" },
                { "dicomimage", $"{payloadId}/dcm" },
                { "outputtaskdir", $"{payloadId}/workflows/{workflowInstanceId}/{executionId}" }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = Guid.NewGuid().ToString(),
                PayloadId = payloadId,
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "image_type_detector",
                            ExecutionId = executionId,
                            Status = TaskExecutionStatus.Dispatched,
                            OutputArtifacts = new Dictionary<string, string>
                            {
                                { "dicom", $"{payloadId}/workflows/{workflowInstanceId}/{executionId}" }
                            }
                        },
                        new TaskExecution
                        {
                            TaskId = "coffee",
                            Status = TaskExecutionStatus.Created,
                            OutputDirectory = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}"
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "image_type_detector")).ReturnsAsync(workflowInstance.Tasks[0]);
            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "coffee")).ReturnsAsync(workflowInstance.Tasks[1]);

            var value1 = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}";
            var value2 = $"{payloadId}/dcm";
            var value3 = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}";

            _storageService.Setup(w => w.VerifyObjectExistsAsync(workflowInstance.BucketId, value1, default)).ReturnsAsync(true);
            _storageService.Setup(w => w.VerifyObjectExistsAsync(workflowInstance.BucketId, value2, default)).ReturnsAsync(true);
            _storageService.Setup(w => w.VerifyObjectExistsAsync(workflowInstance.BucketId, value3, default)).ReturnsAsync(true);

            var result = ArtifactMapper.TryConvertArtifactVariablesToPath(artifacts, workflowInstance.PayloadId, workflowInstance.Id, workflowInstance.BucketId, true, out var response);
            response.Should().NotBeNullOrEmpty();
            Assert.True(result);

            response.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task ConvertArtifactVariablesToPath_MultipleMissingRequiredArtifacts_ThrowsException()
        {
            var artifacts = new Artifact[]
            {
                new Artifact
                {
                    Name = "taskartifact",
                    Value = "{{ context.executions.image_type_detector.artifacts.dicom }}",
                    Mandatory = true
                },
                new Artifact
                {
                    Name = "dicomimage",
                    Value = "{{ context.input.dicom }}",
                    Mandatory = true
                },
                new Artifact
                {
                    Name = "outputtaskdir",
                    Value = "{{ context.executions.coffee.output_dir }}",
                    Mandatory = true
                }
            };

            var payloadId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();
            var executionId = Guid.NewGuid().ToString();

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = Guid.NewGuid().ToString(),
                PayloadId = payloadId,
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                {
                    new TaskExecution
                    {
                        TaskId = "image_type_detector",
                        ExecutionId = executionId,
                        Status = TaskExecutionStatus.Dispatched,
                        OutputArtifacts = new Dictionary<string, string>
                        {
                            { "dicom", $"{payloadId}/workflows/{workflowInstanceId}/{executionId}" }
                        }
                    },
                    new TaskExecution
                    {
                        TaskId = "coffee",
                        Status = TaskExecutionStatus.Created,
                        OutputDirectory = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}"
                    }
                }
            };

            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "image_type_detector")).ReturnsAsync(workflowInstance.Tasks[0]);
            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "coffee")).ReturnsAsync(workflowInstance.Tasks[1]);

            _storageService.Setup(w => w.VerifyObjectExistsAsync(workflowInstance.BucketId, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<FileNotFoundException>(async () => await ArtifactMapper.ConvertArtifactVariablesToPath(artifacts, workflowInstance.PayloadId, workflowInstance.Id, workflowInstance.BucketId));
        }

        [Fact]
        public void TryConvertArtifactVariablesToPath_MultipleMissingRequiredArtifacts_ShouldNotThrow()
        {
            var artifacts = new Artifact[]
            {
                new Artifact
                {
                    Name = "taskartifact",
                    Value = "{{ context.executions.image_type_detector.artifacts.dicom }}",
                    Mandatory = true
                },
                new Artifact
                {
                    Name = "dicomimage",
                    Value = "{{ context.input.dicom }}",
                    Mandatory = true
                },
                new Artifact
                {
                    Name = "outputtaskdir",
                    Value = "{{ context.executions.coffee.output_dir }}",
                    Mandatory = true
                }
            };

            var payloadId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();
            var executionId = Guid.NewGuid().ToString();

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = Guid.NewGuid().ToString(),
                PayloadId = payloadId,
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                {
                    new TaskExecution
                    {
                        TaskId = "image_type_detector",
                        ExecutionId = executionId,
                        Status = TaskExecutionStatus.Dispatched,
                        OutputArtifacts = new Dictionary<string, string>
                        {
                            { "dicom", $"{payloadId}/workflows/{workflowInstanceId}/{executionId}" }
                        }
                    },
                    new TaskExecution
                    {
                        TaskId = "coffee",
                        Status = TaskExecutionStatus.Created,
                        OutputDirectory = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}"
                    }
                }
            };

            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "image_type_detector")).ReturnsAsync(workflowInstance.Tasks[0]);
            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "coffee")).ReturnsAsync(workflowInstance.Tasks[1]);

            _storageService.Setup(w => w.VerifyObjectExistsAsync(workflowInstance.BucketId, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = ArtifactMapper.TryConvertArtifactVariablesToPath(artifacts, workflowInstance.PayloadId, workflowInstance.Id, workflowInstance.BucketId, true, out var response);
            Assert.NotNull(response);
            Assert.False(result);
        }

        [Fact]
        public void TryConvertArtifactVariablesToPath_MultipleMissingRequiredArtifacts_ShouldThrowWhenNotFileNotFoundException()
        {
            var artifacts = new Artifact[]
            {
                new Artifact
                {
                    Name = "taskartifact",
                    Value = "{{ context.executions.image_type_detector.artifacts.dicom }}",
                    Mandatory = true
                },
                new Artifact
                {
                    Name = "dicomimage",
                    Value = "{{ context.input.dicom }}",
                    Mandatory = true
                },
                new Artifact
                {
                    Name = "outputtaskdir",
                    Value = "{{ context.executions.coffee.output_dir }}",
                    Mandatory = true
                }
            };

            var payloadId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();
            var executionId = Guid.NewGuid().ToString();

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = Guid.NewGuid().ToString(),
                PayloadId = payloadId,
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                {
                    new TaskExecution
                    {
                        TaskId = "image_type_detector",
                        ExecutionId = executionId,
                        Status = TaskExecutionStatus.Dispatched,
                        OutputArtifacts = new Dictionary<string, string>
                        {
                            { "dicom", $"{payloadId}/workflows/{workflowInstanceId}/{executionId}" }
                        }
                    },
                    new TaskExecution
                    {
                        TaskId = "coffee",
                        Status = TaskExecutionStatus.Created,
                        OutputDirectory = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}"
                    }
                }
            };

            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "image_type_detector")).ReturnsAsync(workflowInstance.Tasks[0]);
            var exceptionMessage = "test exception";
            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "coffee")).Throws(new Exception(exceptionMessage));

            _storageService.Setup(w => w.VerifyObjectExistsAsync(workflowInstance.BucketId, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var ex = Assert.Throws<AggregateException>(() => ArtifactMapper.TryConvertArtifactVariablesToPath(artifacts, workflowInstance.PayloadId, workflowInstance.Id, workflowInstance.BucketId, false, out var response));
            ex.InnerException.Should().BeOfType<Exception>();
            ex?.InnerException?.Message.Should().Be(exceptionMessage);
        }

        [Fact]
        public async Task ConvertArtifactVariablesToPath_RequiredArtifact_ThrowsException()
        {
            var artifacts = new Artifact[]
            {
                new Artifact
                {
                    Name = "taskartifact",
                    Value = "{{ context.executions.image_type_detector.artifacts.dicom }}",
                    Mandatory = true
                },
                new Artifact
                {
                    Name = "dicomimage",
                    Value = "{{ context.input.dicom }}",
                    Mandatory = true
                },
                new Artifact
                {
                    Name = "outputtaskdir",
                    Value = "{{ context.executions.coffee.output_dir }}",
                    Mandatory = true
                }
            };

            var payloadId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();
            var executionId = Guid.NewGuid().ToString();

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = Guid.NewGuid().ToString(),
                PayloadId = payloadId,
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "image_type_detector",
                            ExecutionId = executionId,
                            Status = TaskExecutionStatus.Dispatched,
                            OutputArtifacts = new Dictionary<string, string>
                            {
                                { "dicom", $"{payloadId}/workflows/{workflowInstanceId}/{executionId}/" }
                            }
                        },
                        new TaskExecution
                        {
                            TaskId = "coffee",
                            Status = TaskExecutionStatus.Created,
                            OutputDirectory = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}/"
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "image_type_detector")).ReturnsAsync(workflowInstance.Tasks[0]);
            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "coffee")).ReturnsAsync(workflowInstance.Tasks[1]);

            var value1 = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}/dicom";
            var value2 = $"{payloadId}/dcm/dicomimage";
            var value3 = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}/outputtaskdir";

            _storageService.Setup(w => w.VerifyObjectExistsAsync(workflowInstance.BucketId, value1, default)).ReturnsAsync(false);
            _storageService.Setup(w => w.VerifyObjectExistsAsync(workflowInstance.BucketId, value2, default)).ReturnsAsync(true);
            _storageService.Setup(w => w.VerifyObjectExistsAsync(workflowInstance.BucketId, value3, default)).ReturnsAsync(true);

            await Assert.ThrowsAsync<FileNotFoundException>(() => ArtifactMapper.ConvertArtifactVariablesToPath(artifacts, workflowInstance.PayloadId, workflowInstance.Id, workflowInstance.BucketId));
        }

        [Fact]
        public async Task ConvertArtifactVariablesToPath_RequiredArtifactShouldNotExistYet_ReturnsMappedPaths()
        {
            var artifacts = new Artifact[]
            {
                new Artifact
                {
                    Name = "taskartifact",
                    Value = "{{ context.executions.image_type_detector.artifacts.dicom }}",
                    Mandatory = true
                },
                new Artifact
                {
                    Name = "dicomimage",
                    Value = "{{ context.input.dicom }}",
                    Mandatory = true
                },
                new Artifact
                {
                    Name = "outputtaskdir",
                    Value = "{{ context.executions.coffee.output_dir }}",
                    Mandatory = true
                }
            };

            var payloadId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();
            var executionId = Guid.NewGuid().ToString();

            var expected = new Dictionary<string, string>
            {
                { "dicom", $"{payloadId}/workflows/{workflowInstanceId}/{executionId}" },
                { "dicomimage", $"{payloadId}/dcm" },
                { "outputtaskdir", $"{payloadId}/workflows/{workflowInstanceId}/{executionId}" }
            };

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = Guid.NewGuid().ToString(),
                PayloadId = payloadId,
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "image_type_detector",
                            ExecutionId = executionId,
                            Status = TaskExecutionStatus.Dispatched,
                            OutputArtifacts = new Dictionary<string, string>
                            {
                                { "dicom", $"{payloadId}/workflows/{workflowInstanceId}/{executionId}" }
                            }
                        },
                        new TaskExecution
                        {
                            TaskId = "coffee",
                            Status = TaskExecutionStatus.Created,
                            OutputDirectory = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}"
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "image_type_detector")).ReturnsAsync(workflowInstance.Tasks[0]);
            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "coffee")).ReturnsAsync(workflowInstance.Tasks[1]);

            var value1 = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}/dicom";
            var value2 = $"{payloadId}/dcm/dicomimage";
            var value3 = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}/outputtaskdir";

            _storageService.Setup(w => w.VerifyObjectExistsAsync(workflowInstance.BucketId, value1, default)).ReturnsAsync(false);
            _storageService.Setup(w => w.VerifyObjectExistsAsync(workflowInstance.BucketId, value2, default)).ReturnsAsync(true);
            _storageService.Setup(w => w.VerifyObjectExistsAsync(workflowInstance.BucketId, value3, default)).ReturnsAsync(true);

            var response = await ArtifactMapper.ConvertArtifactVariablesToPath(artifacts, workflowInstance.PayloadId, workflowInstance.Id, workflowInstance.BucketId, false);

            response.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task ConvertArtifactVariablesToPath_MultipleInvalid_ReturnsEmptyDict()
        {
            var artifacts = new Artifact[]
            {
                new Artifact
                {
                    Name = "taskartifact",
                    Value = "{{ test.not.an.artifact }}",
                    Mandatory = false
                },
                new Artifact
                {
                    Name = "dicomimage",
                    Value = "{{ context }}",
                    Mandatory = false
                },
                new Artifact
                {
                    Name = "outputtaskdir",
                    Value = "{{ sandwich.executions.coffee.output_dir }}",
                    Mandatory = false
                },
                new Artifact
                {
                    Name = "outputtaskdir4",
                    Value = "{{ sandwich.executions.coffee.artifacts.test }}",
                    Mandatory = false
                },
                new Artifact
                {
                    Name = "outputtaskdir2",
                    Value = "{{sandwich.executions.coffee.output_dir",
                    Mandatory = false
                }
            };

            var payloadId = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();
            var executionId = Guid.NewGuid().ToString();

            var expected = new Dictionary<string, string>();

            var workflowInstance = new WorkflowInstance
            {
                Id = workflowInstanceId,
                WorkflowId = Guid.NewGuid().ToString(),
                PayloadId = payloadId,
                Status = Status.Created,
                BucketId = "bucket",
                Tasks = new List<TaskExecution>
                    {
                        new TaskExecution
                        {
                            TaskId = "image_type_detector",
                            ExecutionId = executionId,
                            Status = TaskExecutionStatus.Dispatched,
                            OutputArtifacts = new Dictionary<string, string>
                            {
                                { "dicom", $"{payloadId}/workflows/{workflowInstanceId}/{executionId}" }
                            }
                        },
                        new TaskExecution
                        {
                            TaskId = "coffee",
                            Status = TaskExecutionStatus.Created,
                            OutputDirectory = $"{payloadId}/workflows/{workflowInstanceId}/{executionId}"
                        }
                    }
            };

            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "image_type_detector")).ReturnsAsync(workflowInstance.Tasks[0]);
            _workflowInstanceRepository.Setup(w => w.GetTaskByIdAsync(workflowInstance.Id, "coffee")).ReturnsAsync(workflowInstance.Tasks[1]);

            var response = await ArtifactMapper.ConvertArtifactVariablesToPath(artifacts, workflowInstance.PayloadId, workflowInstance.Id, workflowInstance.BucketId);

            response.Should().BeEquivalentTo(expected);
        }
    }
}
