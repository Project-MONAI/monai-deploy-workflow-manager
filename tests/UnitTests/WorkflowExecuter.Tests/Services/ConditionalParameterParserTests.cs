// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Common;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Services;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.WorkflowExecuter.Tests.Services
{
    public class ConditionalParameterParserTests
    {
        private readonly Mock<ILogger<WorkflowExecuterService>>? _logger;

        public ConditionalParameterParserTests()
        {
            _logger = new Mock<ILogger<WorkflowExecuterService>>();
        }

        [Theory]
        //[InlineData(false, "{{context.dicom.tags[('0010','0040')]}} == 'F' AND {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        //[InlineData(false, "{{context.dicom.tags[('0010','0040')]}} == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData("{{context.executions.body_part_identifier.result.body_part}} == 'match 1'", true)]
        [InlineData("{{context.executions.body_part_identifier.result.body_part}} == 'match 1' OR " +
            "{{context.executions.body_part_identifier.result.body_part}} == 'match 2' AND " +
            "{{context.executions.body_part_identifier.result.body_part}} == 'match 3' OR " +
            "{{context.executions.body_part_identifier.result.body_part}} == 'match 4' OR " +
            "{{context.executions.body_part_identifier.result.body_part}} == 'match 5' OR " +
            "{{context.executions.body_part_identifier.result.body_part}} == 'match 6' AND " +
            "'match 7' == {{context.executions.body_part_identifier.result.body_part}}", true)]
        public async Task ConditionalParameterParser_WhenGivenCorrectString_ShouldEvaluate(string input, bool expectedResult)
        {
            var conditionalParameterParser = new ConditionalParameterParser(_logger.Object);
            var actualResult = conditionalParameterParser.TryParse(input);

            Assert.Equal(expectedResult, actualResult);
        }
    }
}
