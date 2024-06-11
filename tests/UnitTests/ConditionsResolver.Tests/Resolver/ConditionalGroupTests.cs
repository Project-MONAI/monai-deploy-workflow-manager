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

namespace Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Tests.Resolver
{
    public class ConditionalGroupTests
    {
        [Theory]
        [InlineData("'F' == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData("'F' == 'F' OR 'F' == 'leg'")]
        [InlineData("'F' == 'F' AND 'F' == 'leg'")]
        [InlineData("'AND' == 'F' OR 'F' == 'leg'")]
        [InlineData("'OR' == 'F' OR 'F' == 'leg'")]
        [InlineData("'F' == 'F' or 'F' == 'leg'")] // Lowercase OR
        [InlineData("'F' == 'F' and 'F' == 'leg'")] // Lowercase AND
        [InlineData("'LEG' == 'F' OR 'F' == 'leg'")]
        [InlineData("'F' == 'F' OR 'F' == 'leg' AND 'F' == 'F'")]
        [InlineData("'F' == 'F' AND 'F' == 'leg' AND 'F' == 'F'")]
        [InlineData("'F' == 'F' AND 'F' == 'leg' OR 'F' == 'F'")]
        [InlineData("'F' == 'F' OR 'F' == 'leg' OR 'F' == 'F'")]
        [InlineData("'F' == 'F' OR 'F' == 'leg' OR 'F' == 'F' AND 'F' == 'F'")]
        [InlineData("'F' == 'F' OR 'F' == 'leg' OR 'F' == 'F' AND 'F' == 'F' AND 'F' == 'F'")]
        [InlineData("'F' == 'F' AND 'F' == 'leg' OR 'F' == 'F' AND 'F' == 'F' OR 'F' == 'F'")]
        [InlineData("'F' == 'F' AND 'F' == 'leg' OR 'F' == 'F' OR 'F' == 'F' AND 'F' == 'F'")]
        [InlineData("'AND' == 'OR' AND 'F' == 'leg' OR 'F' == 'F' OR 'F' == 'F' AND 'F' == 'F'")]
        [InlineData("'Donkey' CONTAINS [�Donkey�, �Alpaca�, �Zebra�] AND 'F' == 'F'")]
        public void ConditionalGroup_WhenProvidedCorrectInput_ShouldCreateAndHaveLeftAndRightGroups(string input)
        {
            var conditionalGroup = ConditionalGroup.Create(input);
            Assert.True(conditionalGroup.LeftIsSet);
            Assert.True(conditionalGroup.RightIsSet);
        }

        [Theory]
        [InlineData(true, "'F' == 'F'")]
        [InlineData(false, "'F' == 'leg'")]
        [InlineData(true, "'Donkey' CONTAINS [�Donkey�, �Alpaca�, �Zebra�]")]
        [InlineData(true, "'lillie' contains [�jack�, �lillie�, �neil�]")]
        [InlineData(false, "'Donkey' CONTAINS [�aDonkeya�, �Alpaca�, �Zebra�]")]
        [InlineData(true, "[�Donkey�, �Alpaca�, �Zebra�] CONTAINS 'Donkey'")]
        [InlineData(false, "[�Donkey�, �Alpaca�, �Zebra�] CONTAINS 'Betty'")]
        [InlineData(false, "'Donkey' NOT_CONTAINS [�Donkey�, �Alpaca�, �Zebra�]")]
        [InlineData(false, "'Donkey' not_contains [�Donkey�, �Alpaca�, �Zebra�]")]
        [InlineData(true, "'' == NULL")]
        [InlineData(true, "'Lillie' == 'lillie '")]
        [InlineData(true, "'Lillie' == 'lillie'")]
        [InlineData(false, "'donkey' == NULL")]
        [InlineData(true, "null == ''")]
        [InlineData(true, "UNDEFINED == ''")]
        [InlineData(true, "NULL == ''")]
        [InlineData(false, "'bNULL' == ''")]
        [InlineData(false, "'NULLb' == ''")]
        [InlineData(false, "NULL == 'nullb'")]
        [InlineData(false, "NULL == 'bnull'")]
        public void ConditionalGroup_WhenSetSingularConditional_ShouldCreateAndEvaluate(bool expectedEvaluation, string input)
        {
            var conditionalGroup = ConditionalGroup.Create(input);
            Assert.True(conditionalGroup.LeftIsSet);
            Assert.False(conditionalGroup.RightIsSet);
            var result = conditionalGroup.Evaluate();
            Assert.Equal(expectedEvaluation, result);
        }

        [Theory]
        [InlineData(true, "'F' == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")] // this is great doesn't even evaluate right hand param because left hand side passes!
        [InlineData(true, "'F' == 'F' OR 'F' == 'leg'")]
        [InlineData(true, "'LEG' == 'F' OR 'leg' == 'leg'")]
        [InlineData(true, "'1' == '1' OR 'donkey' == 'leg'")]
        [InlineData(true, "'5' > '1' AND 'donkey' == 'donkey'")]
        [InlineData(false, "'5' < '1' AND 'donkey' == 'donkey'")] // 5 less than 1
        [InlineData(true, "'5' > '1' AND 'Donkey' == 'donkey'")] // capital D in donkey
        [InlineData(true, "'5' => '5' AND 'Donkey' == 'donkey'")]
        [InlineData(true, "'5' <= '5' AND 'Donkey' == 'donkey'")]
        [InlineData(true, "'5' <= '5' AND 'Donkey' == 'Donkey'")]
        [InlineData(false, "'5' >= '5' AND 'Donkey' != 'donkey'")]
        [InlineData(true, "'Jack' CONTAINS [\"Lillie\", \"Jack\", \"Lucy\"] AND 'Donkey' == 'donkey'")]
        [InlineData(false, "'ill' CONTAINS [\"Lillie\", \"Billy\", \"Silly\"] AND 'Donkey' == 'donkey'")]
        [InlineData(true, "NULL CONTAINS [\"Lillie\", NULL, \"Silly\"] AND 'Donkey' == 'donkey'")]
        [InlineData(true, "NULL CONTAINS [\"Lillie\", Null, \"Silly\"] AND 'Donkey' == 'donkey'")]
        [InlineData(true, "'Donkey' == 'donkey' AND NULL CONTAINS [\"Lillie\", Null, \"Silly\"]")]
        public void ConditionalGroup_WhenProvidedCorrectInput_ShouldCreateAndEvaluate(bool expectedResult, string input)
        {
            var conditionalGroup = ConditionalGroup.Create(input);
            Assert.NotNull(conditionalGroup.LeftConditional);
            Assert.NotNull(conditionalGroup.RightConditional);
            var result = conditionalGroup.Evaluate();
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true, "('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE')")]
        [InlineData(true, "'TRUE' == 'TRUE' AND ('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE')")]
        [InlineData(true, "'TRUE' == 'TRUE' AND ('Falsee' == 'FLASE' OR 'TRUE' == 'TRUE')")]
        [InlineData(false, "('Falsee' == 'FLASE' AND 'TRUE' == 'TRUE') AND ('Falsee' == 'FLASE' OR 'TRUE' == 'TRUE')")]
        [InlineData(true, "('TRUE' == 'TRUE' AND ('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE'))")]
        [InlineData(true, "('TRUE' == 'TRUE' OR ('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE'))")]
        [InlineData(true, "('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE') OR ('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE')")]
        [InlineData(true, "('TRUE' == 'TRUE' AND 'TRUE' == 'TRUE' AND 'TRUE' == 'TRUE') AND ('TRUE' == 'TRUE' AND 'TRUE' == 'TRUE')")]
        [InlineData(true, "('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE' OR 'TRUE' == 'TRUE') OR ('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE')")]
        [InlineData(true, "('TRUE' == 'TRUE' OR '(TRUE' == 'TRUE' OR 'TRUE' == 'TRUE') OR 'TRUE' == 'TRUE') OR ('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE')")]
        [InlineData(true, "('TRUE' == 'TRUE' AND ('TRUE' == 'TRUE' AND 'TRUE' == 'TRUE') AND 'TRUE' == 'TRUE') AND ('TRUE' == 'TRUE' AND 'TRUE' == 'TRUE')")]
        [InlineData(true, "'Donkey' == 'donkey' AND NULL CONTAINS [\"Lillie\", Null, \"Silly\"] AND 'Donkey' == 'donkey'")]
        public void ConditionalGroup_WhenProvidedCorrectinputWithBrackets_ShouldCreateAndEvaluate(bool expectedResult, string input)
        {
            var conditionalGroup = ConditionalGroup.Create(input);
            Assert.True(conditionalGroup.LeftIsSet);
            Assert.True(conditionalGroup.RightIsSet);
            var result = conditionalGroup.Evaluate();
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("Unable to parse \"NULLLL == ''\"", "NULLLL == ''")]
        [InlineData("Matching brackets missing.", "('TRUE' == 'TRUE' AND ('TRUE' == 'TRUE' OR ('TRUE' == 'TRUE' AND ((('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE'))))")]
        public void ConditionalGroup_WhenErrorHappens_ShouldShowsCorrectError(string expectedMessage, string input)
        {
            var exception = Assert.Throws<ArgumentException>(() => ConditionalGroup.Create(input));

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Theory]
        [InlineData("Matching brackets missing.", "('TRUE' == 'TRUE' AND ('TRUE' == 'TRUE' OR ('TRUE' == 'TRUE' AND ((('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE'))))")]
        public void ConditionalGroup_WhenParsingErrorHappens_ShouldShowsCorrectError(string expectedMessage, string input)
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                var conditionalGroup = new ConditionalGroup();
                conditionalGroup.GroupedLogical = 2;
                conditionalGroup.Parse(input);
            });

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Theory]
        [InlineData(true, "'TRUE' == 'TRUE' OR 'TRUE' == 'TRUE'", "'TRUE' == 'TRUE' OR 'TRUE' == 'TRUE'", Keyword.And)]
        public void ConditionalGroup_WhenSetProvidedCorrectinput_ShouldCreateAndEvaluate(bool expectedResult, string inputLeft, string inputRight, Keyword keyword)
        {
            var conditionalGroup = new ConditionalGroup();
            conditionalGroup.Set(inputLeft, inputRight, keyword);

            Assert.True(conditionalGroup.LeftIsSet);
            Assert.True(conditionalGroup.RightIsSet);

            var result = conditionalGroup.Evaluate();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConditionalGroup_GivenConditionalWithLargeInt_Evaluate()
        {
            var val1 = decimal.MaxValue;
            var val2 = decimal.MinValue;
            var leftEquation = $"'{val1}' >= '{val2}'";
            var conditionalGroup = new ConditionalGroup();
            conditionalGroup.Parse(leftEquation);
            Assert.True(conditionalGroup.Evaluate());
        }

        [Fact]
        public void ConditionalGroup_GivenConditionalGroup_ShouldThrowException()
        {
            var expectedMessage = "Evaluation Error";
            var exception = Assert.Throws<InvalidOperationException>(() => new ConditionalGroup().Evaluate());
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void ConditionalGroup_GivenConditionalGroupParseBrackets_ShouldThrowException()
        {
            var expectedMessage = "Expected Bracket: Bracket not found";
            var exception = Assert.Throws<InvalidOperationException>(() => new ConditionalGroup().ParseBrackets("'f' == 'f'"));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void ConditionalGroup_GivenEmptyStringConditionalGroup_ShouldThrowException()
        {
            var expectedMessage = "The value cannot be an empty string. (Parameter 'input')";
            var exception = Assert.Throws<ArgumentException>(() => ConditionalGroup.Create(""));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void ConditionalGroup_GivenEmptyStringConditionalGroupParse_ShouldThrowException()
        {
            var expectedMessage = "The value cannot be an empty string. (Parameter 'input')";
            var exception = Assert.Throws<ArgumentException>(() => new ConditionalGroup().Parse(""));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void ConditionalGroup_GivenEmptyStringConditionalGroupParseBrackets_ShouldThrowException()
        {
            var expectedMessage = "The value cannot be an empty string or composed entirely of whitespace. (Parameter 'input')";
            var exception = Assert.Throws<ArgumentException>(() => new ConditionalGroup().ParseBrackets(""));
            Assert.Equal(expectedMessage, exception.Message);
        }
    }
}
