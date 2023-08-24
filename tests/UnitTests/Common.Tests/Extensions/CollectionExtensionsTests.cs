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

using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Extensions;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous.Tests.Extensions
{
    public class CollectionExtensionsTests
    {
        [Fact]
        public void CollectionExtension_ListIsNullOrEmpty_ReturnsTrue()
        {
            var expectedResult = true;
            var list = new List<string>();
            Assert.Equal(expectedResult, list.IsNullOrEmpty());
        }

        [Fact]
        public void CollectionExtension_ListWithItemsIsNullOrEmpty_ReturnsFalse()
        {
            var expectedResult = false;
            var list = new List<string>() { "hello" };
            Assert.Equal(expectedResult, list.IsNullOrEmpty());
        }


        [Fact]
        public void CollectionExtension_ListIsNullIsNullOrEmpty_ReturnsTrue()
        {
            var expectedResult = true;
            List<string>? list = null;
#pragma warning disable CS8604 // Possible null reference argument.
            Assert.Equal(expectedResult, list.IsNullOrEmpty());
#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Fact]
        public void CollectionExtension_ListIsNullIsNullOrEmpty_ReturnsNull()
        {
            bool? expectedResult = null;
            List<string>? list = null;
            Assert.Equal(expectedResult, list?.IsNullOrEmpty());
        }

        [Fact]
        public void CollectionExtension_DictionaryAppend_AppendsDictionary()
        {
            var dict1 = new Dictionary<string, string>() { { "one", "a" } };
            var dict2 = new Dictionary<string, string>() { { "two", "b" } };
            var expectedResult = new Dictionary<string, string>()
            {
                { "one", "a" },
                { "two", "b" }
            };
            dict1.Append(dict2);
            Assert.Equal(expectedResult, dict1);
        }

        [Fact]
        public void CollectionExtension_DictionaryAppendEmptyDict_AppendsDictionary()
        {
            var dict1 = new Dictionary<string, string>() { { "one", "a" } };
            var dict2 = new Dictionary<string, string>();
            var expectedResult = new Dictionary<string, string>()
            {
                { "one", "a" },
            };
            dict1.Append(dict2);
            Assert.Equal(expectedResult, dict1);
        }

        [Fact]
        public void CollectionExtension_DictionaryAppendNullDict_AppendsDictionary()
        {
            var dict1 = new Dictionary<string, string>() { { "one", "a" } };
            Dictionary<string, string>? dict2 = null;
            var expectedResult = new Dictionary<string, string>()
            {
                { "one", "a" },
            };
#pragma warning disable CS8604 // Possible null reference argument.
            dict1.Append(dict2);
#pragma warning restore CS8604 // Possible null reference argument.
            Assert.Equal(expectedResult, dict1);
        }

        [Fact]
        public void CollectionExtension_DictionaryAppendToNullDict_AppendsDictionary()
        {
            Dictionary<string, string>? dict1 = null;
            var dict2 = new Dictionary<string, string>() { { "two", "b" } };

            var ex = Assert.Throws<ArgumentNullException>(() => dict1!.Append(dict2));
            Assert.Equal("Value cannot be null. (Parameter 'array')", ex.Message);
        }
    }
}
