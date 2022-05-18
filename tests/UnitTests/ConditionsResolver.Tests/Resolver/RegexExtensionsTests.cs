// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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
    }
}
