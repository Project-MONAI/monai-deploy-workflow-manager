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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Monai.Deploy.Storage.API;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Storage.Services;
using Moq;
using Xunit;
using System.Text;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Storage.Constants;
using Newtonsoft.Json;
using System.IO;

namespace Monai.Deploy.WorkflowManager.Storage.Tests.Services
{
    public class DicomServiceTests
    {
        private IDicomService DicomService { get; set; }

        private readonly Mock<IStorageService> _storageService;

        private readonly Mock<ILogger<DicomService>> _logger;

        public DicomServiceTests()
        {
            _storageService = new Mock<IStorageService>();
            _logger = new Mock<ILogger<DicomService>>();

            DicomService = new DicomService(_storageService.Object, _logger.Object);
        }

        [Fact]
        public void GetDicomPathsForTask_NullInput_ThrowsException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.ThrowsAsync<ArgumentNullException>(async () => await DicomService.GetDicomPathsForTaskAsync(null, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Fact]
        public async Task GetDicomPathsForTask_MultipleValidFiles_ReturnsPaths()
        {
            var bucketName = "bucket";
            var outputDir = "output/dir";

            var expected = new List<string>
            {
                "/test/folder/dicom.dcm",
                "/dicom2.dcm"
            };

            var returnedFiles = new List<VirtualFileInfo>
            {
                new VirtualFileInfo("filename", "/test/folder/dicom.dcm", "tag", 500),
                new VirtualFileInfo("filename", "/dicom2.dcm", "tag2", 25),
            };

            _storageService.Setup(s => s.ListObjectsAsync(bucketName, outputDir, true, It.IsAny<CancellationToken>())).ReturnsAsync(returnedFiles);

            var files = await DicomService.GetDicomPathsForTaskAsync(outputDir, bucketName);

            files.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetDicomPathsForTask_MultipleNonDicom_ReturnsEmptyList()
        {
            var bucketName = "bucket";
            var outputDir = "output/dir";

            var expected = new List<string>();

            var returnedFiles = new List<VirtualFileInfo>
            {
                new VirtualFileInfo("filename", "/test/folder/dicom.txt", "tag", 500),
                new VirtualFileInfo("filename", "/dicom2.json", "tag2", 25),
            };

            _storageService.Setup(s => s.ListObjectsAsync(bucketName, outputDir, true, It.IsAny<CancellationToken>())).ReturnsAsync(returnedFiles);

            var files = await DicomService.GetDicomPathsForTaskAsync(outputDir, bucketName);

            files.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetPayloadPatientDetails_ValidPayloadIdAndBucket_ReturnsValues()
        {
            var bucketName = "bucket";
            var payloadId = Guid.NewGuid().ToString();

            var expected = new PatientDetails
            {
                PatientName = "Jack",
                PatientId = "patientid",
                PatientSex = "Male",
                PatientDob = new DateTime(1996, 01, 20),
                PatientAge = "25",
                PatientHospitalId = "hospitalid"
            };

            var returnedFiles = new List<VirtualFileInfo>
            {
                new VirtualFileInfo("dicom.dcm.json", $"{payloadId}/dcm/folder/dicom.dcm.json", "tag", 500),
                new VirtualFileInfo("dicom2.dcm", $"{payloadId}/dcm/dicom2.dcm", "tag2", 25),
            };

            var fileContents = new Dictionary<string, DicomValue>
            {
                { DicomTagConstants.PatientNameTag, new DicomValue{ Value = new object[] { "Jack" }, Vr = "RR" } },
                { DicomTagConstants.PatientSexTag, new DicomValue{ Value = new object[] { "Male" }, Vr = "RR" } },
                { DicomTagConstants.PatientIdTag, new DicomValue{ Value = new object[] { "patientid" }, Vr = "RR" } },
                { DicomTagConstants.PatientDateOfBirthTag, new DicomValue{ Value = new object[] { "19960120" }, Vr = "RR" } },
                { DicomTagConstants.PatientAgeTag, new DicomValue{ Value = new object[] { "25" }, Vr = "RR" } },
                { DicomTagConstants.PatientHospitalIdTag, new DicomValue{ Value = new object[] { "hospitalid" }, Vr = "RR" } }
            };

            var jsonStr = JsonConvert.SerializeObject(fileContents);
            var byteArray = Encoding.UTF8.GetBytes(jsonStr);
            var stream = new MemoryStream(byteArray);

            _storageService.Setup(s => s.ListObjectsAsync(bucketName, $"{payloadId}/dcm", true, It.IsAny<CancellationToken>())).ReturnsAsync(returnedFiles);
            _storageService.Setup(s => s.GetObjectAsync(bucketName, $"{payloadId}/dcm/folder/dicom.dcm.json", It.IsAny<CancellationToken>())).ReturnsAsync(stream);

            var result = await DicomService.GetPayloadPatientDetailsAsync(payloadId, bucketName);

            result.Should().BeEquivalentTo(expected);
        }
    }
}
