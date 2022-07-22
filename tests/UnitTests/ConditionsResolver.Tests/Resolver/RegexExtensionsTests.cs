// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Text.RegularExpressions;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Extensions;
using Xunit;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Tests.Resolver
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
            var regexFindBrackets = new Regex(@"((?<!\[)\()");
            var result = regexFindBrackets.SplitOnce(stringToSplit);
            Assert.Equal(expected, result);
            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void Regex_WhenSplitOnceProvidedNullInput_ShouldThrowException()
        {
            var expectedErrorMessage = "Value cannot be null. (Parameter 'input')";
            var regexFindBrackets = new Regex(@"((?<!\[)\()");
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var exception = Assert.Throws<ArgumentNullException>(() => regexFindBrackets.SplitOnce(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            var message = exception.Message;
            Assert.Equal(expectedErrorMessage, message);
        }
    }
}
