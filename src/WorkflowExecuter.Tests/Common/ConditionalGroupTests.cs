// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Xunit;

namespace Monai.Deploy.WorkflowManager.WorkflowExecuter.Tests.Common
{
    public class ConditionalGroupTests
    {
        //TODO: Multiple AND/OR Keywords
        //TODO: Error Conditions AND/OR Keywords
        //TODO: Test Operators ('==') AND/OR Keywords
        //TODO: Parse Parameters
        //TODO: Test reversed parameters 'F' on left and {{}} on right

        [Theory]
        [InlineData("{{context.dicom.tags[('0010','0040')]}} == 'F' AND {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData("{{context.dicom.tags[('0010','0040')]}} == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        public void ConditionalGroup_Creates_HasLeftAndRightGroups(string input)
        {
            var conditionalGroup = ConditionalGroup.Create(input);
            Assert.NotNull(conditionalGroup.LeftGroup);
            Assert.NotNull(conditionalGroup.RightGroup);
        }

        [Theory]
        [InlineData("{{context.dicom.tags[('0010','0040')]}}", "F", "{{context.executions.body_part_identifier.result.body_part}}", "leg", "{{context.dicom.tags[('0010','0040')]}} == 'F' AND {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData("{{context.dicom.tags[('0010','0040')]}}", "F", "{{context.executions.body_part_identifier.result.body_part}}", "leg", "{{context.dicom.tags[('0010','0040')]}} == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        public void ConditionalGroup_Creates_HasLeftAndRightGroupsWithValues(string leftGroupLeftParam,
                                                                             string leftGroupRightParam,
                                                                             string rightGroupLeftParam,
                                                                             string rightGroupRightParam,
                                                                             string input)
        {
            var conditionalGroup = ConditionalGroup.Create(input);
            Assert.NotNull(conditionalGroup.LeftGroup);
            Assert.NotNull(conditionalGroup.RightGroup);
        }

        //[InlineData(false, "AND {{context.dicom.tags[('0010','0040')]}} == 'F'")]
        //[InlineData(true, "{{context.dicom.tags[('0010','0040')]}} == 'F'")]
        ////[InlineData(-4, -6, -10)]
        ////[InlineData(-2, 2, 0)]
        ////[InlineData(int.MinValue, -1, int.MaxValue)]
        //{
        //    var conditional = ConditionalGroup.Create(input);
        //    var result = conditional.Evaluate();
        //    Assert.Equal(expected, result);
        //}

        [Theory]
        [InlineData("{{context.dicom.tags[('0010','0040')]}}", "F", "{{context.dicom.tags[('0010','0040')]}} == 'F'")]
        [InlineData("{{context.executions.body_part_identifier.result.body_part}}", "leg", "{{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        public void Conditional_CreatesAndEvaluates(string expectedLeftParam, string expectedRightParam, string input)
        {
            var conditional = Conditional.Create(input);
            var leftParameter = conditional.LeftParameter;
            var rightParameter = conditional.RightParameter;

            Assert.Equal(expectedLeftParam, leftParameter);
            Assert.Equal(expectedRightParam, rightParameter);
        }

        [Theory]
        [InlineData("AND {{context.dicom.tags[('0010','0040')]}} == 'F'", "No left hand parameter at index: 0")]
        [InlineData(" AND {{context.dicom.tags[('0010','0040')]}} == 'F'", "No left hand parameter at index: 0")]
        //[InlineData("OR {{context.dicom.tags[('0010','0040')]}} == 'F'", "No left hand parameter at index: 0")]
        public void Conditional_CreatesAndEvaluates_ErrorsOnInvalidInput(string input, string expectedErrorMessage)
        {
            var exception = Assert.Throws<ArgumentException>(() => Conditional.Create(input));

            Assert.Equal(expectedErrorMessage, exception.Message);
        }
    }
}
