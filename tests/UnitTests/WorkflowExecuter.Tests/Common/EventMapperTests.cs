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
using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Common;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Tests.Common
{
    public class EventMapperTests
    {
        [Fact]
        public void ToTaskDispatchEvent_ValidAeTitleWorkflowRequest_ReturnsTaskDispatch()
        {
            var task = new TaskExecution
            {
                ExecutionId = Guid.NewGuid().ToString(),
                TaskType = "taskType",
                TaskPluginArguments = new Dictionary<string, string>
                {
                    { "key", "value" }
                },
                TaskId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Created,
                InputArtifacts = new Dictionary<string, string>
                {
                    { "key", "value" }
                },
                ResultMetadata = new Dictionary<string, object> { },
                OutputDirectory = "minio/workflowid/taskid"
            };

            var configuration = new StorageServiceConfiguration
            {
                Settings = new Dictionary<string, string>
                {
                    { "securedConnection", "false" },
                     { "endpoint", "localhost" }
                }
            };

            var correlationId = Guid.NewGuid().ToString();

            var workflowInstance = new WorkflowInstance
            {
                Id = Guid.NewGuid().ToString(),
                PayloadId = Guid.NewGuid().ToString(),
                BucketId = "bucket"
            };

            var outputArtifacts = new Dictionary<string, string>()
            {
                { "testoutput", "minio/workflowid/taskid/artifact" }
            };

            var expectedTask = new TaskDispatchEvent
            {
                WorkflowInstanceId = workflowInstance.Id,
                TaskId = task.TaskId,
                ExecutionId = task.ExecutionId.ToString(),
                PayloadId = workflowInstance.PayloadId,
                CorrelationId = correlationId,
                Status = TaskExecutionStatus.Created,
                TaskPluginType = task.TaskType,
                Inputs = new List<Messaging.Common.Storage>
                {
                    new Messaging.Common.Storage
                    {
                        SecuredConnection = bool.Parse(configuration.Settings["securedConnection"]),
                        Endpoint = configuration.Settings["endpoint"],
                        Bucket = workflowInstance.BucketId,
                        RelativeRootPath = "value",
                        Name = "key"
                    }
                },
                Outputs = new List<Messaging.Common.Storage>
                {
                    new Messaging.Common.Storage
                    {
                        SecuredConnection = bool.Parse(configuration.Settings["securedConnection"]),
                        Endpoint = configuration.Settings["endpoint"],
                        Bucket = workflowInstance.BucketId,
                        RelativeRootPath = "minio/workflowid/taskid/artifact",
                        Name = "testoutput"
                    }
                },
                Metadata = { },
                TaskPluginArguments = new Dictionary<string, string>
                {
                    { "key", "value" }
                },
                IntermediateStorage = new Messaging.Common.Storage
                {
                    Bucket = workflowInstance.BucketId,
                    Endpoint = configuration.Settings["endpoint"],
                    Name = task.TaskId,
                    RelativeRootPath = "minio/workflowid/taskid",
                    SecuredConnection = bool.Parse(configuration.Settings["securedConnection"])
                }
            };

            var taskDispatch = EventMapper.ToTaskDispatchEvent(task, workflowInstance, outputArtifacts, correlationId, configuration);

            taskDispatch.Should().BeEquivalentTo(expectedTask, options =>
                options.Excluding(t => t.CorrelationId));
        }

        [Fact]
        public void ToExportRequestEvent_ValidOutputArtifacts_ReturnsExportRequest()
        {
            var task = new TaskExecution
            {
                ExecutionId = Guid.NewGuid().ToString(),
                TaskType = "taskType",
                TaskPluginArguments = new Dictionary<string, string>
                {
                    { "key", "value" }
                },
                TaskId = Guid.NewGuid().ToString(),
                Status = TaskExecutionStatus.Created,
                InputArtifacts = new Dictionary<string, string>
                {
                    { "key", "value" }
                },
                ResultMetadata = new Dictionary<string, object> { },
                OutputDirectory = "minio/workflowid/taskid"
            };

            var exportDestinations = new string[] { "test" };

            var dicomImages = new List<string> { "dicom" };

            var workflowInstanceId = Guid.NewGuid().ToString();
            var correlationId = Guid.NewGuid().ToString();

            var expected = new ExportRequestEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                ExportTaskId = task.TaskId,
                CorrelationId = correlationId,
                Files = dicomImages,
                Destinations = exportDestinations,
                Target = new DataOrigin
                {
                    Destination = exportDestinations[0],
                    DataService = DataService.DIMSE,
                    Source = "WFM"
                }
            };

            var exportRequest = EventMapper.ToExportRequestEvent(dicomImages, exportDestinations, task.TaskId, workflowInstanceId, correlationId);
            exportRequest.Target!.Source = "WFM";

            exportRequest.Should().BeEquivalentTo(expected);
        }
    }
}
