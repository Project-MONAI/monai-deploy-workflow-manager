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

using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Extensions;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous.Tests.Extensions
{
    public class FileExtensionsTests
    {
        [Fact]
        public void IsValidDicomFile_ValidLowerCaseExtension_ReturnsTrue()
        {
            var fileInfo = new VirtualFileInfo("dicomfile.dcm", "folder/folder2/dicomfile.dcm", "tag", 222);

            var result = fileInfo.IsValidDicomFile();

            Assert.True(result);
        }

        [Fact]
        public void IsValidDicomFile_ValidUpperCaseExtension_ReturnsTrue()
        {
            var fileInfo = new VirtualFileInfo("DicomFile.DCM", "folder/folder2/dicomfile.DCM", "tag", 222);

            var result = fileInfo.IsValidDicomFile();

            Assert.True(result);
        }

        [Fact]
        public void IsValidDicomFile_ValidUnknownExtension_ReturnsFalse()
        {
            var fileInfo = new VirtualFileInfo("DicomFile", "folder/folder2/dicomfile", "tag", 222);

            var result = fileInfo.IsValidDicomFile();

            Assert.False(result);
        }

        [Fact]
        public void IsValidDicomFile_InValidPdfExtension_ReturnsFalse()
        {
            var fileInfo = new VirtualFileInfo("DicomFile.pdf", "folder/folder2/dicomfile.pdf", "tag", 222);

            var result = fileInfo.IsValidDicomFile();

            Assert.False(result);
        }
    }
}
