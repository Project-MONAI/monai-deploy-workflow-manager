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
using Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Extensions;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Tests.Resolver
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("(test", "test(test")]
        [InlineData("(test", "test (test")]
        [InlineData("donkey(test", "donkey(test")]
        [InlineData("(test", "testtest(test")]
        [InlineData("(test", "test test(test")]
        [InlineData("(test", "tESt TEST tesT test test(test")]
        public void String_WhenTrimStartExtWithTest_ShouldOnlyRemoveTestFromStart(string expected, string input)
        {
            var result = input.TrimStartExt("test");
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("(test", "test(test")]
        [InlineData("(test", "test (test")]
        [InlineData("donkey(test", "donkey(test")]
        [InlineData("(test", "testtest(test")]
        [InlineData("(test", "test test(test")]
        [InlineData("tESt TEST tesT test test(test", "tESt TEST tesT test test(test")]
        public void String_WhenTrimStartExtWithTestAndSpecifiyCurrentCulture_ShouldOnlyRemoveTestFromStart(string expected, string input)
        {
            var result = input.TrimStartExt("test", StringComparison.CurrentCulture);
            Assert.Equal(expected, result);
        }
    }
}
