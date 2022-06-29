// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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
            Assert.ThrowsAsync<ArgumentNullException>(async () => await DicomService.GetDicomPathsForTaskAsync(null, null));
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
                PatientDob = new DateTime(1996, 01, 20)
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
                { DicomTagConstants.PatientDateOfBirthTag, new DicomValue{ Value = new object[] { new DateTime(1996, 01, 20).ToString() }, Vr = "RR" } }
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
