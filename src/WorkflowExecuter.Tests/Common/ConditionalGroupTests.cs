// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Xunit;

namespace Monai.Deploy.WorkflowManager.WorkflowExecuter.Tests.Common
{
    public class ConditionalGroupTests
    {
        //[Theory]
        //[InlineData(false, "AND {{context.dicom.tags[('0010','0040')]}} == 'F'")]
        //[InlineData(true, "{{context.dicom.tags[('0010','0040')]}} == 'F'")]
        ////[InlineData(-4, -6, -10)]
        ////[InlineData(-2, 2, 0)]
        ////[InlineData(int.MinValue, -1, int.MaxValue)]
        //public void ConditionalGroup_CreatesAndEvaluates(bool expected, string input)
        //{
        //    var conditional = ConditionalGroup.Create(input);
        //    var result = conditional.Evaluate();
        //    Assert.Equal(expected, result);
        //}

        [Theory]
        [InlineData("{{context.dicom.tags[('0010','0040')]}}", "F", "{{context.dicom.tags[('0010','0040')]}} == 'F'")]
        public void ConditionalGroup_CreatesAndEvaluates(string expectedLeftParam, string expectedRightParam, string input)
        {
            var conditional = ConditionalGroup.Create(input);
            var leftParameter = conditional.LeftParameter;
            var rightParameter = conditional.RightParameter;

            Assert.Equal(expectedLeftParam, leftParameter);
            Assert.Equal(expectedRightParam, rightParameter);
        }

        [Theory]
        [InlineData("AND {{context.dicom.tags[('0010','0040')]}} == 'F'", "No left hand parameter at index: 0")]
        public void ConditionalGroup_CreatesAndEvaluates_ErrorsOnInvalidInput(string input, string expectedErrorMessage)
        {
            Assert.Throws<ArgumentException>(expectedErrorMessage, () => ConditionalGroup.Create(input));
        }
    }
}
