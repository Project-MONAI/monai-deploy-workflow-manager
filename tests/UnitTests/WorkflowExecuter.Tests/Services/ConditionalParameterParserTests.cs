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
using System.Linq;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Parser;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Storage.Services;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.WorkflowExecuter.Tests.Services
{
    public class ConditionalParameterParserTests
    {
        private readonly Mock<IDicomService> _dicom;
        private readonly Mock<IWorkflowInstanceService> _workflowInstanceService;
        private readonly Mock<ILogger<ConditionalParameterParser>> _logger;
        private readonly Mock<IPayloadService> _payloadService;
        private readonly Mock<IWorkflowService> _workflowService;

        public ConditionalParameterParserTests()
        {
            _logger = new Mock<ILogger<ConditionalParameterParser>>();
            _workflowInstanceService = new Mock<IWorkflowInstanceService>();
            _payloadService = new Mock<IPayloadService>();
            _dicom = new Mock<IDicomService>();
            _workflowService = new Mock<IWorkflowService>();
        }

        [Theory]
        [InlineData(new string[] {"{{ context.dicom.series.all('0010','0040') }} == 'lordge'",
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.'Fred' }} == 'Bob'",
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.'fred' }} == 'lowercasefred'" }, true, "lordge")]
        [InlineData(
            new string[] {"{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.'Fred' }} == 'Bob'",
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.'fred' }} == 'lowercasefred'",
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.'Sandra' }} == 'YassQueen' OR " +
            "{{ context.executions.other_task.result.'Fred' }} >= '32' OR " +
            "{{ context.executions.other_task.result.'Sandra' }} == 'other YassQueen' OR " +
            "{{ context.executions.other_task.result.'Derick' }} == 'lordge'" }, true)]
        [InlineData(
            new string[] {"{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.execution_stats.'stat1' }} == 'completed in 1 hour' AND " +
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.execution_stats.'stat2' }} ==  'ran successfully'"}, true)]
        public void ConditionalParameterParser_WhenGivenCorrectResultMetadataString_MultiConditionShouldEvaluate(string[] input, bool expectedResult, string? expectedDicomReturn = null)
        {
            if (expectedDicomReturn is not null)
            {
                _dicom.Setup(w => w.GetAnyValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                .ReturnsAsync(() => expectedDicomReturn);
                _dicom.Setup(w => w.GetAllValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(() => expectedDicomReturn);
            }

            var testData = CreateTestData();
            var workflow = testData.First();
            workflow.BucketId = "bucket1";

            var conditionalParameterParser = new ConditionalParameterParser(_logger.Object, _dicom.Object, _workflowInstanceService.Object, _payloadService.Object, _workflowService.Object);

            var actualResult = conditionalParameterParser.TryParse(input, workflow);

            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("{{ context.dicom.series.any('0010','0040') }} == 'lordge'", true, "lordge")]
        [InlineData("{{ context.dicom.series.all('0010','0040') }} == 'lordge'", true, "lordge")]
        [InlineData("'invalid' > 'false'", false)]
        public void ConditionalParameterParser_WhenGivenCorrectDicomString_ShouldEvaluate(string input, bool expectedResult, string? expectedDicomReturn = null)
        {
            if (expectedDicomReturn is not null)
            {
                _dicom.Setup(w => w.GetAnyValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                .ReturnsAsync(() => expectedDicomReturn);
                _dicom.Setup(w => w.GetAllValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(() => expectedDicomReturn);
            }

            var testData = CreateTestData();
            var workflow = testData.First();
            workflow.BucketId = "bucket1";

            var conditionalParameterParser = new ConditionalParameterParser(_logger.Object, _dicom.Object, _workflowInstanceService.Object, _payloadService.Object, _workflowService.Object);

            var actualResult = conditionalParameterParser.TryParse(input, workflow);

            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.task_id }} == '2dbd1af7-b699-4467-8e99-05a0c22422b4' AND " +
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.output_dir }} == 'output/dir' AND " +
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.status }} == 'Succeeded' AND " +
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.start_time }} == '25/12/2022 00:00:00' AND " +
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.end_time }} == '25/12/2022 01:00:00' AND " +
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.execution_id }} == '3c4484bd-e1a4-4347-902e-31a6503edd5f'", true)]
        public void ConditionalParameterParser_WhenGivenCorrectExecutionString_ShouldEvaluate(string input, bool expectedResult)
        {
            var testData = CreateTestData();
            var workflow = testData.First();
            workflow.BucketId = "bucket1";

            var conditionalParameterParser = new ConditionalParameterParser(_logger.Object, _dicom.Object, _workflowInstanceService.Object, _payloadService.Object, _workflowService.Object);

            var actualResult = conditionalParameterParser.TryParse(input, workflow);

            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("{{ context.input.patient_details.id }}", "'patientid'")]
        [InlineData("{{ context.input.patient_details.name }}", "'patientname'")]
        [InlineData("{{ context.input.patient_details.sex }}", "'patientsex'")]
        [InlineData("{{ context.input.patient_details.dob }}", "'19/10/2000'")]
        [InlineData("{{ context.input.patient_details.age }}", "'32'")]
        [InlineData("{{ context.input.patient_details.hospital_id }}", "'patienthospitalid'")]
        [InlineData("{{ context.workflow.name }}", "'workflowname'")]
        [InlineData("{{ context.workflow.description }}", "'workflow description'")]
        public void ResolveParametersWhenGivenPatientDetailsString_ShouldReturnValue(string input, string expectedResult)
        {
            var testData = CreateTestData();
            var workflow = testData.First();

            var payload = new Payload
            {
                PayloadId = "workflow1payload1",
                PatientDetails = new PatientDetails
                {
                    PatientDob = new DateTime(2000, 10, 19, 0, 0, 0, kind: DateTimeKind.Utc),
                    PatientId = "patientid",
                    PatientName = "patientname",
                    PatientSex = "patientsex",
                    PatientAge = "32",
                    PatientHospitalId = "patienthospitalid"
                }
            };

            var workflowRevision = new WorkflowRevision
            {
                Workflow = new Workflow
                {
                    Name = "workflowname",
                    Description = "workflow description"
                }
            };

            _payloadService.Setup(w => w.GetByIdAsync(payload.PayloadId))
                .ReturnsAsync(() => payload);

            _workflowService.Setup(w => w.GetAsync("workflow1")).ReturnsAsync(workflowRevision);

            var conditionalParameterParser = new ConditionalParameterParser(_logger.Object, _dicom.Object, _workflowInstanceService.Object, _payloadService.Object, _workflowService.Object);
            var actualResult = conditionalParameterParser.ResolveParameters(input, workflow);

            Assert.Equal(expectedResult, actualResult);
        }

        public List<WorkflowInstance> CreateTestData()
        {
            return new List<WorkflowInstance>()
            {
                new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = "workflow1",
                    PayloadId = "workflow1payload1",
                    StartTime = DateTime.UtcNow,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "a", "b" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = "3c4484bd-e1a4-4347-902e-31a6503edd5f",
                            TaskId = "2dbd1af7-b699-4467-8e99-05a0c22422b4",
                            TaskType = "Multi_task",
                            OutputDirectory = "output/dir",
                            TaskStartTime = new DateTime(2022, 12, 25, 0, 0, 0, DateTimeKind.Utc),
                            TaskEndTime = new DateTime(2022, 12, 25, 1, 0, 0, DateTimeKind.Utc),
                            Status = TaskExecutionStatus.Succeeded,
                            ResultMetadata = new Dictionary<string, object>()
                            {
                                { "Fred", "Bob" },
                                { "fred", "lowercasefred" },
                                { "Sandra", "YassQueen" },
                            },
                            ExecutionStats = new Dictionary<string, string>()
                            {
                                { "stat1", "completed in 1 hour" },
                                { "stat2", "ran successfully" }
                            }
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "other_task",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Succeeded,
                            ResultMetadata = new Dictionary<string, object>()
                            {
                                { "Fred", "55" },
                                { "fred", "other lowercasefred" },
                                { "Sandra", "other YassQueen" },
                                { "Derick", "lordge" },
                            }
                        }
                    }
                }

                #region Extra Test Data

                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = "Multi_Req",
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Dispatched").WorkflowRevision.WorkflowId,
                //    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Dispatched").WorkflowRequestMessage.PayloadId.ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = "7d7c8b83-6628-413c-9912-a89314e5e2d5",
                //            TaskType = "Multi_task",
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Task_Status_Update_Workflow").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Task_Status_Update_Workflow").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Task_Status_Update_Workflow").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Task_Status_Update_Workflow").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = "Task_Update_9",
                //    WorkflowId = Guid.NewGuid().ToString(),
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Guid.NewGuid().ToString(),
                //            TaskType = "Multi_task_5",
                //            Status = TaskExecutionStatus.Succeeded
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = "Task_Update_10",
                //    WorkflowId = Guid.NewGuid().ToString(),
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Guid.NewGuid().ToString(),
                //            TaskType = "Multi_task_6",
                //            Status = TaskExecutionStatus.Failed
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = "Task_Update_11",
                //    WorkflowId = Guid.NewGuid().ToString(),
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Guid.NewGuid().ToString(),
                //            TaskType = "Multi_task_7",
                //            Status = TaskExecutionStatus.Canceled
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_2").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_2").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_2").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_2").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_3").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_3").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_3").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_3").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        },
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        },
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Type,
                //            Status = TaskExecutionStatus.Accepted
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        },
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Type,
                //            Status = TaskExecutionStatus.Succeeded
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.UtcNow,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //}

                #endregion Extra Test Data
            };
        }
    }
}
