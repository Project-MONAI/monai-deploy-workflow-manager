/*
 * Copyright 2023 MONAI Consortium
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

using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Utilities;

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous.Tests
{
    public class DicomTagUtilitiesTests
    {
        [Fact]
        public void GetDicomTagByName_ReturnsDicomTag()
        {
            var tag = "MessageID";

            var dicomTag = DicomTagUtilities.GetDicomTagByName(tag);

            Assert.NotNull(dicomTag);
        }

        [Fact]
        public void GetDicomTagByName_ReturnsNull()
        {
            var tag = "InvalidDicomTag";

            var dicomTag = DicomTagUtilities.GetDicomTagByName(tag);

            Assert.Null(dicomTag);
        }

        [Fact]
        public void DicomTagsValid_ValidTags()
        {
            var tags = new string[] { "MessageID", "Status", "EventTypeID" };

            var (valid, invalidTags) = DicomTagUtilities.DicomTagsValid(tags);

            Assert.True(valid);
            Assert.Empty(invalidTags);
        }

        [Fact]
        public void DicomTagsValid_InvalidTag()
        {
            var tags = new string[] { "MessageID", "Status", "EventTypeID", "InvalidDicomTag" };

            var (valid, invalidTags) = DicomTagUtilities.DicomTagsValid(tags);

            Assert.False(valid);
            Assert.NotEmpty(invalidTags);
            Assert.Single(invalidTags);
        }
    }
}
