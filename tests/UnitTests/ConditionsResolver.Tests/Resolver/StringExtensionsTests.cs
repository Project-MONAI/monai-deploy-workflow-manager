using System;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Extensions;
using Xunit;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Tests.Resolver
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

        public void String_WhenTrimStartExtNull_ShouldOnlyRemoveTestFromStart(string expected, string input)
        {
            var result = input.TrimStartExt("test", StringComparison.CurrentCulture);
            Assert.Equal(expected, result);
        }
    }
}
