// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Common;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.WorkflowExecuter.Tests.Common
{
    public class ArtifactMapperTests
    {
        private IArtifactMapper ArtifactMapper { get; set; }

        private readonly Mock<IWorkflowInstanceRepository> _workflowInstanceRepository;
        public ArtifactMapperTests()
        {
            _workflowInstanceRepository = new Mock<IWorkflowInstanceRepository>();

            ArtifactMapper = new ArtifactMapper(_workflowInstanceRepository.Object);
        }

        [Fact]
        public async Task ConvertArtifactVariablesToPath_MultipleArtifacts_ReturnsMappedPaths()
        {
            var artifacts = new Artifact[]
            {
                new Artifact
                {
                    Name = "taskartifact",
                    Value = "{{ context.executions.image_type_detector.artifacts.dicom }}"
                },
                new Artifact
                {
                    Name = "dicomimage",
                    Value = "{{ context.input }}"
                },
                new Artifact
                {
                    Name = "outputtaskdir",
                    Value = "{{ context.executions.coffee.output_dir }}"
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

            var response = await ArtifactMapper.ConvertArtifactVariablesToPath(artifacts, workflowInstance.PayloadId, workflowInstance.Id);

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
                    Value = "{{ test.not.an.artifact }}"
                },
                new Artifact
                {
                    Name = "dicomimage",
                    Value = "{{ context }}"
                },
                new Artifact
                {
                    Name = "outputtaskdir",
                    Value = "{{ sandwich.executions.coffee.output_dir }}"
                },
                new Artifact
                {
                    Name = "outputtaskdir4",
                    Value = "{{ sandwich.executions.coffee.artifacts.test }}"
                },
                new Artifact
                {
                    Name = "outputtaskdir2",
                    Value = "{{sandwich.executions.coffee.output_dir"
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

            var response = await ArtifactMapper.ConvertArtifactVariablesToPath(artifacts, workflowInstance.PayloadId, workflowInstance.Id);

            response.Should().BeEquivalentTo(expected);
        }
    }
}
