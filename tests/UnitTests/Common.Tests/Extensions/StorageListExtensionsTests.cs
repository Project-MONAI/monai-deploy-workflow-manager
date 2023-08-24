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
    public class StorageListExtensionsTests
    {
        [Fact]
        public void ToArtifactDictionary_ValidStorageList_GeneratesDictionary()
        {
            var storageList = new List<Messaging.Common.Storage>
            {
                new Messaging.Common.Storage
                {
                    Name = "test",
                    RelativeRootPath = "payloadid/dcm"
                },
                new Messaging.Common.Storage
                {
                    Name = "test2",
                    RelativeRootPath = "payloadid/dcm"
                }
            };

            var expected = new Dictionary<string, string>
            {
                { "test", "payloadid/dcm" },
                { "test2", "payloadid/dcm" }
            };

            var artifactDict = storageList.ToArtifactDictionary();

            Assert.Equal(expected, artifactDict);
        }

        [Fact]
        public void ToArtifactDictionary_MissingFields_ReturnsEmpty()
        {
            var storageList = new List<Messaging.Common.Storage>
            {
                new Messaging.Common.Storage
                {
                    Endpoint = "test"
                }
            };

            var expected = new Dictionary<string, string>();

            var artifactDict = storageList.ToArtifactDictionary();

            Assert.Equal(expected, artifactDict);
        }

        [Fact]
        public void ToArtifactDictionary_EmptyList_ReturnsEmpty()
        {
            var storageList = new List<Messaging.Common.Storage>();

            var expected = new Dictionary<string, string>();

            var artifactDict = storageList.ToArtifactDictionary();

            Assert.Equal(expected, artifactDict);
        }
    }
}
