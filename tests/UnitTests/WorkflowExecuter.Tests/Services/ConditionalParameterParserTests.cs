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
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Parser;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Storage.Services;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Tests.Services
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
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.Fred }} == 'Bob'",
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.fred }} == 'lowercasefred'" },
            true,
            "lordge",
            "('lordge' == 'lordge') AND ('Bob' == 'Bob') AND ('lowercasefred' == 'lowercasefred')")]
        [InlineData(
            new string[] {"{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.Fred }} == 'Bob'",
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.fred }} == 'lowercasefred'",
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.Sandra }} == 'YassQueen' OR " +
            "{{ context.executions.other_task.result.Fred }} >= '32' OR " +
            "{{ context.executions.other_task.result.Sandra }} == 'other YassQueen' OR " +
            "{{ context.executions.other_task.result.Derick }} == 'lordge'" },
            true,
            null,
            "('Bob' == 'Bob') AND ('lowercasefred' == 'lowercasefred') AND ('YassQueen' == 'YassQueen' OR '55' >= '32' OR 'other YassQueen' == 'other YassQueen' OR 'lordge' == 'lordge')")]
        [InlineData(
            new string[] {"{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.inttest }} == '2.5'",
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.booltest }} == 'True'",
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.result.datetest }} == '2022-12-05T14:06:34'"},
            true,
            null,
            "('2.5' == '2.5') AND ('True' == 'True') AND ('2022-12-05T14:06:34' == '2022-12-05T14:06:34')")]
        [InlineData(
            new string[] {"{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.execution_stats.stat1 }} == 'completed in 1 hour' AND " +
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.execution_stats.stat2 }} ==  'ran successfully'"},
            true,
            null,
            "'completed in 1 hour' == 'completed in 1 hour' AND 'ran successfully' ==  'ran successfully'")]
        public void ConditionalParameterParser_WhenGivenCorrectResultMetadataString_MultiConditionShouldEvaluate(string[] input, bool expectedResult, string? expectedDicomReturn, string expectedResolvedConditional)
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

            var actualResult = conditionalParameterParser.TryParse(input, workflow, out var resolvedConditional);

            Assert.Equal(expectedResolvedConditional, resolvedConditional);
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("{{ context.dicom.series.any('0010','0040') }} == 'lordge'", true, "lordge", "'lordge' == 'lordge'")]
        [InlineData("{{ context.dicom.series.all('0010','0040') }} == 'lordge'", true, "lordge", "'lordge' == 'lordge'")]
        [InlineData("'invalid' > 'false'", false, "lordge", "'invalid' > 'false'")]
        [InlineData("{{ context.dicom.series.any('0018','0050') }} >= '1.25000'", true, "2.25000", "'2.25000' >= '1.25000'")]
        [InlineData("{{ context.dicom.series.any('0018','0050') }} == '1.25000'", true, "1.25000", "'1.25000' == '1.25000'")]
        [InlineData("{{ context.dicom.series.any('0018','0050') }} <= '0.25000'", false, "1.25000", "'1.25000' <= '0.25000'")]
        [InlineData("{{ context.dicom.series.any('0018','0050') }} <= '1.25000'", true, "0.25000", "'0.25000' <= '1.25000'")]
        [InlineData("{{ context.dicom.series.any('0018','0050') }} <= '0.25000'", true, "0.24000", "'0.24000' <= '0.25000'")]
        [InlineData("{{ context.dicom.series.any('0018','0050') }} < '0.25000'", true, "0.24000", "'0.24000' < '0.25000'")]
        [InlineData("{{ context.dicom.series.any('0018','0050') }} CONTAINS '5'", true, "['2','5','3']", "['2','5','3'] CONTAINS '5'")]
        [InlineData("{{ context.dicom.series.any('0018','0050') }} CONTAINS '6'", false, "['2','5','3']", "['2','5','3'] CONTAINS '6'")]
        [InlineData("{{ context.dicom.series.any('0018','0050') }} CONTAINS '0.6'", true, "['2','5','3','0.6']", "['2','5','3','0.6'] CONTAINS '0.6'")]
        [InlineData("{{ context.dicom.series.any('0018','0050') }} CONTAINS ['100','0.6']", true, "['2','5','3','0.6']", "['2','5','3','0.6'] CONTAINS ['100','0.6']")]
        public void ConditionalParameterParser_WhenGivenCorrectDicomString_ShouldEvaluate(string input, bool expectedResult, string? expectedDicomReturn, string expectedResolvedConditional)
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

            var actualResult = conditionalParameterParser.TryParse(input, workflow, out var resolvedConditional);

            Assert.Equal(expectedResolvedConditional, resolvedConditional);
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData(
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.task_id }} == '2dbd1af7-b699-4467-8e99-05a0c22422b4' AND " +
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.output_dir }} == 'output/dir' AND " +
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.status }} == 'Succeeded' AND " +
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.start_time }} == '2022-12-25T00:00:00' AND " +
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.end_time }} == '2022-12-25T01:00:00' AND " +
            "{{ context.executions.2dbd1af7-b699-4467-8e99-05a0c22422b4.execution_id }} == '3c4484bd-e1a4-4347-902e-31a6503edd5f'", true,
            "'2dbd1af7-b699-4467-8e99-05a0c22422b4' == '2dbd1af7-b699-4467-8e99-05a0c22422b4' AND 'output/dir' == 'output/dir' AND 'Succeeded' == 'Succeeded' AND '2022-12-25T00:00:00' == '2022-12-25T00:00:00' AND '2022-12-25T01:00:00' == '2022-12-25T01:00:00' AND '3c4484bd-e1a4-4347-902e-31a6503edd5f' == '3c4484bd-e1a4-4347-902e-31a6503edd5f'")]
        public void ConditionalParameterParser_WhenGivenCorrectExecutionString_ShouldEvaluate(string input, bool expectedResult, string expectedResolvedConditional)
        {
            var testData = CreateTestData();
            var workflow = testData.First();
            workflow.BucketId = "bucket1";

            var conditionalParameterParser = new ConditionalParameterParser(_logger.Object, _dicom.Object, _workflowInstanceService.Object, _payloadService.Object, _workflowService.Object);

            var actualResult = conditionalParameterParser.TryParse(input, workflow, out var resolvedConditional);
            Assert.Equal(expectedResolvedConditional, resolvedConditional);
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
                                { "booltest", true },
                                { "datetest", new DateTime(2022,12,05,14,06,34) },
                                { "inttest", 2.5 },
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
                                { "Derick", "lordge" }
                            }
                        }
                    }
                }
            };
        }
    }
}
