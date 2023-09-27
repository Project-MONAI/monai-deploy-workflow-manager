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
using Monai.Deploy.WorkflowManager.ConditionsResolver.Resovler;
using Xunit;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Tests.Resolver
{
    public class ConditionalTests
    {
        [Theory]
        [InlineData("{{context.dicom.tags[('0010','0040')]}}", "F", "{{context.dicom.tags[('0010','0040')]}} == 'F'")]
        [InlineData("{{context.executions.body_part_identifier.result.body_part}}", "leg", "{{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData("F", "F", "'F' == 'F'")]
        [InlineData("F", "{{context.dicom.tags[('0010','0040')]}}", "'F' == {{context.dicom.tags[('0010','0040')]}}")]
        [InlineData("{{context.dicom.tags[('0010','0040')]}}", "�Donkey�, �Alpaca�, �Zebra�", "{{context.dicom.tags[('0010','0040')]}} CONTAINS [�Donkey�, �Alpaca�, �Zebra�]")]
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
        [InlineData("OR {{context.dicom.tags[('0010','0040')]}} == 'F'", "No left hand parameter at index: 0")]
        [InlineData(" OR {{context.dicom.tags[('0010','0040')]}} == 'F'", "No left hand parameter at index: 0")]
        [InlineData("{{context.dicom.tags[('0010','0040')]}} == 'F'  AND ", "No right hand parameter at index: 0")]
        public void Conditional_WhenGivenInvalidInput_ShouldThrowErrors(string input, string expectedErrorMessage)
        {
            var exception = Assert.Throws<ArgumentException>(() => Conditional.Create(input));

            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        [Fact]
        public void Conditional_GivenEmptyStringConditional_ShouldThrowException()
        {
            var expectedMessage = "Value cannot be null. (Parameter 'input')";
            var exception = Assert.Throws<ArgumentNullException>(() => Conditional.Create(""));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Conditional_GivenInvalidNumberStringConditional_ShouldThrowException()
        {
            var expectedMessage = "Invalid numeric value in: 'F' > 'F'";
            var exception = Assert.Throws<ArgumentException>(() => Conditional.Create("'F' > 'F'"));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Conditional_GivenEmptyStringConditionalParse_ShouldThrowException()
        {
            var expectedMessage = "Value cannot be null. (Parameter 'input')";
            var exception = Assert.Throws<ArgumentNullException>(() => new Conditional().Parse(null));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Conditional_GiveNullStringConditionalSetNextParameter_ShouldHaveNULL()
        {
            var conditional = new Conditional();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            conditional.SetNextParameter(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Equal("NULL", conditional.LeftParameter);
        }
    }
}
