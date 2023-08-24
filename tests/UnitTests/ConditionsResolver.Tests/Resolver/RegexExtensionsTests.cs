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
using System.Text.RegularExpressions;
using Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Extensions;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Tests.Resolver
{
    public class RegexExtensionsTests
    {
        [Theory]
        [InlineData(new string[] { "test", "(test" }, "test(test")]
        [InlineData(new string[] { "test", "( test" }, "test( test")]
        [InlineData(new string[] { "test ", "( test" }, "test ( test")]
        [InlineData(new string[] { "test ", "( test( test" }, "test ( test( test")]
        public void Regex_WhenSplitOnce_ShouldOnlyHaveArrayOfTwo(string[] expected, string stringToSplit)
        {
            var regexFindBrackets = new Regex(@"((?<!\[)\()", RegexOptions.None, matchTimeout: TimeSpan.FromSeconds(2));
            var result = regexFindBrackets.SplitOnce(stringToSplit);
            Assert.Equal(expected, result);
            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void Regex_WhenSplitOnceProvidedNullInput_ShouldThrowException()
        {
            var expectedErrorMessage = "Value cannot be null. (Parameter 'input')";
            var regexFindBrackets = new Regex(@"((?<!\[)\()", RegexOptions.None, matchTimeout: TimeSpan.FromSeconds(2));
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var exception = Assert.Throws<ArgumentNullException>(() => regexFindBrackets.SplitOnce(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            var message = exception.Message;
            Assert.Equal(expectedErrorMessage, message);
        }
    }
}
